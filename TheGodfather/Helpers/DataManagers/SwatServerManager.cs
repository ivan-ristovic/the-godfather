#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json;

using TheGodfather.Helpers.Swat;

using DSharpPlus;
#endregion

namespace TheGodfather.Helpers.DataManagers
{
    public class SwatServerManager
    {
        public IReadOnlyDictionary<string, SwatServer> Servers => _serverlist;
        private ConcurrentDictionary<string, SwatServer> _serverlist = new ConcurrentDictionary<string, SwatServer>();
        private bool _ioerr = false;


        public SwatServerManager()
        {

        }


        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/servers.json")) {
                try {
                    _serverlist = JsonConvert.DeserializeObject<ConcurrentDictionary<string, SwatServer>>(File.ReadAllText("Resources/servers.json"));
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

        public bool TryAdd(string name, SwatServer server)
        {
            return _serverlist.TryAdd(name.ToLower(), server);
        }

        public bool TryRemove(string name)
        {
            if (!_serverlist.ContainsKey(name))
                return true;

            return _serverlist.TryRemove(name, out _);
        }

        public SwatServer GetServer(string ip, int queryport, string name = null)
        {
            ip = ip.ToLower();
            int joinport = 10480;

            if (_serverlist.ContainsKey(ip))
                return _serverlist[ip];

            var split = ip.Split(':');
            ip = split[0];
            if (split.Length > 1) {
                try {
                    joinport = int.Parse(split[1]);
                } catch (FormatException) {
                    joinport = 10480;
                }
            }
            if (queryport == 10481)
                queryport = joinport + 1;

            return new SwatServer(name, ip, joinport, queryport);
        }
    }
}
