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

namespace TheGodfather.Helpers
{
    public class AliasList
    {
        public IReadOnlyDictionary<ulong, SortedDictionary<string, string>> Aliases => _aliases;
        private ConcurrentDictionary<ulong, SortedDictionary<string, string>> _aliases = new ConcurrentDictionary<ulong, SortedDictionary<string, string>>();
        private bool _ioerr = false;


        public AliasList()
        {

        }

        
        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/aliases.json")) {
                try {
                    _aliases = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, SortedDictionary<string, string>>>(File.ReadAllText("Resources/aliases.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Alias loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerr = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "aliases.json is missing.", DateTime.Now);
            }
        }

        public bool Save(DebugLogger log)
        {
            if (_ioerr) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Alias saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/aliases.json", JsonConvert.SerializeObject(_aliases));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Alias save error. Details:\n" + e.ToString(), DateTime.Now);
                return false;
            }

            return true;
        }

        public string GetResponse(ulong gid, string trigger)
        {
            trigger = trigger.ToLower();
            if (_aliases.ContainsKey(gid) && _aliases[gid].ContainsKey(trigger))
                return _aliases[gid][trigger];
            else
                return null;
        }

        public bool TryAdd(ulong gid, string alias, string response)
        {
            alias = alias.ToLower();
            if (!_aliases.ContainsKey(gid))
                if (!_aliases.TryAdd(gid, new SortedDictionary<string, string>()))
                    return false;

            if (_aliases[gid].ContainsKey(alias))
                return false;

            _aliases[gid].Add(alias, response);
            return true;
        }

        public bool TryRemove(ulong gid, string alias)
        {
            if (_aliases.ContainsKey(gid) || _aliases[gid].ContainsKey(alias))
                return false;

            return _aliases[gid].Remove(alias);
        }

        public bool ClearGuildAliases(ulong gid)
        {
            return _aliases.TryRemove(gid, out _);
        }

        public void ClearAllAliases()
        {
            _aliases.Clear();
        }
    }
}
