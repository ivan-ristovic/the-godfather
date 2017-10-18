#region USING_DIRECTIVES
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ServiceModel.Syndication;
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
        private ConcurrentDictionary<string, FeedInfo> _feeds = new ConcurrentDictionary<string, FeedInfo>();
        private bool _ioerr = false;


        public FeedManager()
        {
            Task.Run(async () => await CheckFeedsForChangesContinuousAsync());
        }

        
        public void Load(DebugLogger log)
        {
            if (File.Exists("Resources/feeds.json")) {
                try {
                    _feeds = JsonConvert.DeserializeObject<ConcurrentDictionary<string, FeedInfo>>(File.ReadAllText("Resources/feeds.json"));
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

        public IEnumerable<SyndicationItem> GetFeedResults(string url)
        {
            SyndicationFeed feed = null;
            XmlReader reader = null;
            try {
                reader = XmlReader.Create(url);
                feed = SyndicationFeed.Load(reader);
                reader.Close();
            } catch (Exception) {
                return null;
            } finally {
                reader?.Close();
            }

            return feed.Items.Take(5);
        }

        private async Task CheckFeedsForChangesContinuousAsync()
        {
            while (true) {
                foreach (var feed in _feeds) {
                    var newest = GetFeedResults(feed.Key).First();
                    if (newest.Title.Text != feed.Value.SavedTitle) {
                        feed.Value.SavedTitle = newest.Title.Text;
                        // Send message
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }


        private sealed class FeedInfo
        {
            public ulong ChannelId { get; internal set; }
            public string SavedTitle { get; internal set; }
        }
    }
}
