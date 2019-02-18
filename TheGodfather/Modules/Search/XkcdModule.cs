#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("xkcd"), Module(ModuleType.Searches), NotBlocked]
    [Description("Search xkcd. Group call returns random comic or, if an ID is provided, a comic with given ID.")]
    [Aliases("x")]
    [UsageExamples("!xkcd")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class XkcdModule : TheGodfatherModule
    {

        public XkcdModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Blue;
        }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Comic ID.")] int id)
            => this.ByIdAsync(ctx, id);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.RandomAsync(ctx);


        #region COMMAND_XKCD_ID
        [Command("id")]
        [Description("Retrieves comic with given ID from xkcd.")]
        [UsageExamples("!xkcd id 650")]
        public async Task ByIdAsync(CommandContext ctx,
                                 [Description("Comic ID.")] int? id = null)
        {
            XkcdComic comic = await XkcdService.GetComicAsync(id);
            if (comic is null)
                throw new CommandFailedException("Failed to retrieve comic from xkcd.");

            await ctx.RespondAsync(embed: comic.ToDiscordEmbed(this.ModuleColor));
        }
        #endregion

        #region COMMAND_XKCD_LATEST
        [Command("latest")]
        [Description("Retrieves latest comic from xkcd.")]
        [Aliases("fresh", "newest", "l")]
        [UsageExamples("!xkcd latest")]
        public Task LatestAsync(CommandContext ctx)
            => this.ByIdAsync(ctx);
        #endregion

        #region COMMAND_XKCD_RANDOM
        [Command("random")]
        [Description("Retrieves a random comic.")]
        [Aliases("rnd", "r", "rand")]
        [UsageExamples("!xkcd random")]
        public async Task RandomAsync(CommandContext ctx)
        {
            XkcdComic comic = await XkcdService.GetRandomComicAsync();
            if (comic is null)
                throw new CommandFailedException("Failed to retrieve comic from xkcd.");

            await ctx.RespondAsync(embed: comic.ToDiscordEmbed(this.ModuleColor));
        }
        #endregion
    }
}
