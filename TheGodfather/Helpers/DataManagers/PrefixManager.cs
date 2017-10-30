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
    public class PrefixManager
    {
        public IReadOnlyDictionary<ulong, string> Prefixes => _prefixes;
        private ConcurrentDictionary<ulong, string> _prefixes = new ConcurrentDictionary<ulong, string>();
        private bool _ioerr = false;


        public PrefixManager()
        {

        }


        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/prefixes.json")) {
                try {
                    _prefixes = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, string>>(File.ReadAllText("Resources/prefixes.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Prefix loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerr = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "prefixes.json is missing.", DateTime.Now);
            }
        }

        public bool Save(DebugLogger log)
        {
            if (_ioerr) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Prefix saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/prefixes.json", JsonConvert.SerializeObject(_prefixes, Formatting.Indented));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Prefix save error. Details:\n" + e.ToString(), DateTime.Now);
                return false;
            }

            return true;
        }

        public string GetPrefixForChannelId(ulong cid)
        {
            if (_prefixes.ContainsKey(cid))
                return _prefixes[cid];
            else
                return null;
        }

        public void SetPrefixForChannelId(ulong cid, string prefix)
        {
            if (_prefixes.ContainsKey(cid))
                _prefixes[cid] = prefix;
            _prefixes.TryAdd(cid, prefix);
        }
    }
}
