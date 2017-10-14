#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers.DataManagers
{
    public class ReactionManager
    {
        public IReadOnlyDictionary<ulong, SortedDictionary<string, string>> Reactions => _reactions;
        private ConcurrentDictionary<ulong, SortedDictionary<string, string>> _reactions = new ConcurrentDictionary<ulong, SortedDictionary<string, string>>();
        private bool _ioerr = false;


        public ReactionManager()
        {

        }


        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/reactions.json")) {
                try {
                    _reactions = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, SortedDictionary<string, string>>>(File.ReadAllText("Resources/reactions.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Reaction loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerr = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "reactions.json is missing.", DateTime.Now);
            }
        }

        public bool Save(DebugLogger log)
        {
            if (_ioerr) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Reactions saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/reactions.json", JsonConvert.SerializeObject(_reactions));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "Reactions save error. Details:\n" + e.ToString(), DateTime.Now);
                return false;
            }

            return true;
        }

        public IReadOnlyList<DiscordEmoji> GetReactionEmojis(DiscordClient client, ulong gid, string message)
        {
            var emojis = new List<DiscordEmoji>();

            if (_reactions.ContainsKey(gid)) {
                foreach (var word in message.ToLower().Split(' ')) {
                    if (_reactions[gid].ContainsKey(word)) {
                        try {
                            emojis.Add(DiscordEmoji.FromName(client, _reactions[gid][word]));
                        } catch (ArgumentException) {
                            client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "Emoji name is not valid!", DateTime.Now);
                        }
                    }
                }
            }

            return emojis;
        }

        public bool TryAdd(ulong gid, DiscordEmoji emoji, string[] triggers)
        {
            if (!_reactions.ContainsKey(gid))
                if (!_reactions.TryAdd(gid, new SortedDictionary<string, string>()))
                    return false;

            bool conflict_exists = false;
            foreach (var word in triggers) {
                if (_reactions[gid].ContainsKey(word))
                    conflict_exists = true;
                else
                    _reactions[gid].Add(word, emoji.GetDiscordName());
            }

            return !conflict_exists;
        }

        public bool TryRemove(ulong gid, string[] triggers)
        {
            if (!_reactions.ContainsKey(gid))
                return false;

            bool found = true;
            foreach (var trigger in triggers) {
                if (!_reactions[gid].ContainsKey(trigger))
                    found = false;
                else
                    _reactions[gid].Remove(trigger);
            }

            return found;
        }

        public bool ClearGuildReactions(ulong gid)
        {
            if (!_reactions.ContainsKey(gid))
                return false;

            return _reactions.TryRemove(gid, out _);
        }

        public void ClearAllReactions()
        {
            _reactions.Clear();
        }
    }
}
