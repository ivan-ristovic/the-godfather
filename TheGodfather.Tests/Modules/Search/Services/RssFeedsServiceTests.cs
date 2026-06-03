using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Tests.Modules.Search.Services;

[TestFixture]
public sealed class RssFeedsServiceTests : ITheGodfatherServiceTest<RssFeedsService>
{
    public RssFeedsService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
        => this.Service = new RssFeedsService(TestDbProvider.Database);


    [Test]
    public async Task FeedAndSubscriptionQueriesUseUrlNameAndChannelScopes()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.RssFeeds.Add(new RssFeed { Id = 1, Url = "https://example.com/feed.xml", LastPostUrl = "https://example.com/1" });
                db.RssSubscriptions.Add(new RssSubscription {
                    Id = 1,
                    GuildId = MockData.Ids[0],
                    ChannelId = MockData.Ids[1],
                    Name = "news"
                });
                db.RssSubscriptions.Add(new RssSubscription {
                    Id = 1,
                    GuildId = MockData.Ids[0],
                    ChannelId = MockData.Ids[2],
                    Name = "other"
                });
                return Task.CompletedTask;
            },
            _ => Task.CompletedTask,
            async _ => {
                RssFeed? feed = await this.Service.GetByUrlAsync("https://example.com/feed.xml");
                RssSubscription? subscription = await this.Service.Subscriptions.GetByNameAsync((MockData.Ids[0], MockData.Ids[1]), "NEWS");

                Assert.That(feed, Is.Not.Null);
                Assert.That(subscription, Is.Not.Null);
                Assert.That(subscription!.Feed.Url, Is.EqualTo("https://example.com/feed.xml"));
                Assert.That(this.Service.Subscriptions.GetIds((MockData.Ids[0], MockData.Ids[1])), Is.EqualTo(new[] { 1 }));
            }
        );
    }

    [Test]
    public void InvalidFeedUrlsReturnFalseOrNull()
    {
        Assert.That(RssFeedsService.IsValidFeedURL("not a url"), Is.False);
        Assert.That(RssFeedsService.GetFeedResults("", 5), Is.Null);
        Assert.That(RssFeedsService.GetFeedResults("not a url", 5), Is.Null);
    }
}
