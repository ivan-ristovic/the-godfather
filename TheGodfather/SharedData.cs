#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Entities;
using TheGodfather.Extensions.Collections;
using TheGodfather.Modules.Gambling.Cards;
using TheGodfather.Services;

using DSharpPlus;
#endregion

namespace TheGodfather
{
    public sealed class SharedData
    {
        public CancellationTokenSource CTS { get; set; }
        public ConcurrentHashSet<ulong> BlockedUsers { get; set; } = new ConcurrentHashSet<ulong>();
        public ConcurrentHashSet<ulong> BlockedChannels { get; set; } = new ConcurrentHashSet<ulong>();
        public BotConfig BotConfiguration { get; internal set; }
        public ConcurrentDictionary<ulong, Deck> CardDecks { get; internal set; } = new ConcurrentDictionary<ulong, Deck>();
        public ConcurrentDictionary<ulong, string> GuildPrefixes { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<Regex>> GuildFilters { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<(Regex, string)>> GuildTextReactions { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentDictionary<string, ConcurrentHashSet<Regex>>> GuildEmojiReactions { get; internal set; }
        public ConcurrentDictionary<ulong, ulong> MessageCount { get; internal set; }
        public ConcurrentDictionary<int, SavedTaskExecuter> SavedTasks { get; set; } = new ConcurrentDictionary<int, SavedTaskExecuter>();
        public bool StatusRotationEnabled { get; set; } = true;
        public ConcurrentHashSet<ulong> UserIDsCheckingForSpace = new ConcurrentHashSet<ulong>();
        public IReadOnlyList<string> Ranks = new List<string>() {
            #region RANKS
            // If you make more than 25 ranks, then fix the embed
            "4U donor",
            "SoH MNG",
            "Cheap gypsy",
            "Romanian wallet stealer",
            "Romanian car cracker",
            "Serbian street cleaner",
            "German closet cleaner",
            "Swed's beer supplier",
            "JoJo's harem cleaner",
            "Torq's nurse",
            "Expensive gypsy",
            "Pakistani bomb carrier",
            "Michal's worker (black)",
            "Michal's worker (white)",
            "World Mafia Waste",
            "KF's goat",
            "Legendary Seagull Master",
            "Brazillian flip-flop maker",
            "The Global Elite Silver",
            "LDR",
            "Generalissimo (tribute to Raptor)"
            #endregion
        }.AsReadOnly();
        

        public string GetGuildPrefix(ulong gid)
        {
            if (GuildPrefixes.ContainsKey(gid) && !string.IsNullOrWhiteSpace(GuildPrefixes[gid]))
                return GuildPrefixes[gid];
            else
                return BotConfiguration.DefaultPrefix;
        }
        
        public ulong GetMessageCountForId(ulong uid)
            => MessageCount.ContainsKey(uid) ? MessageCount[uid] : 0;

        public int GetRankForMessageCount(ulong msgcount)
            => (int)Math.Floor(Math.Sqrt(msgcount / 10));

        public int GetRankForUser(ulong uid)
            => MessageCount.ContainsKey(uid) ? GetRankForMessageCount(MessageCount[uid]) : 0;

        public bool MessageContainsFilter(ulong gid, string message)
        {
            if (!GuildFilters.ContainsKey(gid) || GuildFilters[gid] == null)
                return false;

            message = message.ToLowerInvariant();
            return GuildFilters[gid].Any(f => f.IsMatch(message));
        }

        public async Task SaveRanksToDatabaseAsync(DBService db)
        {
            foreach (var entry in MessageCount)
                await db.UpdateMessageCountForUserAsync(entry.Key, entry.Value).ConfigureAwait(false);
        }

        public bool TextTriggerExists(ulong gid, string trigger)
        {
            string regex = $@"\b{trigger}\b".ToLowerInvariant();
            return GuildTextReactions.ContainsKey(gid) && GuildTextReactions[gid] != null && GuildTextReactions[gid].Any(tup => tup.Item1.ToString() == regex);
        }

        public int UpdateMessageCount(ulong uid)
        {
            if (MessageCount.ContainsKey(uid)) {
                MessageCount[uid]++;
            } else if (!MessageCount.TryAdd(uid, 1)) {
                return -1;
            }

            int curr = GetRankForMessageCount(MessageCount[uid]);
            int prev = GetRankForMessageCount(MessageCount[uid] - 1);

            return curr != prev ? curr : -1;
        }

        public uint XpNeededForRankWithIndex(int index)
            => (uint)(index * index * 10);

        public async Task SyncDataWithDatabaseAsync(DBService db)
        {
            try {
                await SaveRanksToDatabaseAsync(db)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                Logger.LogException(LogLevel.Error, e);
            }
        }

        public void Dispose()
        {
            foreach (var kvp in SavedTasks)
                kvp.Value.Dispose();
        }
    }
}
