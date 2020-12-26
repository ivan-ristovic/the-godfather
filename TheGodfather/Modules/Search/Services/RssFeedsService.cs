using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Search.Services
{
    public sealed class RssFeedsService : DbAbstractionServiceBase<RssFeed, int>
    {
        public static bool IsValidFeedURL(string url)
        {
            try {
                var feed = SyndicationFeed.Load(XmlReader.Create(url, _settings));
            } catch {
                return false;
            }
            return true;
        }

        public static IReadOnlyList<SyndicationItem>? GetFeedResults(string url, int amount = 5)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (amount < 1 || amount > 20)
                amount = 5;

            try {
                using var reader = XmlReader.Create(url, _settings);
                var feed = SyndicationFeed.Load(reader);
                return feed.Items?.Take(amount).ToList().AsReadOnly();
            } catch {
                return null;
            }
        }

        [Obsolete]
        public static async Task CheckFeedsForChangesAsync(DiscordClient client, DbContextBuilder dbb)
        {
            IReadOnlyList<RssFeed> feeds;
            using (TheGodfatherDbContext db = dbb.CreateContext())
                feeds = await db.RssFeeds.Include(f => f.Subscriptions).ToListAsync();

            foreach (RssFeed feed in feeds) {
                try {
                    if (!feed.Subscriptions.Any()) {
                        using TheGodfatherDbContext db = dbb.CreateContext();
                        db.RssFeeds.Remove(feed);
                        await db.SaveChangesAsync();
                        continue;
                    }

                    SyndicationItem latest = GetFeedResults(feed.Url)?.FirstOrDefault();
                    if (latest is null)
                        continue;

                    string url = latest.Links.FirstOrDefault()?.Uri.ToString();
                    if (url is null)
                        continue;

                    if (string.Compare(url, feed.LastPostUrl, true) != 0) {
                        using (TheGodfatherDbContext db = dbb.CreateContext()) {
                            feed.LastPostUrl = url;
                            db.RssFeeds.Update(feed);
                            await db.SaveChangesAsync();
                        }

                        foreach (RssSubscription sub in feed.Subscriptions) {
                            DiscordChannel chn;
                            try {
                                chn = await client.GetChannelAsync(sub.ChannelId);
                            } catch (NotFoundException) {
                                using TheGodfatherDbContext db = dbb.CreateContext();
                                db.RssSubscriptions.Remove(sub);
                                await db.SaveChangesAsync();
                                continue;
                            } catch {
                                continue;
                            }

                            var emb = new DiscordEmbedBuilder {
                                Title = latest.Title.Text,
                                Url = url,
                                Timestamp = latest.LastUpdatedTime > latest.PublishDate ? latest.LastUpdatedTime : latest.PublishDate,
                                Color = DiscordColor.White,
                            };

                            if (latest.Content is TextSyndicationContent content)
                                emb.WithImageUrl(RedditService.GetImageUrl(content));

                            if (!string.IsNullOrWhiteSpace(sub.Name))
                                emb.AddField("From", sub.Name);
                            emb.AddField("Content link", url);

                            await chn.SendMessageAsync(embed: emb.Build());

                            await Task.Delay(100);
                        }
                    }
                } catch {

                }
            }
        }


        private static readonly XmlReaderSettings _settings = new XmlReaderSettings {
            MaxCharactersInDocument = 2097152,
            IgnoreComments = true,
            IgnoreWhitespace = true,
        };


        public RssSubscriptionService Subscriptions { get; }


        public RssFeedsService(DbContextBuilder dbb)
            : base(dbb)
        {
            this.Subscriptions = new RssSubscriptionService(dbb);
        }


        public override bool IsDisabled => false;

        public override DbSet<RssFeed> DbSetSelector(TheGodfatherDbContext db) => db.RssFeeds;
        public override RssFeed EntityFactory(int id) => new RssFeed { Id = id };
        public override int EntityIdSelector(RssFeed entity) => entity.Id;
        public override object[] EntityPrimaryKeySelector(int id) => new object[] { id };

        public async Task<RssFeed?> GetByUrlAsync(string url)
        {
            url = url.ToLowerInvariant();
            RssFeed? feed = null;
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            feed = await db.RssFeeds.SingleOrDefaultAsync(f => f.Url == url);
            return feed;
        }

        public async Task<bool> SubscribeAsync(ulong gid, ulong cid, string url, string? name = null)
        {
            SyndicationItem? newest = GetFeedResults(url)?.FirstOrDefault();
            if (newest is null)
                return false;

            RssFeed? feed = await this.GetByUrlAsync(url);
            if (feed is null) {
                feed = new RssFeed {
                    Url = url,
                    LastPostUrl = newest.Links[0].Uri.ToString()
                };
                await this.AddAsync(feed);
            }

            int added = await this.Subscriptions.AddAsync(new RssSubscription {
                ChannelId = cid,
                GuildId = gid,
                Id = feed.Id,
                Name = name ?? url
            });

            return added > 0;
        }


        public sealed class RssSubscriptionService : DbAbstractionServiceBase<RssSubscription, (ulong gid, ulong cid), int>
        {
            public override bool IsDisabled => false;


            public RssSubscriptionService(DbContextBuilder dbb)
                : base(dbb) { }


            public override DbSet<RssSubscription> DbSetSelector(TheGodfatherDbContext db)
                => db.RssSubscriptions;

            public override IQueryable<RssSubscription> GroupSelector(IQueryable<RssSubscription> entities, (ulong gid, ulong cid) grid)
                => entities.Where(s => s.GuildIdDb == (long)grid.gid && s.ChannelIdDb == (long)grid.cid);

            public override RssSubscription EntityFactory((ulong gid, ulong cid) grid, int id)
                => new RssSubscription { ChannelId = grid.cid, GuildId = grid.gid, Id = id };

            public override int EntityIdSelector(RssSubscription entity)
                => entity.Id;

            public override (ulong gid, ulong cid) EntityGroupSelector(RssSubscription entity)
                => (entity.GuildId, entity.ChannelId);

            public override object[] EntityPrimaryKeySelector((ulong gid, ulong cid) grid, int id)
                => new object[] { id, (long)grid.gid, (long)grid.cid };

            public async Task<RssSubscription?> GetByNameAsync((ulong gid, ulong cid) grid, string name)
            {
                name = name.ToLowerInvariant();
                RssSubscription? sub = null;
                using TheGodfatherDbContext db = this.dbb.CreateContext();
                sub = await this.GroupSelector(db.RssSubscriptions, grid)
                    .Include(s => s.Feed)
                    .SingleOrDefaultAsync(s => s.Name == name)
                    ;
                return sub;
            }
        }
    }
}
