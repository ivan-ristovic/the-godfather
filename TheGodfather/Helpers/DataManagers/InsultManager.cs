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


        public void Load()
        {
            if (File.Exists("Resources/insults.json")) {
                try {
                    _insults = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("Resources/insults.json"));
                } catch (Exception e) {
                    Console.WriteLine("Insult loading error, clearing insults. Details:\n" + e.ToString());
                    _ioerr = true;
                }
            } else {
                Console.WriteLine("insults.json is missing.");
            }
        }

        public bool Save()
        {
            if (_ioerr) {
                Console.WriteLine("Insult saving skipped until file conflicts are resolved!");
                return false;
            }

            try {
                File.WriteAllText("Resources/insults.json", JsonConvert.SerializeObject(_insults, Formatting.Indented));
            } catch (Exception e) {
                Console.WriteLine("IO insults save error. Details:\n" + e.ToString());
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
