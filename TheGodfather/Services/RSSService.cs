#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using TheGodfather.Services.Common;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Services
{
    public static class RssService
    {
        private static readonly Regex _subPrefixRegex = new Regex("^/?r?/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly  Regex _urlRegex = new Regex("<span> *<a +href *= *\"([^\"]+)\"> *\\[link\\] *</a> *</span>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _sanitizeRegex = new Regex("[^a-z0-9/]", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        public static async Task CheckFeedsForChangesAsync(DiscordClient client, DBService db)
        {
            IReadOnlyList<FeedEntry> feeds = await db.GetAllFeedEntriesAsync();
            foreach (var feed in feeds) {
                try {
                    if (!feed.Subscriptions.Any()) {
                        await db.RemoveFeedAsync(feed.Id);
                        continue;
                    }

                    SyndicationItem latest = GetFeedResults(feed.URL)?.FirstOrDefault();
                    if (latest == null)
                        continue;

                    string url = latest.Links.FirstOrDefault()?.Uri.ToString();
                    if (url == null)
                        continue;

                    if (string.Compare(url, feed.SavedURL, true) != 0) {
                        await db.UpdateFeedSavedURLAsync(feed.Id, url);

                        foreach (var sub in feed.Subscriptions) {
                            DiscordChannel chn;
                            try {
                                chn = await client.GetChannelAsync(sub.ChannelId);
                            } catch (NotFoundException) {
                                await db.RemoveSubscriptionByIdAsync(sub.ChannelId, feed.Id);
                                continue;
                            } catch {
                                continue;
                            }

                            var emb = new DiscordEmbedBuilder() {
                                Title = latest.Title.Text,
                                Url = url,
                                Timestamp = latest.LastUpdatedTime,
                                Color = DiscordColor.White,
                            };

                            if (latest.Content is TextSyndicationContent content) {
                                var matches = _urlRegex.Match(content.Text);
                                if (matches.Success)
                                    emb.WithImageUrl(matches.Groups[1].Value);
                            }

                            if (!string.IsNullOrWhiteSpace(sub.QualifiedName))
                                emb.AddField("From", sub.QualifiedName);
                            emb.AddField("Content link", url);

                            await chn.SendMessageAsync(embed: emb.Build());

                            await Task.Delay(100);
                        }
                    }
                } catch {

                }
            }
        }

        public static IReadOnlyList<SyndicationItem> GetFeedResults(string url, int amount = 5)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL missing", "url");

            if (amount < 1 || amount > 20)
                throw new ArgumentException("Question amount out of range (max 20)", "amount");

            try {
                using (var reader = XmlReader.Create(url)) {
                    var feed = SyndicationFeed.Load(reader);
                    return feed.Items?.Take(amount).ToList().AsReadOnly();
                }
            } catch {
                return null;
            }
        }

        public static string GetFeedURLForSubreddit(string sub, out string rsub)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new ArgumentException("Subreddit missing", "sub");

            if (_sanitizeRegex.IsMatch(sub))
                throw new ArgumentException("Subreddit is in invalid format (needs to be lowercase and without spaces, for example `/r/rule34`)", "sub");

            sub = _subPrefixRegex.Replace(sub, String.Empty);
            rsub = "/r/" + sub.ToLowerInvariant();

            string url = $"https://www.reddit.com{rsub}/new/.rss";
            if (!IsValidFeedURL(url))
                return null;

            return url;
        }

        public static bool IsValidFeedURL(string url)
        {
            try {
                var feed = SyndicationFeed.Load(XmlReader.Create(url));
            } catch {
                return false;
            }
            return true;
        }

        public static async Task SendFeedResultsAsync(DiscordChannel channel, IEnumerable<SyndicationItem> results)
        {
            if (results == null)
                return;

            var emb = new DiscordEmbedBuilder() {
                Title = "Topics active recently",
                Color = DiscordColor.White
            };

            foreach (var res in results)
                emb.AddField(res.Title.Text, res.Links.First().Uri.ToString());

            await channel.SendMessageAsync(embed: emb.Build());
        }
    }
}
