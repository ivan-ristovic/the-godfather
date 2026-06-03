using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using Discord.Commands;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

public sealed class SearchModule : TheGodfatherModule
{
    private readonly CatFactsService catFacts;
    private readonly QuoteService quotes;


    public SearchModule(LocalizationService localization, CatFactsService catFacts, QuoteService quotes)
        : base(localization, new Color(0x1abc9c))
    {
        this.catFacts = catFacts;
        this.quotes = quotes;
    }


    [Command("cat")]
    [Alias("kitty", "kitten")]
    public async Task RandomCatAsync()
    {
        string? url = await PetImagesService.GetRandomCatImageAsync();
        if (url is null) {
            await this.FailAsync(TranslationKey.cmd_err_image);
            return;
        }

        await this.ReplyAsync(embed: new EmbedBuilder {
            Description = Emojis.Animals.All[1],
            ImageUrl = url,
            Color = this.ModuleColor
        }.Build());
    }

    [Command("catfact")]
    [Alias("kittyfact", "kittenfact")]
    public async Task RandomCatFactAsync()
    {
        string? fact = await this.catFacts.GetFactAsync();
        if (fact is null) {
            await this.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await this.ReplyAsync(embed: new EmbedBuilder {
            Description = $"{Emojis.Animals.All[1]} {fact}",
            Color = this.ModuleColor
        }.Build());
    }

    [Command("dog")]
    [Alias("doge", "puppy", "pup")]
    public async Task RandomDogAsync()
    {
        string? url = await PetImagesService.GetRandomDogImageAsync();
        if (url is null) {
            await this.FailAsync(TranslationKey.cmd_err_image);
            return;
        }

        await this.ReplyAsync(embed: new EmbedBuilder {
            Description = "\U0001f436",
            ImageUrl = url,
            Color = this.ModuleColor
        }.Build());
    }

    [Command("ip")]
    [Alias("ipstack", "geolocation", "iplocation", "iptracker", "iptrack", "trackip", "iplocate", "geoip")]
    public async Task IpAsync(IPAddress ip)
    {
        IpInfo? info = await IpGeolocationService.GetInfoForIpAsync(ip);
        if (info is null || !info.Success) {
            await this.FailAsync(TranslationKey.cmd_err_geoloc);
            return;
        }

        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(info.Ip);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_location, FormatLocation(info));
            emb.AddLocalizedField(TranslationKey.str_location_exact, $"({info.Latitude} , {info.Longitude})", true);
            emb.AddLocalizedField(TranslationKey.str_isp, info.Isp, true);
            emb.AddLocalizedField(TranslationKey.str_org, info.Organization, true);
            emb.AddLocalizedField(TranslationKey.str_hosting, info.Hosting, true);
            emb.AddLocalizedField(TranslationKey.str_mobile, info.Mobile, true);
            emb.AddLocalizedField(TranslationKey.str_proxy, info.Proxy, true);
            if (!string.IsNullOrWhiteSpace(info.Reverse))
                emb.AddLocalizedField(TranslationKey.str_reverse, info.Reverse, true);
            emb.WithLocalizedFooter(TranslationKey.fmt_powered_by("ip-api"), null);
            emb.AddLocalizedField(TranslationKey.str_as, info.As, true);
            if (!string.IsNullOrWhiteSpace(info.ASName))
                emb.AddLocalizedField(TranslationKey.str_as_name, info.ASName, true);
        });
    }

    [Command("news")]
    [Alias("worldnews")]
    public Task NewsRssAsync(string topic = "world")
    {
        IReadOnlyList<SyndicationItem>? results = NewsService.FetchNews(this.Localization.GetGuildCulture(this.Context.Guild?.Id), topic);
        if (results is null || !results.Any())
            return this.FailAsync(TranslationKey.cmd_err_news);

        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.fmt_news(Emojis.Globe, topic));
            emb.WithColor(this.ModuleColor);
            var sb = new StringBuilder();
            foreach (SyndicationItem result in results)
                sb.Append(Emojis.SmallBlueDiamond)
                  .Append(' ')
                  .Append('[')
                  .Append(result.Title.Text.Replace("]", "\\]"))
                  .Append("](")
                  .Append(result.Links.First().Uri)
                  .AppendLine(")");
            emb.WithDescription(sb.ToString());
        });
    }

    [Command("quoteoftheday")]
    [Alias("qotd", "qod", "quote", "q")]
    public async Task QotdAsync()
    {
        Quote? quote = await this.quotes.GetQuoteOfTheDayAsync();
        if (quote is null) {
            await this.FailAsync(TranslationKey.cmd_err_quote);
            return;
        }

        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_qotd);
            emb.WithColor(this.ModuleColor);
            emb.WithLocalizedDescription(TranslationKey.fmt_qotd(quote.Content, quote.Author));
            emb.WithImageUrl(quote.BackgroundImageUrl);
            emb.WithUrl(quote.Permalink);
            emb.WithLocalizedFooter(TranslationKey.fmt_powered_by("theysaidso.com"), null);
        });
    }


    private static string FormatLocation(IpInfo info)
    {
        return new[] {
            info.District,
            info.City,
            info.Region,
            info.RegionName,
            info.RegionCode,
            info.CountryName,
            info.CountryCode,
            info.ContinentCode,
            info.Continent,
        }.Where(s => !string.IsNullOrWhiteSpace(s)).JoinWith(", ");
    }
}
