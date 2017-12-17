#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Services;

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
        private DiscordClient _client;
        private DatabaseService _db;


        public GameStatsManager(DiscordClient client, DatabaseService db)
        {
            _client = client;
            _db = db;
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

        public async Task UpdateStatAsync(ulong uid, string stat)
        {
            await _db.UpdateStat(uid, stat, 1)
                   .ConfigureAwait(false);
        }

        public GameStats GetStatsForUser(ulong uid)
        {
            //var stats = await _db.GetStatsForUserAsync(uid);
            if (_stats.ContainsKey(uid))
                return _stats[uid];
            else
                return null;
        }

        public DiscordEmbed GetEmbeddedStatsForUser(DiscordUser u)
        {
            if (!_stats.ContainsKey(u.Id)) {
                return new DiscordEmbedBuilder() {
                    Title = $"Stats for {u.Username}",
                    Description = "No games played yet!",
                    ThumbnailUrl = u.AvatarUrl,
                    Color = DiscordColor.Chartreuse
                }.Build();
            }

            var eb = _stats[u.Id].GetEmbeddedStats();
            eb.WithTitle($"Stats for {u.Username}");
            eb.WithThumbnailUrl(u.AvatarUrl);
            return eb.Build();
        }

        public async Task<DiscordEmbed> GetLeaderboardAsync()
        {
            var em = new DiscordEmbedBuilder() {
                Title = "HALL OF FAME",
                Color = DiscordColor.Chartreuse
            };

            string desc;

            desc = await StatSelectorAsync(5, 
                sorter:    kvp => kvp.Value.DuelWinPercentage,
                filter:    kvp => kvp.Value.DuelsWon > 0,
                formatter: uid => _stats[uid].DuelStatsString(),
                additionalSorter: kvp => kvp.Value.DuelsWon
            ).ConfigureAwait(false);
            em.AddField("Top 5 in Duel game:", string.IsNullOrWhiteSpace(desc) ? "No records" : desc , inline: true);

            desc = await StatSelectorAsync(5,
                sorter:    kvp => kvp.Value.TTTWinPercentage,
                filter:    kvp => kvp.Value.TTTWon > 0,
                formatter: uid => _stats[uid].TTTStatsString(),
                additionalSorter: kvp => kvp.Value.TTTWon
            ).ConfigureAwait(false);
            em.AddField("Top 5 in Tic-Tac-Toe game:", string.IsNullOrWhiteSpace(desc) ? "No records" : desc, inline: true);

            desc = await StatSelectorAsync(5,
                sorter:    kvp => kvp.Value.Connect4WinPercentage,
                filter:    kvp => kvp.Value.Connect4Won > 0,
                formatter: uid => _stats[uid].Connect4StatsString(),
                additionalSorter: kvp => kvp.Value.Connect4Won
            ).ConfigureAwait(false);
            em.AddField("Top 5 in Connect4 game:", string.IsNullOrWhiteSpace(desc) ? "No records" : desc, inline: true);

            desc = await StatSelectorAsync(5,
                sorter: kvp => kvp.Value.CaroWinPercentage,
                filter: kvp => kvp.Value.CaroWon > 0,
                formatter: uid => _stats[uid].CaroStatsString(),
                additionalSorter: kvp => kvp.Value.CaroWon
            ).ConfigureAwait(false);
            em.AddField("Top 5 in Caro game:", string.IsNullOrWhiteSpace(desc) ? "No records" : desc, inline: true);

            desc = await StatSelectorAsync(5,
                sorter:    kvp => kvp.Value.NunchiGamesWon,
                filter:    kvp => kvp.Value.NunchiGamesWon > 0,
                formatter: uid => _stats[uid].NunchiStatsString()
            ).ConfigureAwait(false);
            em.AddField("Top 5 in Nunchi game:", string.IsNullOrWhiteSpace(desc) ? "No records" : desc, inline: true);

            desc = await StatSelectorAsync(5,
                sorter:    kvp => kvp.Value.QuizesWon,
                filter:    kvp => kvp.Value.QuizesWon > 0,
                formatter: uid => _stats[uid].QuizStatsString()
            ).ConfigureAwait(false);
            em.AddField("Top 5 in Quiz game:", string.IsNullOrWhiteSpace(desc) ? "No records" : desc, inline: true);

            desc = await StatSelectorAsync(5,
                sorter:    kvp => kvp.Value.RacesWon,
                filter:    kvp => kvp.Value.RacesWon > 0,
                formatter: uid => _stats[uid].RaceStatsString()
            ).ConfigureAwait(false);
            em.AddField("Top 5 in Race game:", string.IsNullOrWhiteSpace(desc) ? "No records" : desc, inline: true);

            desc = await StatSelectorAsync(5,
                sorter:    kvp => kvp.Value.HangmanWon,
                filter:    kvp => kvp.Value.HangmanWon > 0,
                formatter: uid => _stats[uid].HangmanStatsString()
            ).ConfigureAwait(false);
            em.AddField("Top 5 in Hangman game:", string.IsNullOrWhiteSpace(desc) ? "No records" : desc, inline: true);

            return em.Build();
        }

        private async Task<string> StatSelectorAsync(int ammount, 
                                                     Func<KeyValuePair<ulong, GameStats>, uint> sorter,
                                                     Func<KeyValuePair<ulong, GameStats>, bool> filter,
                                                     Func<ulong, string> formatter,
                                                     Func<KeyValuePair<ulong, GameStats>, ulong> additionalSorter = null)
        {
            if (additionalSorter == null)
                additionalSorter = kvp => kvp.Key;

            var topuids = _stats.OrderByDescending(sorter)
                                .ThenByDescending(additionalSorter)
                                .Where(filter)
                                .Select(kvp => kvp.Key)
                                .Take(ammount);
            var stats = topuids.Select(formatter);
            var usernames = await Task.WhenAll(topuids.Select(uid => _client.GetUserAsync(uid)))
                .ConfigureAwait(false);
            return string.Join("\n", usernames.Zip(stats, (u, s) => $"{Formatter.Bold(u.Username)} > {s}"));
        }
    }
}
