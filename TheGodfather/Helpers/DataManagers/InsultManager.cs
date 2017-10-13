#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
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
    public class InsultManager
    {
        public IReadOnlyList<string> Insults => _insults;
        private List<string> _insults = new List<string>();
        private readonly object _lock = new object();
        private bool _ioerr = false;


        public InsultManager()
        {

        }


        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/insults.json")) {
                try {
                    _insults = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("Resources/insults.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Insult loading error, clearing insults. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerr = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "insults.txt is missing.", DateTime.Now);
            }
        }

        public bool Save(DebugLogger log)
        {
            if (_ioerr) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Insult saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/insults.json", JsonConvert.SerializeObject(_insults));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO insults save error. Details:\n" + e.ToString(), DateTime.Now);
                return false;
            }

            return true;
        }

        public string GetRandomInsult()
        {
            if (_insults.Count == 0)
                return null;

            return _insults[new Random().Next(_insults.Count)];
        }

        public void Add(string insult)
        {
            lock (_lock) {
                _insults.Add(insult);
            }
        }

        public bool RemoveAt(int index)
        {
            lock (_lock) {
                if (index < 0 || index > _insults.Count)
                    return false;
                _insults.RemoveAt(index);
            }
            return true;
        }

        public void ClearInsults()
        {
            lock (_lock) {
                _insults.Clear();
            }
        }
    }
}
