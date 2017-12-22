#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers.DataManagers
{
    public class GuildConfigManager
    {
        #region PRIVATE_FIELDS
        private ConcurrentDictionary<ulong, GuildConfig> _gcfg = new ConcurrentDictionary<ulong, GuildConfig>();
        private BotConfig _cfg { get; set; }
        private bool _ioerr = false;
        private readonly object _filterLock = new object();
        #endregion


        public GuildConfigManager(BotConfig cfg)
        {
            _cfg = cfg;
        }


        #region LOAD/SAVE
        public void Load()
        {
            if (File.Exists("Resources/guilds.json")) {
                try {
                    _gcfg = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, GuildConfig>>(File.ReadAllText("Resources/guilds.json"));
                } catch (Exception e) {
                    Console.WriteLine("Guild config loading error, check file formatting. Details:\n" + e.ToString());
                    _ioerr = true;
                }
            } else {
                Console.WriteLine("guilds.json is missing.");
            }
        }

        public bool Save()
        {
            if (_ioerr) {
                Console.WriteLine("Guild info saving skipped until file conflicts are resolved!");
                return false;
            }

            try {
                File.WriteAllText("Resources/guilds.json", JsonConvert.SerializeObject(_gcfg, Formatting.Indented));
            } catch (Exception e) {
                Console.WriteLine("IO Guild info save error. Details:\n" + e.ToString());
                return false;
            }

            return true;
        }
        #endregion

        #region REACTIONS
        public IReadOnlyDictionary<string, string> GetAllGuildReactions(ulong gid)
        {
            if (_gcfg.ContainsKey(gid) && _gcfg[gid].Reactions != null)
                return _gcfg[gid].Reactions;
            else
                return null;
        }

        public IReadOnlyList<DiscordEmoji> GetReactionEmojis(DiscordClient client, ulong gid, string message)
        {
            var emojis = new List<DiscordEmoji>();

            if (_gcfg.ContainsKey(gid) && _gcfg[gid].Reactions != null) {
                foreach (var word in message.ToLower().Split(' ')) {
                    if (_gcfg[gid].Reactions.ContainsKey(word)) {
                        try {
                            emojis.Add(DiscordEmoji.FromName(client, _gcfg[gid].Reactions[word]));
                        } catch (ArgumentException) {
                            client.DebugLogger.LogMessage(LogLevel.Warning, "TheGodfather", "Emoji name is not valid!", DateTime.Now);
                        }
                    }
                }
            }

            return emojis;
        }

        public bool TryAddReaction(ulong gid, DiscordEmoji emoji, string[] triggers)
        {
            if (_gcfg.ContainsKey(gid)) {
                if (_gcfg[gid].Reactions == null)
                    _gcfg[gid].Reactions = new ConcurrentDictionary<string, string>();
            } else {
                if (!_gcfg.TryAdd(gid, new GuildConfig() { Reactions = new ConcurrentDictionary<string, string>() }))
                    return false;
            }

            bool conflict_exists = false;
            foreach (var trigger in triggers.Select(t => t.ToLower())) {
                if (string.IsNullOrWhiteSpace(trigger))
                    continue;
                if (_gcfg[gid].Reactions.ContainsKey(trigger))
                    conflict_exists = true;
                else
                    conflict_exists |= !_gcfg[gid].Reactions.TryAdd(trigger, emoji.GetDiscordName());
            }

            return !conflict_exists;
        }

        public bool TryRemoveReactions(ulong gid, string[] triggers)
        {
            if (!_gcfg.ContainsKey(gid))
                return true;

            bool conflict_found = false;
            foreach (var trigger in triggers) {
                if (string.IsNullOrWhiteSpace(trigger))
                    continue;
                if (_gcfg[gid].Reactions.ContainsKey(trigger))
                    conflict_found |= !_gcfg[gid].Reactions.TryRemove(trigger, out _);
                else
                    conflict_found = true;
            }

            return !conflict_found;
        }

        public void ClearGuildReactions(ulong gid)
        {
            if (!_gcfg.ContainsKey(gid))
                return;

            _gcfg[gid].Reactions.Clear();
        }
        #endregion

    }
}
