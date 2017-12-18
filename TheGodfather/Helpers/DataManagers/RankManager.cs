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
    public class RankManager
    {
        public IReadOnlyList<string> Ranks => _ranks;
        public IReadOnlyDictionary<ulong, ulong> MessageCount => _msgcount;
        private ConcurrentDictionary<ulong, ulong> _msgcount = new ConcurrentDictionary<ulong, ulong>();
        private bool _ioerr = false;
        private string[] _ranks = {
            #region RANKS
            // If you make more than 25 ranks, then fix the embed
            "4U donor",
            "SoH MNG",
            "Cheap gypsy",
            "Romanian wallet stealer",
            "Romanian car cracker",
            "Serbian street cleaner",
            "German closet cleaner",
            "Swed's beer supplier",
            "JoJo's harem cleaner",
            "Torq's nurse",
            "Expensive gypsy",
            "Pakistani bomb carrier",
            "Michal's worker (black)",
            "Michal's worker (white)",
            "World Mafia Waste",
            "KF's goat",
            "Legendary Seagull Master",
            "Brazillian flip-flop maker",
            "The Global Elite Silver",
            "LDR",
            "Generalissimo (tribute to Raptor)"
            #endregion
        };


        public RankManager()
        {

        }


        public void Load()
        {
            if (File.Exists("Resources/ranks.json")) {
                try {
                    _msgcount = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, ulong>>(File.ReadAllText("Resources/ranks.json"));
                } catch (Exception e) {
                    Console.WriteLine("Rank loading error, check file formatting. Details:\n" + e.ToString());
                    _ioerr = true;
                }
            } else {
                Console.WriteLine("ranks.json is missing.");
            }
        }

        public bool Save()
        {
            if (_ioerr) {
                Console.WriteLine("Ranks saving skipped until file conflicts are resolved!");
                return false;
            }

            try {
                File.WriteAllText("Resources/ranks.json", JsonConvert.SerializeObject(_msgcount, Formatting.Indented));
            } catch (Exception e) {
                Console.WriteLine("IO Ranks save error. Details:\n" + e.ToString());
                return false;
            }

            return true;
        }

        public int UpdateMessageCount(ulong uid)
        {
            if (_msgcount.ContainsKey(uid)) {
                _msgcount[uid]++;
            } else if (!_msgcount.TryAdd(uid, 1)) {
                return -1;
            }

            int curr = GetRankForMessageCount(_msgcount[uid]);
            int prev = GetRankForMessageCount(_msgcount[uid] - 1);
                
            return curr != prev ? curr : -1;
        }

        public int GetRankForId(ulong id)
        {
            return GetRankForMessageCount(_msgcount[id]);
        }

        public ulong GetMessageCountForId(ulong id)
        {
            return _msgcount[id];
        }

        public int GetRankForMessageCount(ulong msgcount)
        {
            return (int)Math.Floor(Math.Sqrt(msgcount / 10));
        }

        public uint XpNeededForRankWithIndex(int index)
        {
            return (uint)(index * index * 10);
        }
    }
}
