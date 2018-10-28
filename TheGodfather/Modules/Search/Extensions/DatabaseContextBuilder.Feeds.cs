using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search.Extensions
{
    public static class DatabaseContextBuilderFeedsExtensions
    {
        public static async Task SubscribeAsync(this DatabaseContextBuilder dbb, ulong gid, ulong cid, string url, string name = null)
        {
            var newest = RssService.GetFeedResults(url)?.FirstOrDefault();
            if (newest is null)
                throw new Exception("Can't load the feed entries.");

            using (DatabaseContext db = dbb.CreateContext()) {
                DatabaseRssFeed feed = db.RssFeeds.SingleOrDefault(f => f.Url == url);
                if (feed is null) {
                    feed = new DatabaseRssFeed() {
                        Url = url,
                        LastPostUrl = newest.Links[0].Uri.ToString()
                    };
                    db.RssFeeds.Add(feed);
                    await db.SaveChangesAsync();
                }

                db.RssSubscriptions.Add(new DatabaseRssSubscription() {
                    ChannelId = cid,
                    GuildId = gid,
                    Id = feed.Id,
                    Name = name ?? url
                });

                await db.SaveChangesAsync();
            }
        }
    }
}
