#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using GiphyDotNet.Model.GiphyImage;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("gif", CanInvokeWithoutSubcommand = true)]
    [Description("GIPHY commands.")]
    [Aliases("giphy")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class GiphyModule
    {
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Query.")] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                throw new InvalidCommandUsageException("Query missing!");

            var res = await ctx.Dependencies.GetDependency<GiphyService>().SearchAsync(q)
                .ConfigureAwait(false);

            if (res.Count() != 0)
                await ctx.RespondAsync(res[0].Url).ConfigureAwait(false);
            else
                await ctx.RespondAsync("No results...").ConfigureAwait(false);
        }


        #region COMMAND_GIPHY_RANDOM
        [Command("random")]
        [Description("Return a random GIF.")]
        [Aliases("r", "rand", "rnd")]
        public async Task RandomAsync(CommandContext ctx)
        {
            var res = await ctx.Dependencies.GetDependency<GiphyService>().GetRandomGifAsync()
                .ConfigureAwait(false);
            await ctx.RespondAsync(res.ImageUrl)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GIPHY_TRENDING
        [Command("trending")]
        [Description("Return an amount of trending GIFs.")]
        [Aliases("t", "tr", "trend")]
        public async Task TrendingAsync(CommandContext ctx,
                                       [Description("Number of results (1-10).")] int n = 5)
        {
            if (n < 1 || n > 10)
                throw new CommandFailedException("Number of results must be 1-10.", new ArgumentOutOfRangeException());

            var res = await ctx.Dependencies.GetDependency<GiphyService>().GetTrendingGifsAsync(n)
                .ConfigureAwait(false);

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = "Trending gifs:",
                Description = res.Aggregate("", (string s, Data r) => s += r.Url + '\n'),
                Color = DiscordColor.Gold
            }.Build()).ConfigureAwait(false);
        }
        #endregion
    }
}
