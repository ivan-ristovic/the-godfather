#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Module(ModuleType.Searches), NotBlocked]
    [Cooldown(5, 10, CooldownBucketType.Channel)]
    public class SearchModule : TheGodfatherModule
    {

        #region COMMAND_CAT
        [Command("cat")]
        [Description("Get a random cat image.")]
        [Aliases("kitty", "kitten")]
        public async Task RandomCatAsync(CommandContext ctx)
        {
            string url = await PetImagesService.GetRandomCatImageAsync();
            if (url is null)
                throw new CommandFailedException("Connection to random.cat failed!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = DiscordEmoji.FromName(ctx.Client, ":cat:"),
                ImageUrl = url,
                Color = this.ModuleColor
            });
        }
        #endregion

        #region COMMAND_DOG
        [Command("dog")]
        [Description("Get a random dog image.")]
        [Aliases("doge", "puppy", "pup")]
        public async Task RandomDogAsync(CommandContext ctx)
        {
            string url = await PetImagesService.GetRandomDogImageAsync();
            if (url is null)
                throw new CommandFailedException("Connection to random.dog failed!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = DiscordEmoji.FromName(ctx.Client, ":dog:"),
                ImageUrl = url,
                Color = this.ModuleColor
            });
        }
        #endregion

        #region COMMAND_IPSTACK
        [Command("ipstack")]
        [Description("Retrieve IP geolocation information.")]
        [Aliases("ip", "geolocation", "iplocation", "iptracker", "iptrack", "trackip", "iplocate", "geoip")]

        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("IP.")] IPAddress ip)
        {
            IpInfo info = await IpGeolocationService.GetInfoForIpAsync(ip);

            if (!info.Success)
                throw new CommandFailedException($"Retrieving IP geolocation info failed! Details: {info.ErrorMessage}");

            await ctx.RespondAsync(embed: info.ToDiscordEmbed(this.ModuleColor));
        }
        #endregion

        #region COMMAND_NEWS
        [Command("news")]
        [Description("Get newest world news.")]
        [Aliases("worldnews")]
        public Task NewsRssAsync(CommandContext ctx)
        {
            IReadOnlyList<SyndicationItem> res = RssFeedsService.GetFeedResults("https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en");
            if (res is null)
                throw new CommandFailedException("Error getting world news.");

            return RssFeedsService.SendFeedResultsAsync(ctx.Channel, res);
        }
        #endregion

        #region COMMAND_QUOTEOFTHEDAY
        [Command("quoteoftheday")]
        [Description("Get quote of the day. You can also specify a category from the list: inspire, management, sports, life, funny, love, art, students.")]
        [Aliases("qotd", "qod", "quote", "q")]

        public async Task QotdAsync(CommandContext ctx,
                                   [Description("Category.")] string category = null)
        {
            Quote quote = await QuoteService.GetQuoteOfTheDayAsync(category);
            if (quote is null)
                throw new CommandFailedException("Failed to retrieve quote! Possibly the given quote category does not exist.");

            await ctx.RespondAsync(embed: quote.ToDiscordEmbed($"Quote of the day{(string.IsNullOrWhiteSpace(category) ? "" : $" in category {category}")}"));
        }
        #endregion
    }
}
