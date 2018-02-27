using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

using DSharpPlus;
using DSharpPlus.Entities;

namespace TheGodfather.Services
{
    public static class RSSService
    {
        private static Regex SuredditbPrefixRegex = new Regex("^/?r/", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);


        public static bool IsValidRSSFeedURL(string url)
        {
            try {
                var feed = SyndicationFeed.Load(XmlReader.Create(url));
            } catch {
                return false;
            }
            return true;
        }

        public static string GetRedditFeedURLForSubreddit(string sub, out string fixedsub)
        {
            sub = SuredditbPrefixRegex.Replace(sub, "");
            fixedsub = "/r/" + sub.ToLowerInvariant();

            string url = $"https://www.reddit.com{fixedsub}/new/.rss";
            if (!IsValidRSSFeedURL(url))
                return null;

            return url;
        }

        public static IEnumerable<SyndicationItem> GetFeedResults(string url, int amount = 5)
        {
            try {
                using (var reader = XmlReader.Create(url)) {
                    var feed = SyndicationFeed.Load(reader);
                    return feed.Items.Take(amount);
                }
            } catch {
                return null;
            }
        }

        public static async Task CheckFeedsForChangesAsync(DiscordClient client, DBService db)
        {
            var _feeds = await db.GetAllSubscriptionsAsync()
                .ConfigureAwait(false);
            foreach (var feed in _feeds) {

                // TODO ?
                if (feed.Subscriptions.Count == 0)
                    continue;

                try {
                    var newest = GetFeedResults(feed.URL).First();
                    var url = newest.Links[0].Uri.ToString();
                    if (url != feed.SavedURL) {
                        await db.UpdateFeedSavedURLAsync(feed.Id, url)
                            .ConfigureAwait(false);
                        foreach (var sub in feed.Subscriptions) {
                            var chn = await client.GetChannelAsync(sub.ChannelId)
                                .ConfigureAwait(false);
                            var em = new DiscordEmbedBuilder() {
                                Title = $"{newest.Title.Text}",
                                Url = url,
                                Timestamp = newest.LastUpdatedTime,
                                Color = DiscordColor.Orange,
                            };

                            // FIXME reddit hack
                            if (newest.Content is TextSyndicationContent content) {
                                var r = new Regex("<span> *<a +href *= *\"([^\"]+)\"> *\\[link\\] *</a> *</span>");
                                var matches = r.Match(content.Text);
                                if (matches.Success)
                                    em.WithImageUrl(matches.Groups[1].Value);
                            }
                            if (!string.IsNullOrWhiteSpace(sub.QualifiedName))
                                em.AddField("From", sub.QualifiedName);
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
        }

        public static async Task SendFeedResultsAsync(DiscordChannel channel, IEnumerable<SyndicationItem> results)
        {
            if (results == null)
                return;

            var emb = new DiscordEmbedBuilder() {
                Title = "Topics active recently",
                Color = DiscordColor.Green
            };

            foreach (var res in results)
                emb.AddField(res.Title.Text, res.Links[0].Uri.ToString());

            await channel.SendMessageAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
    }
}
