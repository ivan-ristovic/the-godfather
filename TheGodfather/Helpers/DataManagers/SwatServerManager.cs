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
    public class SwatServerManager
    {
        public IReadOnlyDictionary<string, string> Servers => _serverlist;
        private ConcurrentDictionary<string, string> _serverlist = new ConcurrentDictionary<string, string>();
        private bool _ioerr = false;


        public SwatServerManager()
        {

        }


        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/servers.json")) {
                try {
                    _serverlist = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(File.ReadAllText("Resources/servers.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Servers loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerr = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "servers.json is missing.", DateTime.Now);
            }
        }

        public bool Save(DebugLogger log)
        {
            if (_ioerr) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Server saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/servers.json", JsonConvert.SerializeObject(_serverlist, Formatting.Indented));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Server save error. Details:\n" + e.ToString(), DateTime.Now);
                return false;
            }

            return true;
        }

        public bool TryAdd(string name, string ip)
        {
            return _serverlist.TryAdd(name, ip);
        }

        public bool TryRemove(string name)
        {
            if (!_serverlist.ContainsKey(name))
                return true;

            return _serverlist.TryRemove(name, out _);
        }
    }
}
