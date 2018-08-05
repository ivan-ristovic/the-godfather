#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;
using TheGodfather.Services.Common;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("imdb"), Module(ModuleType.Searches), NotBlocked]
    [Description("Search Open Movie Database. Group call searches by title.")]
    [Aliases("movies", "series", "serie", "movie", "film", "cinema", "omdb")]
    [UsageExamples("!imdb Airplane")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class OMDbModule : TheGodfatherServiceModule<OMDbService>
    {

        public OMDbModule(OMDbService omdb, SharedData shared, DBService db)
            : base(omdb, shared, db)
        {
            this.ModuleColor = DiscordColor.Yellow;
        }


        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Title.")] string title)
            => SearchByTitleAsync(ctx, title);


        #region COMMAND_IMDB_SEARCH
        [Command("search")]
        [Description("Searches IMDb for given query and returns paginated results.")]
        [Aliases("s", "find")]
        [UsageExamples("!imdb search Kill Bill")]
        public async Task SearchAsync(CommandContext ctx,
                                     [RemainingText, Description("Search query.")] string query)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            IReadOnlyList<Page> pages = await this.Service.GetPaginatedResultsAsync(query);
            if (pages == null) {
                await InformFailureAsync(ctx, "No results found!");
                return;
            }

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages);
        }
        #endregion

        #region COMMAND_IMDB_TITLE
        [Command("title")]
        [Description("Search by title.")]
        [Aliases("t", "name", "n")]
        [UsageExamples("!imdb title Airplane")]
        public Task SearchByTitleAsync(CommandContext ctx,
                                      [RemainingText, Description("Title.")] string title)
            => SearchAndSendResultAsync(ctx, OMDbQueryType.Title, title);
        #endregion

        #region COMMAND_IMDB_ID
        [Command("id")]
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

            MovieInfo info = await this.Service.GetSingleResultAsync(type, query);
            if (info == null) {
                await InformFailureAsync(ctx, "No results found!");
                return;
            }

            await ctx.RespondAsync(embed: info.ToDiscordEmbed(this.ModuleColor));
        }
        #endregion
    }
}
