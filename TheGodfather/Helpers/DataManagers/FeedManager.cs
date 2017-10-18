#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers.DataManagers
{
    public class FeedManager
    {
        public IReadOnlyDictionary<string, ulong> Prefixes => _feeds;
        private ConcurrentDictionary<string, ulong> _feeds = new ConcurrentDictionary<string, ulong>();
        private bool _ioerr = false;


        public FeedManager()
        {
            Task.Run(async () => await CheckFeedsForChanges());
        }

        
        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/feeds.json")) {
                try {
                    _feeds = JsonConvert.DeserializeObject<ConcurrentDictionary<string, ulong>>(File.ReadAllText("Resources/feeds.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Feed loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _ioerr = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "feeds.json is missing.", DateTime.Now);
            }
        }

        public bool Save(DebugLogger log)
        {
            if (_ioerr) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Feed saving skipped until file conflicts are resolved!", DateTime.Now);
                return false;
            }

            try {
                File.WriteAllText("Resources/feeds.json", JsonConvert.SerializeObject(_feeds));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "IO Feed save error. Details:\n" + e.ToString(), DateTime.Now);
                return false;
            }

            return true;
        }

        private async Task CheckFeedsForChanges()
        {
            while (true) {
                foreach (var feed in _feeds) {
                    // TODO
                }
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }
    }
}
