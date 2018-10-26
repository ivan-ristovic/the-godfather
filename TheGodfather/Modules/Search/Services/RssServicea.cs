#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Humanizer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Search.Services
{
    public static class RssService
    {
        public static async Task CheckFeedsForChangesAsync(DiscordClient client, DBService db)
        {
            IReadOnlyList<FeedEntry> feeds = await db.GetAllFeedEntriesAsync();
            foreach (var feed in feeds) {
                try {
                    if (!feed.Subscriptions.Any()) {
                        await db.RemoveFeedEntryAsync(feed.Id);
                        continue;
                    }

                    SyndicationItem latest = GetFeedResults(feed.Url)?.FirstOrDefault();
                    if (latest is null)
                        continue;

                    string url = latest.Links.FirstOrDefault()?.Uri.ToString();
                    if (url is null)
                        continue;

                    if (string.Compare(url, feed.SavedUrl, true) != 0) {
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
                                Timestamp = latest.LastUpdatedTime > latest.PublishDate ? latest.LastUpdatedTime : latest.PublishDate,
                                Color = DiscordColor.White,
                            };

                            if (latest.Content is TextSyndicationContent content)
                                emb.WithImageUrl(RedditService.GetImageUrl(content));

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
                throw new ArgumentException("URL missing", nameof(url));

            if (amount < 1 || amount > 20)
                throw new ArgumentException("Question amount out of range (max 20)", nameof(amount));

            try {
                using (var reader = XmlReader.Create(url)) {
                    var feed = SyndicationFeed.Load(reader);
                    return feed.Items?.Take(amount).ToList().AsReadOnly();
                }
            } catch {
                return null;
            }
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
            if (results is null)
                return;

            var emb = new DiscordEmbedBuilder() {
                Title = "Topics active recently",
                Color = DiscordColor.White
            };

            foreach (var res in results)
                emb.AddField(res.Title.Text.Truncate(255), res.Links.First().Uri.ToString());

            await channel.SendMessageAsync(embed: emb.Build());
        }
    }
}
