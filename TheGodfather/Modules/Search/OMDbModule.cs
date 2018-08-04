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
    [Group("imdb"), Module(ModuleType.Searches)]
    [Description("Search Open Movie Database.")]
    [Aliases("movies", "series", "serie", "movie", "film", "cinema", "omdb")]
    [UsageExamples("!imdb Airplane")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class OMDbModule : TheGodfatherServiceModule<OMDbService>
    {

        public OMDbModule(OMDbService omdb) : base(omdb) { }


        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Title.")] string title)
            => SearchByTitleAsync(ctx, title);


        #region COMMAND_IMDB_SEARCH
        [Command("search"), Module(ModuleType.Searches)]
        [Description("Searches IMDb for given query and returns paginated results.")]
        [Aliases("s", "find")]
        [UsageExamples("!imdb search Kill Bill")]
        public async Task SearchAsync(CommandContext ctx,
                                     [RemainingText, Description("Search query.")] string query)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            var pages = await this.Service.GetPaginatedResultsAsync(query)
                .ConfigureAwait(false);

            if (pages == null)
                throw new CommandFailedException("No results found!");

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_IMDB_TITLE
        [Command("title"), Module(ModuleType.Searches)]
        [Description("Search by title.")]
        [Aliases("t", "name", "n")]
        [UsageExamples("!imdb title Airplane")]
        public Task SearchByTitleAsync(CommandContext ctx,
                                      [RemainingText, Description("Title.")] string title)
            => SearchAndSendResultAsync(ctx, OMDbQueryType.Title, title);
        #endregion

        #region COMMAND_IMDB_ID
        [Command("id"), Module(ModuleType.Searches)]
        [Description("Search by IMDb ID.")]
        [UsageExamples("!imdb id tt4158110")]
        public Task SearchByIdAsync(CommandContext ctx,
                                   [Description("ID.")] string id)
            => SearchAndSendResultAsync(ctx, OMDbQueryType.Id, id);
        #endregion


        #region HELPER_FUNCTIONS
        private async Task SearchAndSendResultAsync(CommandContext ctx, OMDbQueryType type, string query)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            var info = await this.Service.GetSingleResultAsync(type, query)
                .ConfigureAwait(false);

            if (info == null)
                throw new CommandFailedException("No results found!");

            await ctx.RespondAsync(embed: info.ToDiscordEmbed())
                .ConfigureAwait(false);
        }
        #endregion
    }
}
