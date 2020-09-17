using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search.Extensions
{
    public static class DatabaseContextBuilderFeedsExtensions
    {
        public static async Task SubscribeAsync(this DbContextBuilder dbb, ulong gid, ulong cid, string url, string? name = null)
        {
            SyndicationItem? newest = RssService.GetFeedResults(url)?.FirstOrDefault();
            if (newest is null)
                throw new Exception("Can't load the feed entries.");

            using TheGodfatherDbContext db = dbb.CreateContext();
            RssFeed? feed = db.RssFeeds.SingleOrDefault(f => f.Url == url);
            if (feed is null) {
                feed = new RssFeed {
                    Url = url,
                    LastPostUrl = newest.Links[0].Uri.ToString()
                };
                db.RssFeeds.Add(feed);
                await db.SaveChangesAsync();
            }

            db.RssSubscriptions.Add(new RssSubscription {
                ChannelId = cid,
                GuildId = gid,
                Id = feed.Id,
                Name = name ?? url
            });

            await db.SaveChangesAsync();
        }
    }
}
