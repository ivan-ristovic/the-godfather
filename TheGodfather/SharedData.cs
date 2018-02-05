#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Services;
using TheGodfather.Helpers;
using TheGodfather.Helpers.Collections;
using TheGodfather.Modules.Gambling.Cards;

using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather
{
    public sealed class SharedData
    {
        public ConcurrentDictionary<ulong, string> GuildPrefixes { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<Regex>> GuildFilters { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>> GuildTextReactions { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>> GuildEmojiReactions { get; internal set; }
        public ConcurrentDictionary<ulong, ulong> MessageCount { get; internal set; }
        public ConcurrentDictionary<ulong, Deck> CardDecks { get; internal set; }
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

        private BotConfig _cfg { get; }


        public SharedData(BotConfig cfg,
                          ConcurrentDictionary<ulong, string> gp,
                          ConcurrentDictionary<ulong, ConcurrentHashSet<Regex>> gf,
                          ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>> gtt,
                          ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>> ger,
                          ConcurrentDictionary<ulong, ulong> msgcount)
        {
            _cfg = cfg;
            GuildPrefixes = gp;
            GuildFilters = gf;
            GuildTextReactions = gtt;
            GuildEmojiReactions = ger;
            MessageCount = msgcount;
        }


        #region FILTERS
        public IReadOnlyCollection<Regex> GetFiltersForGuild(ulong gid)
        {
            if (GuildFilters.ContainsKey(gid) && GuildFilters[gid] != null)
                return GuildFilters[gid];
            else
                return null;
        }

        public bool ContainsFilter(ulong gid, string message)
        {
            if (!GuildFilters.ContainsKey(gid) || GuildFilters[gid] == null)
                return false;

            message = message.ToLower();
            return GuildFilters[gid].Any(f => f.Match(message).Success);
        }

        public bool TryAddGuildFilter(ulong gid, Regex regex)
        {
            if (GuildFilters.ContainsKey(gid)) {
                if (GuildFilters[gid] == null)
                    GuildFilters[gid] = new ConcurrentHashSet<Regex>();
            } else {
                if (!GuildFilters.TryAdd(gid, new ConcurrentHashSet<Regex>()))
                    return false;
            }

            if (GuildFilters[gid].Any(r => r.ToString() == regex.ToString()))
                return false;

            return GuildFilters[gid].Add(regex);
        }

        public bool TryRemoveGuildFilter(ulong gid, string filter)
        {
            if (!GuildFilters.ContainsKey(gid))
                return false;

            var rstr = $@"\b{filter}\b";
            return GuildFilters[gid].RemoveWhere(r => r.ToString() == rstr) > 0;
        }

        public void ClearGuildFilters(ulong gid)
        {
            if (!GuildFilters.ContainsKey(gid))
                return;

            GuildFilters[gid].Clear();
        }
        #endregion

        #region PREFIXES
        public string GetGuildPrefix(ulong gid)
        {
            if (GuildPrefixes.ContainsKey(gid) && !string.IsNullOrWhiteSpace(GuildPrefixes[gid]))
                return GuildPrefixes[gid];
            else
                return _cfg.DefaultPrefix;
        }

        public bool TrySetGuildPrefix(ulong gid, string prefix)
        {
            if (GuildPrefixes.ContainsKey(gid)) {
                GuildPrefixes[gid] = prefix;
                return true;
            } else {
                return GuildPrefixes.TryAdd(gid, prefix);
            }
        }
        #endregion

        #region RANKS
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

        public int GetRankForId(ulong uid)
        {
            if (MessageCount.ContainsKey(uid))
                return GetRankForMessageCount(MessageCount[uid]);
            else
                return 0;
        }

        public ulong GetMessageCountForId(ulong uid)
        {
            if (MessageCount.ContainsKey(uid))
                return MessageCount[uid];
            else
                return 0;
        }

        public int GetRankForMessageCount(ulong msgcount)
        {
            return (int)Math.Floor(Math.Sqrt(msgcount / 10));
        }

        public uint XpNeededForRankWithIndex(int index)
        {
            return (uint)(index * index * 10);
        }

        public async Task SaveRanksToDatabaseAsync(DatabaseService db)
        {
            //lock (_rankLock) {
                foreach (var entry in MessageCount)
                    await db.UpdateMessageCountForUserAsync(entry.Key, entry.Value).ConfigureAwait(false);
            //}
        }
        #endregion

        #region REACTIONS
        public IReadOnlyDictionary<string, string> GetAllGuildEmojiReactions(ulong gid)
        {
            if (GuildEmojiReactions.ContainsKey(gid) && GuildEmojiReactions[gid] != null)
                return GuildEmojiReactions[gid];
            else
                return null;
        }

        public IReadOnlyList<DiscordEmoji> GetEmojisForEmojiReaction(DiscordClient client, ulong gid, string message)
        {
            var emojis = new List<DiscordEmoji>();

            if (GuildEmojiReactions.ContainsKey(gid) && GuildEmojiReactions[gid] != null) {
                foreach (var word in message.ToLower().Split(' ')) {
                    if (GuildEmojiReactions[gid].ContainsKey(word)) {
                        try {
                            emojis.Add(DiscordEmoji.FromName(client, GuildEmojiReactions[gid][word]));
                        } catch (ArgumentException) {
                            client.DebugLogger.LogMessage(LogLevel.Warning, "TheGodfather", "Emoji name is not valid!", DateTime.Now);
                        }
                    }
                }
            }

            return emojis.AsReadOnly();
        }

        public bool TryAddGuildEmojiReaction(ulong gid, DiscordEmoji emoji, string[] triggers)
        {
            if (GuildEmojiReactions.ContainsKey(gid)) {
                if (GuildEmojiReactions[gid] == null)
                    GuildEmojiReactions[gid] = new ConcurrentDictionary<string, string>();
            } else {
                if (!GuildEmojiReactions.TryAdd(gid, new ConcurrentDictionary<string, string>()))
                    return false;
            }

            bool conflict_exists = false;
            foreach (var trigger in triggers.Select(t => t.ToLower())) {
                if (string.IsNullOrWhiteSpace(trigger))
                    continue;
                if (GuildEmojiReactions[gid].ContainsKey(trigger))
                    conflict_exists = true;
                else
                    conflict_exists |= !GuildEmojiReactions[gid].TryAdd(trigger, emoji.GetDiscordName());
            }

            return !conflict_exists;
        }

        public bool TryRemoveGuildEmojiReactions(ulong gid, string[] triggers)
        {
            if (!GuildEmojiReactions.ContainsKey(gid))
                return true;

            bool conflict_found = false;
            foreach (var trigger in triggers) {
                if (string.IsNullOrWhiteSpace(trigger))
                    continue;
                if (GuildEmojiReactions[gid].ContainsKey(trigger))
                    conflict_found |= !GuildEmojiReactions[gid].TryRemove(trigger, out _);
                else
                    conflict_found = true;
            }

            return !conflict_found;
        }

        public void DeleteAllGuildEmojiReactions(ulong gid)
        {
            if (!GuildEmojiReactions.ContainsKey(gid))
                return;

            GuildEmojiReactions[gid].Clear();
        }
        #endregion

        #region TRIGGERS
        public IReadOnlyDictionary<string, string> GetAllGuildTextReactions(ulong gid)
        {
            if (GuildTextReactions.ContainsKey(gid) && GuildTextReactions[gid] != null)
                return GuildTextReactions[gid];
            else
                return null;
        }

        public bool TextTriggerExists(ulong gid, string trigger)
        {
            return GuildTextReactions.ContainsKey(gid) && GuildTextReactions[gid] != null && GuildTextReactions[gid].ContainsKey(trigger);
        }

        public string GetResponseForTextReaction(ulong gid, string trigger)
        {
            trigger = trigger.ToLower();
            if (TextTriggerExists(gid, trigger))
                return GuildTextReactions[gid][trigger];
            else
                return null;
        }

        public bool TryAddGuildTextTrigger(ulong gid, string trigger, string response)
        {
            trigger = trigger.ToLower();
            if (GuildTextReactions.ContainsKey(gid)) {
                if (GuildTextReactions[gid] == null)
                    GuildTextReactions[gid] = new ConcurrentDictionary<string, string>();
            } else {
                if (!GuildTextReactions.TryAdd(gid, new ConcurrentDictionary<string, string>()))
                    return false;
            }

            return GuildTextReactions[gid].TryAdd(trigger, response);
        }

        public bool TryRemoveGuildTriggers(ulong gid, string[] triggers)
        {
            if (!GuildTextReactions.ContainsKey(gid))
                return true;

            bool conflict_found = false;
            foreach (var trigger in triggers) {
                if (string.IsNullOrWhiteSpace(trigger))
                    continue;
                if (GuildTextReactions[gid].ContainsKey(trigger))
                    conflict_found |= !GuildTextReactions[gid].TryRemove(trigger, out _);
                else
                    conflict_found = true;
            }

            return !conflict_found;
        }

        public void DeleteAllGuildTextReactions(ulong gid)
        {
            if (!GuildTextReactions.ContainsKey(gid))
                return;

            GuildTextReactions[gid].Clear();
        }
        #endregion
    }
}
