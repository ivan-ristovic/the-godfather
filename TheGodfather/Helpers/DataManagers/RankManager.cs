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
            "4U donor",
            "SoH MNG",
            "Gypsy",
            "Romanian wallet stealer",
            "Serbian street cleaner",
            "German closet cleaner",
            "Swed's beer supplier",
            "JoJo's harem cleaner",
            "Torq's nurse",
            "Pakistani bomb carrier",
            "Michal's worker (black)",
            "Michal's worker (white)",
            "LDR"
            #endregion
        };


        public RankManager()
        {

        }


        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/ranks.json")) {
                try {
                    _msgcount = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, ulong>>(File.ReadAllText("Resources/ranks.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Rank loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerr = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "ranks.json is missing.", DateTime.Now);
            }
        }

        public bool Save(DebugLogger log)
        {
            if (_ioerr) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Ranks saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/ranks.json", JsonConvert.SerializeObject(_msgcount, Formatting.Indented));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Ranks save error. Details:\n" + e.ToString(), DateTime.Now);
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
