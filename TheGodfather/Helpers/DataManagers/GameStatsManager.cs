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
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers.DataManagers
{
    public class GameStatsManager
    {
        private static ConcurrentDictionary<ulong, GameStats> _stats = new ConcurrentDictionary<ulong, GameStats>();
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

        public bool UpdateDuelsWonForUser(ulong uid)
        {
            if (_stats.ContainsKey(uid)) {
                _stats[uid].DuelsWon++;
                return true;
            } else {
                return _stats.TryAdd(uid, new GameStats() { DuelsWon = 1 });
            }
        }

        public bool UpdateDuelsLostForUser(ulong uid)
        {
            if (_stats.ContainsKey(uid)) {
                _stats[uid].DuelsLost++;
                return true;
            } else {
                return _stats.TryAdd(uid, new GameStats() { DuelsLost = 1 });
            }
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

            em.AddField($"Duel stats", $"Won: {_stats[u.Id].DuelsWon}, Lost: {_stats[u.Id].DuelsLost}, Percentage: {Math.Round((double)_stats[u.Id].DuelsWon / (_stats[u.Id].DuelsWon + _stats[u.Id].DuelsLost) * 100)}%");

            return em.Build();
        }

        public async Task<DiscordEmbed> GetLeaderboardAsync(DiscordClient client)
        {
            var em = new DiscordEmbedBuilder() {
                Title = "Game leaderboard",
                Color = DiscordColor.Chartreuse
            };

            string desc;

            var topDuelists = _stats.OrderBy(kvp => kvp.Value.DuelsWon).Select(kvp => kvp.Key).Take(5);
            desc = "";
            foreach (var uid in topDuelists) {
                var u = await client.GetUserAsync(uid); // catch if user no longer exists
                desc += $"{Formatter.Bold(u.Username)} => Won: {_stats[uid].DuelsWon}; Lost: {_stats[uid].DuelsLost}\n"; 
            }
            em.AddField("Top 5 in Duel game:", desc);

            return em.Build();
        }
    }
}
