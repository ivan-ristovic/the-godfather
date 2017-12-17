#region USING_DIRECTIVES
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ServiceModel.Syndication;
using System.Linq;
using System.Text.RegularExpressions;
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


        public FeedManager(DiscordClient client)
        {
            Task.Run(async () => await CheckFeedsForChangesContinuousAsync(client).ConfigureAwait(false));
        }


        public static IEnumerable<SyndicationItem> GetFeedResults(string url)
        {
            SyndicationFeed feed = null;
            XmlReader reader = null;
            try {
                reader = XmlReader.Create(url);
                feed = SyndicationFeed.Load(reader);
            } catch (Exception) {
                return null;
            } finally {
                reader?.Close();
            }

            return feed.Items.Take(5);
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
                File.WriteAllText("Resources/feeds.json", JsonConvert.SerializeObject(_feeds, Newtonsoft.Json.Formatting.Indented));
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

            return _feeds.TryAdd(url, new FeedInfo(new List<ulong> { cid }, res.First().Links[0].Uri.ToString(), qname));
        }

        public bool TryRemove(ulong cid, string url)
        {
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
            var mathes = _feeds.Where(kvp => kvp.Value.QualifiedName.ToLower() == qname);
            bool succ = true;
            foreach (var feedkvp in mathes) {
                succ &= _feeds[feedkvp.Key].ChannelIds.Remove(cid);
                if (_feeds[feedkvp.Key].ChannelIds.Count == 0)
                    _feeds.TryRemove(feedkvp.Key, out _);
            }

            return succ;
        }

        public IReadOnlyList<string> GetFeedListForChannel(ulong cid)
        {
            return _feeds.Where(kvp => kvp.Value.ChannelIds.Contains(cid)).Select(kvp => kvp.Value.QualifiedName != null ? kvp.Value.QualifiedName : kvp.Key).ToList();
        }

        private async Task CheckFeedsForChangesContinuousAsync(DiscordClient client)
        {
            while (true) {
                foreach (var feed in _feeds) {
                    try {
                        var newest = GetFeedResults(feed.Key).First();
                        var url = newest.Links[0].Uri.ToString();
                        if (url != feed.Value.SavedURL) {
                            feed.Value.SavedURL = url;
                            foreach (var cid in feed.Value.ChannelIds) {
                                var chn = await client.GetChannelAsync(cid)
                                    .ConfigureAwait(false);
                                var em = new DiscordEmbedBuilder() {
                                    Title = $"{newest.Title.Text}",
                                    Url = url,
                                    Timestamp = newest.LastUpdatedTime,
                                    Color = DiscordColor.Orange,
                                };
                                
                                // TODO reddit hack
                                if (newest.Content is TextSyndicationContent content) {
                                    var r = new Regex("<span> *<a +href *= *\"([^\"]+)\"> *\\[link\\] *</a> *</span>");
                                    var matches = r.Match(content.Text);
                                    if (matches.Success)
                                        em.WithImageUrl(matches.Groups[1].Value);
                                }
                                if (feed.Value.QualifiedName != null)
                                    em.AddField("From", feed.Value.QualifiedName);
                                em.AddField("Link to content", url);
                                await chn.SendMessageAsync(embed: em.Build())
                                    .ConfigureAwait(false);
                                await Task.Delay(250)
                                    .ConfigureAwait(false);
                            }
                        }
                    } catch {

                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(2))
                    .ConfigureAwait(false);
            }
        }


        private sealed class FeedInfo
        {
            [JsonProperty("ChannelIds")]
            public List<ulong> ChannelIds { get; set; }

            [JsonProperty("SavedURL")]
            public string SavedURL { get; internal set; }
            
            [JsonProperty("QualifiedName")]
            public string QualifiedName { get; private set; }


            public FeedInfo(List<ulong> cids, string link, string qname = null)
            {
                ChannelIds = cids;
                SavedURL = link;
                QualifiedName = qname;
            }
        }
    }
}
