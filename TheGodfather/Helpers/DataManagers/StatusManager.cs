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
    public class StatusManager
    {
        public IReadOnlyList<string> Statuses => _statuses;
        private List<string> _statuses = new List<string> { "!help", "worldmafia.net", "worldmafia.net/discord" };
        private bool _ioerr = false;


        public StatusManager()
        {

        }


        public void Load()
        {
            if (File.Exists("Resources/statuses.json")) {
                try {
                    _statuses = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("Resources/statuses.json"));
                } catch (Exception e) {
                    Console.WriteLine("Status loading error, check file formatting. Details:\n" + e.ToString());
                    _ioerr = true;
                }
            } else {
                Console.WriteLine("statuses.json is missing.");
            }
        }

        public bool Save()
        {
            if (_ioerr) {
                Console.WriteLine("Status saving skipped until file conflicts are resolved!");
                return false;
            }

            try {
                File.WriteAllText("Resources/statuses.json", JsonConvert.SerializeObject(_statuses, Formatting.Indented));
            } catch (Exception e) {
                Console.WriteLine("IO Status save error. Details:\n" + e.ToString());
                return false;
            }

            return true;
        }

        public void AddStatus(string status)
        {
            if (_statuses.Contains(status))
                return;
            _statuses.Add(status);
        }

        public void DeleteStatus(string status)
        {
            _statuses.RemoveAll(s => s.ToLower() == status.ToLower());
        }
        
        public string GetRandomStatus()
        {
            return Statuses[new Random().Next(Statuses.Count)];
        }
    }
}
