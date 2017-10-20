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

        public bool TryAdd(ulong cid, string url, string qname = null)
        {
            var res = GetFeedResults(url);
            if (res == null)
                return false;

            if (_feeds.ContainsKey(url)) {
                if (_feeds[url].ChannelIds.Contains(cid))
                    return false;
                else
                    _feeds[url].ChannelIds.Add(cid);
                return true;
            }

            return _feeds.TryAdd(url, new FeedInfo(new List<ulong> { cid }, res.First().Title.Text, qname));
        }

        public bool TryRemove(ulong cid, string url)
        {
            url = url.ToLower();
            if (!_feeds.ContainsKey(url))
                return false;

            bool succ = _feeds[url].ChannelIds.Remove(cid);
            if (_feeds[url].ChannelIds.Count == 0)
                _feeds.TryRemove(url, out _);
            return succ;
        }

        public bool TryRemoveUsingQualified(ulong cid, string qname)
        {
            qname = qname.ToLower();
            var mathes = _feeds.Where(kvp => kvp.Value.QualifiedName == qname);
            bool succ = true;
            foreach (var feedkvp in mathes) {
                succ &= _feeds[feedkvp.Key].ChannelIds.Remove(cid);
                if (_feeds[feedkvp.Key].ChannelIds.Count == 0)
                    _feeds.TryRemove(feedkvp.Key, out _);
            }

            return succ;
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
                    try {
                        var newest = GetFeedResults(feed.Key).First();
                        if (newest.Title.Text != feed.Value.SavedTitle) {
                            feed.Value.SavedTitle = newest.Title.Text;
                            foreach (var cid in feed.Value.ChannelIds) {
                                var chn = await TheGodfather.Client.GetChannelAsync(cid);
                                await chn.SendMessageAsync(embed: new DiscordEmbedBuilder() {
                                    Title = $"New post: {newest.Title.Text}",
                                    Description = $"(from {(feed.Value.QualifiedName != null ? feed.Value.QualifiedName : feed.Key)})",
                                    Url = newest.Links[0].Uri.ToString(),
                                    Color = DiscordColor.Orange
                                });
                            }
                        }
                    } catch {

                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }


        private sealed class FeedInfo
        {
            [JsonProperty("ChannelIds")]
            public List<ulong> ChannelIds { get; set; }

            [JsonProperty("SavedTitle")]
            public string SavedTitle { get; internal set; }
            
            [JsonProperty("QualifiedName")]
            public string QualifiedName { get; private set; }


            public FeedInfo(List<ulong> cids, string title, string qname = null)
            {
                ChannelIds = cids;
                SavedTitle = title;
                QualifiedName = qname;
            }
        }
    }
}
