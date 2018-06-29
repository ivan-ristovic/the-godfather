#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Modules.Reactions.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.Entities;

using TexasHoldem.Logic.Cards;
#endregion

namespace TheGodfather
{
    public sealed class SharedData
    {
        public ConcurrentHashSet<ulong> BlockedUsers { get; internal set; } = new ConcurrentHashSet<ulong>();
        public ConcurrentHashSet<ulong> BlockedChannels { get; internal set; } = new ConcurrentHashSet<ulong>();
        public BotConfig BotConfiguration { get; internal set; }
        public ConcurrentDictionary<ulong, CachedGuildConfig> GuildConfigurations { get; internal set; }
        public ConcurrentDictionary<ulong, Deck> CardDecks { get; internal set; } = new ConcurrentDictionary<ulong, Deck>();
        public CancellationTokenSource CTS { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>> Filters { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>> TextReactions { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>> EmojiReactions { get; internal set; }
        public ConcurrentDictionary<ulong, ulong> MessageCount { get; internal set; }
        public ConcurrentDictionary<int, SavedTaskExecuter> SavedTasks { get; internal set; } = new ConcurrentDictionary<int, SavedTaskExecuter>();
        public bool StatusRotationEnabled { get; internal set; } = true;
        public ConcurrentDictionary<ulong, CancellationTokenSource> SpaceCheckingCTS = new ConcurrentDictionary<ulong, CancellationTokenSource>();
        public ConcurrentDictionary<ulong, MusicPlayer> MusicPlayers = new ConcurrentDictionary<ulong, MusicPlayer>();
        public ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>> AwaitingUsersInteractively = new ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>>();


        public CachedGuildConfig GetGuildConfig(ulong gid)
            => GuildConfigurations.ContainsKey(gid) ? GuildConfigurations[gid] : CachedGuildConfig.Default;

        public DiscordChannel GetLogChannelForGuild(DiscordClient client, DiscordGuild guild)
        {
            var gcfg = GetGuildConfig(guild.Id);
            return gcfg.LoggingEnabled ? guild.GetChannel(gcfg.LogChannelId) : null;
        }

        public void AddAwaitingUser(ulong cid, ulong uid)
        {
            AwaitingUsersInteractively.AddOrUpdate(
                cid,
                new ConcurrentHashSet<ulong> { uid },
                (k, v) => { v.Add(uid); return v; }
            );
        }

        public void RemoveAwaitingUser(ulong cid, ulong uid)
        {
            AwaitingUsersInteractively[cid].TryRemove(uid);
            if (AwaitingUsersInteractively[cid].Count == 0)
                AwaitingUsersInteractively.TryRemove(cid, out _);
        }

        public string GetGuildPrefix(ulong gid)
        {
            if (GuildConfigurations.ContainsKey(gid) && !string.IsNullOrWhiteSpace(GuildConfigurations[gid].Prefix))
                return GuildConfigurations[gid].Prefix;
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
            if (!Filters.ContainsKey(gid) || Filters[gid] == null)
                return false;

            message = message.ToLowerInvariant();
            return Filters[gid].Any(f => f.Trigger.IsMatch(message));
        }

        public bool TextTriggerExists(ulong gid, string trigger)
        {
            return TextReactions.ContainsKey(gid) && TextReactions[gid] != null && TextReactions[gid].Any(tr => tr.ContainsTriggerPattern(trigger));
        }

        public bool EmojiTriggerExists(ulong gid, string trigger)
        {
            return EmojiReactions.ContainsKey(gid) && EmojiReactions[gid] != null && EmojiReactions[gid].Any(er => er.ContainsTriggerPattern(trigger));
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
                TheGodfather.LogHandle.LogException(LogLevel.Error, e);
            }
        }

        private async Task SaveRanksToDatabaseAsync(DBService db)
        {
            foreach (var entry in MessageCount)
                await db.UpdateExperienceForUserAsync(entry.Key, entry.Value).ConfigureAwait(false);
        }

        public void Dispose()
        {
            foreach (var kvp in SavedTasks)
                kvp.Value.Dispose();
        }
    }
}
