using Google.Apis.YouTube.v3.Data;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Tests.Modules.Search.Services;

[TestFixture]
public sealed class SearchServiceAvailabilityTests
{
    [Test]
    public async Task ApiKeyBackedServicesShortCircuitWhenKeysAreMissing()
    {
        var cfg = new BotConfigService();

        var crypto = new CryptoCurrencyService(cfg);
        Assert.That(crypto.IsDisabled, Is.True);
        Assert.That(await crypto.SearchAsync("btc"), Is.Null);
        Assert.That(await crypto.GetAllAsync(), Is.Null);

        var giphy = new GiphyService(cfg);
        Assert.That(giphy.IsDisabled, Is.True);
        Assert.That(await giphy.SearchGifAsync("test"), Is.Null);
        Assert.That(await giphy.GetRandomGifAsync(), Is.Null);

        var goodreads = new GoodreadsService(cfg);
        Assert.That(goodreads.IsDisabled, Is.True);
        Assert.That(await goodreads.SearchBooksAsync("dune"), Is.Null);

        var imgur = new ImgurService(cfg);
        Assert.That(imgur.IsDisabled, Is.True);
        Assert.That(await imgur.GetItemsFromSubAsync("aww", 1, SubredditGallerySortOrder.Top, TimeWindow.Day), Is.Null);

        var omdb = new OMDbService(cfg);
        Assert.That(omdb.IsDisabled, Is.True);
        Assert.That(await omdb.SearchAsync("dune"), Is.Null);
        Assert.That(await omdb.SearchSingleAsync(OMDbQueryType.Title, "dune"), Is.Null);

        var quote = new QuoteService(cfg);
        Assert.That(quote.IsDisabled, Is.True);

        using var steam = new SteamService(cfg);
        Assert.That(steam.IsDisabled, Is.True);
        Assert.That(await steam.GetInfoAsync("someone"), Is.Null);
        Assert.That(steam.GetCommunityProfileUrl("someone"), Is.EqualTo("http://steamcommunity.com/id/someone"));
        Assert.That(steam.GetGameStoreUrl(570), Is.EqualTo("https://store.steampowered.com//app/570"));

        var weather = new WeatherService(cfg);
        Assert.That(weather.IsDisabled, Is.True);
        Assert.That(weather.GraphFor("Belgrade"), Is.Null);
        Assert.That(await weather.GetAsync("Belgrade", "en"), Is.Null);

        var youtube = new YtService(cfg);
        Assert.That(youtube.IsDisabled, Is.True);
        Assert.That(await youtube.GetChannelIdAsync("UC123"), Is.Null);
        Assert.That(youtube.GetUrlForResourceId(new ResourceId { Kind = "youtube#video", VideoId = "abc" }),
            Is.EqualTo("https://www.youtube.com/watch?v=abc"));
    }

    [Test]
    public void PublicSearchServicesDoNotRequireConfigurationKeys()
    {
        Assert.That(new CatFactsService().IsDisabled, Is.False);
        Assert.That(new IpGeolocationService().IsDisabled, Is.False);
        Assert.That(new JokesService().IsDisabled, Is.False);
        Assert.That(new NewsService().IsDisabled, Is.False);
        Assert.That(new PetImagesService().IsDisabled, Is.False);
        Assert.That(new RedditService().IsDisabled, Is.False);
        Assert.That(new UrbanDictService().IsDisabled, Is.False);
        Assert.That(new WikiService().IsDisabled, Is.False);
        Assert.That(new XkcdService().IsDisabled, Is.False);
    }
}
