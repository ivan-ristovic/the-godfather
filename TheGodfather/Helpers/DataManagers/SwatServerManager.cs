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
            string[] serverlist = {
                "wm$46.251.251.9:10880:10881",
                "myt$51.15.152.220:10480:10481",
                "4u$109.70.149.161:10480:10481",
                "soh$158.58.173.64:16480:10481",
                "sh$5.9.50.39:8480:8481",
                "esa$77.250.71.231:11180:11181",
                "kos$31.186.250.32:10480:10481"
            };

            if (!File.Exists("Resources/servers.txt")) {
                FileStream f = File.Open("Resources/servers.txt", FileMode.CreateNew);
                f.Close();
                File.WriteAllLines("Resources/servers.txt", serverlist);
            }

            try {
                serverlist = File.ReadAllLines("Resources/servers.txt");
                foreach (string line in serverlist) {
                    if (line.Trim() == "" || line[0] == '#')
                        continue;
                    var values = line.Split('$');
                    _serverlist.TryAdd(values[0], values[1]);
                }
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "Serverlist loading error, clearing list. Details : " + e.ToString(), DateTime.Now);
                _ioerr = true;
            }
        }

        public bool Save(DebugLogger log)
        {
            if (_ioerr) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Servers saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                List<string> serverlist = new List<string>();
                foreach (var entry in _serverlist)
                    serverlist.Add(entry.Key + "$" + entry.Value);

                File.WriteAllLines("Resources/servers.txt", serverlist);
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "Servers save error: " + e.ToString(), DateTime.Now);
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
