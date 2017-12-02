#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Helpers;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Exceptions;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers.DataManagers
{
    public class GameStatsManager
    {
        private ConcurrentDictionary<ulong, GameStats> _stats = new ConcurrentDictionary<ulong, GameStats>();
        private bool _ioerr = false;


        public GameStatsManager()
        {

        }


        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/stats.json")) {
                try {
                    _stats = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, GameStats>>(File.ReadAllText("Resources/stats.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Game stats loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerr = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "stats.json is missing.", DateTime.Now);
            }
        }

        public bool Save(DebugLogger log)
        {
            if (_ioerr) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Game stats saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/stats.json", JsonConvert.SerializeObject(_stats, Formatting.Indented));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Game stats save error. Details:\n" + e.ToString(), DateTime.Now);
                return false;
            }

            return true;
        }

        public void UpdateDuelsWonForUser(ulong uid)
        {
            _stats.AddOrUpdate(uid, new GameStats() { DuelsWon = 1 }, (k, v) => { v.DuelsWon++; return v; });
        }

        public void UpdateDuelsLostForUser(ulong uid)
        {
            _stats.AddOrUpdate(uid, new GameStats() { DuelsLost = 1 }, (k, v) => { v.DuelsLost++; return v; });
        }

        public void UpdateNunchiGamesWonForUser(ulong uid)
        {
            _stats.AddOrUpdate(uid, new GameStats() { NunchiGamesWon = 1 }, (k, v) => { v.NunchiGamesWon++; return v; });
        }

        public void UpdateQuizesWonForUser(ulong uid)
        {
            _stats.AddOrUpdate(uid, new GameStats() { QuizesWon = 1 }, (k, v) => { v.QuizesWon++; return v; });
        }

        public void UpdateRacesWonForUser(ulong uid)
        {
            _stats.AddOrUpdate(uid, new GameStats() { RacesWon = 1 }, (k, v) => { v.RacesWon++; return v; });
        }

        public void UpdateHangmanWonForUser(ulong uid)
        {
            _stats.AddOrUpdate(uid, new GameStats() { HangmanWon = 1 }, (k, v) => { v.HangmanWon++; return v; });
        }

        public GameStats GetStatsForUser(ulong uid)
        {
            if (_stats.ContainsKey(uid))
                return _stats[uid];
            else
                return null;
        }

        public DiscordEmbed GetEmbeddedStatsForUser(DiscordUser u)
        {
            var em = new DiscordEmbedBuilder() {
                Title = $"Stats for {u.Username}",
                Color = DiscordColor.Chartreuse
            };

            if (!_stats.ContainsKey(u.Id)) {
                em.WithDescription("No games played yet!");
                return em.Build();
            }

            em.AddField($"Duel stats", $"Won: {_stats[u.Id].DuelsWon}, Lost: {_stats[u.Id].DuelsLost}, Percentage: {_stats[u.Id].DuelWinPercentage}%");
            em.AddField($"Nunchi stats", $"Won: {_stats[u.Id].NunchiGamesWon}", inline: true);
            em.AddField($"Quiz stats", $"Won: {_stats[u.Id].QuizesWon}", inline: true);
            em.AddField($"Race stats", $"Won: {_stats[u.Id].RacesWon}", inline: true);
            em.AddField($"Hangman stats", $"Won: {_stats[u.Id].HangmanWon}", inline: true);

            return em.Build();
        }

        public async Task<DiscordEmbed> GetLeaderboardAsync(DiscordClient client)
        {
            var em = new DiscordEmbedBuilder() {
                Title = "Game leaderboard",
                Color = DiscordColor.Chartreuse
            };

            string desc;

            var topDuelists = _stats.OrderByDescending(kvp => kvp.Value.DuelsWon).Select(kvp => kvp.Key).Take(5);
            desc = "";
            foreach (var uid in topDuelists) {
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    desc += $"{Formatter.Bold(u.Username)} => Won: {_stats[uid].DuelsWon}; Lost: {_stats[uid].DuelsLost}\n";
                } catch (NotFoundException) {
                    continue;
                }
            }
            em.AddField("Top 5 in Duel game:", desc, inline: true);

            var topNunchiPlayers = _stats.OrderByDescending(kvp => kvp.Value.NunchiGamesWon).Select(kvp => kvp.Key).Take(5);
            desc = "";
            foreach (var uid in topNunchiPlayers) {
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    desc += $"{Formatter.Bold(u.Username)} => Won: {_stats[uid].NunchiGamesWon}\n";
                } catch (NotFoundException) {
                    continue;
                }
            }
            em.AddField("Top 5 in Nunchi game:", desc, inline: true);

            var topQuizPlayers = _stats.OrderByDescending(kvp => kvp.Value.QuizesWon).Select(kvp => kvp.Key).Take(5);
            desc = "";
            foreach (var uid in topQuizPlayers) {
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    desc += $"{Formatter.Bold(u.Username)} => Won: {_stats[uid].QuizesWon}\n";
                } catch (NotFoundException) {
                    continue;
                }
            }
            em.AddField("Top 5 in Quiz game:", desc, inline: true);

            var topRacePlayers = _stats.OrderByDescending(kvp => kvp.Value.RacesWon).Select(kvp => kvp.Key).Take(5);
            desc = "";
            foreach (var uid in topRacePlayers) {
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    desc += $"{Formatter.Bold(u.Username)} => Won: {_stats[uid].RacesWon}\n";
                } catch (NotFoundException) {
                    continue;
                }
            }
            em.AddField("Top 5 in Race game:", desc, inline: true);

            var topHangmanPlayers = _stats.OrderByDescending(kvp => kvp.Value.HangmanWon).Select(kvp => kvp.Key).Take(5);
            desc = "";
            foreach (var uid in topHangmanPlayers) {
                try {
                    var u = await client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    desc += $"{Formatter.Bold(u.Username)} => Won: {_stats[uid].HangmanWon}\n";
                } catch (NotFoundException) {
                    continue;
                }
            }
            em.AddField("Top 5 in Hangman game:", desc, inline: true);

            return em.Build();
        }
    }
}
