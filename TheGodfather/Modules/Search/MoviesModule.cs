#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("movies"), Module(ModuleType.Searches)]
    [Description("Search Open Movie Database.")]
    [Aliases("movie", "film", "cinema", "imdb", "omdb")]
    [UsageExample("!movie Kill Bill")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class MoviesModule : TheGodfatherServiceModule<MovieInfoService>
    {

        public MoviesModule(MovieInfoService omdb) : base(omdb) { }


        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Search uery.")] string query)
        {
            var pages = await _Service.GetPaginatedResultsAsync(query)
                .ConfigureAwait(false);

            if (pages == null)
                throw new CommandFailedException("No results found!");

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages)
                .ConfigureAwait(false);
        }
    }
}
