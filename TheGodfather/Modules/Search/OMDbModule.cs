#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using TheGodfather.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("imdb"), Module(ModuleType.Searches), NotBlocked]
    [Description("Search Open Movie Database. Group call searches by title.")]
    [Aliases("movies", "series", "serie", "movie", "film", "cinema", "omdb")]

    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class OMDbModule : TheGodfatherServiceModule<OMDbService>
    {

        public OMDbModule(OMDbService service, DbContextBuilder db)
            : base(service, db)
        {

        }


        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Title.")] string title)
            => this.SearchByTitleAsync(ctx, title);


        #region COMMAND_IMDB_SEARCH
        [Command("search")]
        [Description("Searches IMDb for given query and returns paginated results.")]
        [Aliases("s", "find")]

        public async Task SearchAsync(CommandContext ctx,
                                     [RemainingText, Description("Search query.")] string query)
        {
            if (this.Service.IsDisabled)
                throw new ServiceDisabledException(ctx);

            IReadOnlyList<Page> pages = await this.Service.GetPaginatedResultsAsync(query);
            if (pages is null) {
                await this.InformFailureAsync(ctx, "No results found!");
                return;
            }

            await ctx.Client.GetInteractivity().SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);
        }
        #endregion

        #region COMMAND_IMDB_TITLE
        [Command("title")]
        [Description("Search by title.")]
        [Aliases("t", "name", "n")]

        public Task SearchByTitleAsync(CommandContext ctx,
                                      [RemainingText, Description("Title.")] string title)
            => this.SearchAndSendResultAsync(ctx, OMDbQueryType.Title, title);
        #endregion

        #region COMMAND_IMDB_ID
        [Command("id")]
        [Description("Search by IMDb ID.")]

        public Task SearchByIdAsync(CommandContext ctx,
                                   [Description("ID.")] string id)
            => this.SearchAndSendResultAsync(ctx, OMDbQueryType.Id, id);
        #endregion


        #region HELPER_FUNCTIONS
        private async Task SearchAndSendResultAsync(CommandContext ctx, OMDbQueryType type, string query)
        {
            if (this.Service.IsDisabled)
                throw new ServiceDisabledException(ctx);

            MovieInfo info = await this.Service.GetSingleResultAsync(type, query);
            if (info is null) {
                await this.InformFailureAsync(ctx, "No results found!");
                return;
            }

            await ctx.RespondAsync(embed: info.ToDiscordEmbed(this.ModuleColor));
        }
        #endregion
    }
}
