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
        private DiscordClient _client;
        private DatabaseService _db;


        public GameStatsManager(DiscordClient client, DatabaseService db)
        {
            _client = client;
            _db = db;
        }

        public async Task UpdateStatAsync(ulong uid, string stat)
        {
            await _db.UpdateStat(uid, stat, 1)
                   .ConfigureAwait(false);
        }

        public async Task<IReadOnlyDictionary<string, string>> GetStatsForUserAsync(ulong uid)
        {
            var stats = await _db.GetStatsForUserAsync(uid)
                .ConfigureAwait(false);
            return stats;
        }

        public async Task<DiscordEmbed> GetEmbeddedStatsForUserAsync(DiscordUser u)
        {
            var stats = await GetStatsForUserAsync(u.Id)
                .ConfigureAwait(false);

            if (stats == null) {
                return new DiscordEmbedBuilder() {
                    Title = $"Stats for {u.Username}",
                    Description = "No games played yet!",
                    ThumbnailUrl = u.AvatarUrl,
                    Color = DiscordColor.Chartreuse
                }.Build();
            }

            var eb = GameStats.GetEmbeddedStats(stats);
            eb.WithTitle($"Stats for {u.Username}");
            eb.WithThumbnailUrl(u.AvatarUrl);
            return eb.Build();
        }

        public async Task<DiscordEmbed> GetLeaderboardAsync()
        {
            return new DiscordEmbedBuilder().Build();
        }
    }
}
