#region USING_DIRECTIVES
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("goodreads"), Module(ModuleType.Searches)]
    [Description("Goodreads commands. If invoked without a subcommand, searches Goodreads books with given query.")]
    [Aliases("gr")]
    [UsageExamples("!goodreads Ender's Game")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class GoodreadsModule : TheGodfatherServiceModule<GoodreadsService>
    {

        public GoodreadsModule(GoodreadsService goodreads, SharedData shared, DBService db)
            : base(goodreads, shared, db) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Query.")] string query)
            => SearchBookAsync(ctx, query);


        #region COMMAND_GOODREADS_BOOK
        [Command("book"), Module(ModuleType.Searches)]
        [Description("Search Goodreads books by title, author, or ISBN.")]
        [Aliases("books", "b")]
        [UsageExamples("!goodreads book Ender's Game")]
        public async Task SearchBookAsync(CommandContext ctx,
                                         [RemainingText, Description("Query.")] string query)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            var res = await this.Service.SearchBooksAsync(query)
                .ConfigureAwait(false);
            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, res.ToPaginatedList())
                .ConfigureAwait(false);
        }
        #endregion
    }
}
