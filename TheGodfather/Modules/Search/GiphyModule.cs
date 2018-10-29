#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("gif"), Module(ModuleType.Searches), NotBlocked]
    [Description("GIPHY commands. If invoked without a subcommand, searches GIPHY with given query.")]
    [Aliases("giphy")]
    [UsageExamples("!gif wat")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class GiphyModule : TheGodfatherServiceModule<GiphyService>
    {

        public GiphyModule(GiphyService giphy, SharedData shared, DatabaseContextBuilder db) 
            : base(giphy, shared, db)
        {
            this.ModuleColor = DiscordColor.Violet;
        }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Query.")] string query)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            var res = await this.Service.SearchAsync(query);
            if (!res.Any()) {
                await this.InformFailureAsync(ctx, "No results...");
                return;
            }

            await ctx.RespondAsync(res.First().Url);
        }


        #region COMMAND_GIPHY_RANDOM
        [Command("random")]
        [Description("Return a random GIF.")]
        [Aliases("r", "rand", "rnd")]
        [UsageExamples("!gif random")]
        public async Task RandomAsync(CommandContext ctx)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            var res = await this.Service.GetRandomGifAsync();

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = "Random gif:",
                ImageUrl = res.Url,
                Color = this.ModuleColor
            }.Build());
        }
        #endregion

        #region COMMAND_GIPHY_TRENDING
        [Command("trending")]
        [Description("Return an amount of trending GIFs.")]
        [Aliases("t", "tr", "trend")]
        [UsageExamples("!gif trending",
                       "!gif trending 3")]
        public async Task TrendingAsync(CommandContext ctx,
                                       [Description("Number of results (1-10).")] int amount = 5)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            var res = await this.Service.GetTrendingGifsAsync(amount);

            var emb = new DiscordEmbedBuilder() {
                Title = "Trending gifs:",
                Color = this.ModuleColor
            };

            foreach (var gif in res)
                emb.AddField($"{gif.Username} (rating: {gif.Rating})", gif.EmbedUrl);

            await ctx.RespondAsync(embed: emb.Build());
        }
        #endregion
    }
}
