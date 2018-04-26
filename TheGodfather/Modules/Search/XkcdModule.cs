#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("xkcd"), Module(ModuleType.Searches)]
    [Description("Search xkcd. If invoked without subcommands returns random comic or, if an ID is provided, a comic with given ID.")]
    [UsageExample("!xkcd")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class XkcdModule : TheGodfatherBaseModule
    {

        [GroupCommand, Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Comic ID.")] int id)
        {

        }

        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {

        }


        #region COMMAND_XKCD_LATEST
        [Command("latest"), Module(ModuleType.Searches)]
        [Description("Retrieves latest comic from xkcd.")]
        [Aliases("fresh", "newest", "l")]
        [UsageExample("!xkcd latest")]
        public async Task LatestAsync(CommandContext ctx)
        {
            var comic = await XkcdService.GetLatestComicAsync()
                .ConfigureAwait(false);

            if (comic == null)
                throw new CommandFailedException("Failed to retrieve latest comic from xkcd.");

            await ctx.RespondAsync(embed: comic.Embed())
                .ConfigureAwait(false);
        }
        #endregion
    }
}
