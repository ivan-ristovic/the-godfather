#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("xkcd"), Module(ModuleType.Searches)]
    [Description("Search xkcd. If invoked without subcommands returns random comic or, if an ID is provided, a comic with given ID.")]
    [Aliases("x")]
    [UsageExamples("!xkcd")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class XkcdModule : TheGodfatherBaseModule
    {

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Comic ID.")] int id)
            => ByIdAsync(ctx, id);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => RandomAsync(ctx);


        #region COMMAND_XKCD_ID
        [Command("id"), Module(ModuleType.Searches)]
        [Description("Retrieves comic with given ID from xkcd.")]
        [UsageExamples("!xkcd id 650")]
        public async Task ByIdAsync(CommandContext ctx,
                                 [Description("Comic ID.")] int? id = null)
        {
            var comic = await XkcdService.GetComicAsync(id)
                .ConfigureAwait(false);

            if (comic == null)
                throw new CommandFailedException("Failed to retrieve comic from xkcd.");

            await ctx.RespondAsync(embed: comic.ToDiscordEmbed())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_XKCD_LATEST
        [Command("latest"), Module(ModuleType.Searches)]
        [Description("Retrieves latest comic from xkcd.")]
        [Aliases("fresh", "newest", "l")]
        [UsageExamples("!xkcd latest")]
        public Task LatestAsync(CommandContext ctx)
            => ByIdAsync(ctx);
        #endregion

        #region COMMAND_XKCD_RANDOM
        [Command("random"), Module(ModuleType.Searches)]
        [Description("Retrieves a random comic.")]
        [Aliases("rnd", "r", "rand")]
        [UsageExamples("!xkcd random")]
        public async Task RandomAsync(CommandContext ctx)
        {
            var comic = await XkcdService.GetRandomComicAsync()
                .ConfigureAwait(false);

            if (comic == null)
                throw new CommandFailedException("Failed to retrieve comic from xkcd.");

            await ctx.RespondAsync(embed: comic.ToDiscordEmbed())
                .ConfigureAwait(false);
        }
        #endregion
    }
}
