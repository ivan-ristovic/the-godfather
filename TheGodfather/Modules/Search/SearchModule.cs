using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search
{
    [Module(ModuleType.Searches), NotBlocked]
    [Cooldown(5, 10, CooldownBucketType.Channel)]
    public sealed class SearchModule : TheGodfatherModule
    {
        #region cat
        [Command("cat")]
        [Aliases("kitty", "kitten")]
        public async Task RandomCatAsync(CommandContext ctx)
        {
            string? url = await PetImagesService.GetRandomCatImageAsync();
            if (url is null)
                throw new CommandFailedException(ctx, "cmd-err-image");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = Emojis.Animals.All[1],
                ImageUrl = url,
                Color = this.ModuleColor
            });
        }
        #endregion

        #region catfact
        [Command("catfact")]
        [Aliases("kittyfact", "kittenfact")]
        public async Task RandomCatFactAsync(CommandContext ctx)
        {
            string? fact = await ctx.Services.GetRequiredService<CatFactsService>().GetFactAsync();
            if (fact is null)
                throw new CommandFailedException(ctx, "cmd-err-res-none");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.Animals.All[1]} {fact}",
                Color = this.ModuleColor
            });
        }
        #endregion

        #region dog
        [Command("dog")]
        [Aliases("doge", "puppy", "pup")]
        public async Task RandomDogAsync(CommandContext ctx)
        {
            string? url = await PetImagesService.GetRandomDogImageAsync();
            if (url is null)
                throw new CommandFailedException(ctx, "cmd-err-image");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = DiscordEmoji.FromName(ctx.Client, ":dog:"),
                ImageUrl = url,
                Color = this.ModuleColor
            });
        }
        #endregion

        #region ip
        [Command("ip")]
        [Aliases("ipstack", "geolocation", "iplocation", "iptracker", "iptrack", "trackip", "iplocate", "geoip")]
        public async Task IpAsync(CommandContext ctx,
                                 [Description("desc-ip")] IPAddress ip)
        {
            IpInfo? info = await IpGeolocationService.GetInfoForIpAsync(ip);
            if (info is null || !info.Success)
                throw new CommandFailedException(ctx, "cmd-err-geoloc");

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle(info.Ip);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField("str-location", $"{info.City}, {info.RegionName} {info.RegionCode}, {info.CountryName} {info.CountryCode}");
                emb.AddLocalizedField("str-location-exact", $"({info.Latitude} , {info.Longitude})", inline: true);
                emb.AddLocalizedField("str-isp", info.Isp, inline: true);
                emb.AddLocalizedField("str-org", info.Organization, inline: true);
                emb.AddLocalizedField("str-as", info.As, inline: true);
                emb.WithLocalizedFooter("fmt-powered-by", null, "ip-api");
            });
        }
        #endregion

        #region news
        [Command("news")]
        [Aliases("worldnews")]
        public Task NewsRssAsync(CommandContext ctx,
                                [Description("str-topic")] string topic = "world")
        {
            IReadOnlyList<SyndicationItem>? res = NewsService.FetchNews(this.Localization.GetGuildCulture(ctx.Guild.Id), topic);
            if (res is null || !res.Any())
                throw new CommandFailedException(ctx, "cmd-err-news");

            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle("fmt-news", Emojis.Globe, topic);
                emb.WithColor(this.ModuleColor);
                var sb = new StringBuilder();
                foreach (SyndicationItem r in res)
                    sb.Append(Emojis.SmallBlueDiamond).Append(' ').AppendLine(Formatter.MaskedUrl(r.Title.Text, r.Links.First().Uri));
                emb.WithDescription(sb.ToString());
            });
        }
        #endregion

        #region quoteoftheday
        [Command("quoteoftheday")]
        [Aliases("qotd", "qod", "quote", "q")]
        public async Task QotdAsync(CommandContext ctx,
                                   [Description("str-topic")] string? category = null)
        {
            Quote? quote = await QuoteService.GetQuoteOfTheDayAsync(category);
            if (quote is null)
                throw new CommandFailedException(ctx, "cmd-err-quote");

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                if (string.IsNullOrWhiteSpace(category))
                    emb.WithLocalizedTitle("str-qotd");
                else
                    emb.WithLocalizedTitle("str-qotd-cat", category);
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription("fmt-qotd", quote.Content, quote.Author);
                emb.WithImageUrl(quote.BackgroundImageUrl);
                emb.WithUrl(quote.Permalink);
                emb.WithLocalizedFooter("fmt-powered-by", null, "theysaidso.com");
            });
        }
        #endregion
    }
}
