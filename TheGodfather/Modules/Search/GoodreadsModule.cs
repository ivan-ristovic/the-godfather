#region USING_DIRECTIVES
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using TheGodfather.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("goodreads"), Module(ModuleType.Searches), NotBlocked]
    [Description("Goodreads commands. Group call searches Goodreads books with given query.")]
    [Aliases("gr")]

    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class GoodreadsModule : TheGodfatherServiceModule<GoodreadsService>
    {

        public GoodreadsModule(GoodreadsService service, DbContextBuilder db)
            : base(service, db)
        {

        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Query.")] string query)
            => this.SearchBookAsync(ctx, query);


        #region COMMAND_GOODREADS_BOOK
        [Command("book")]
        [Description("Search Goodreads books by title, author, or ISBN.")]
        [Aliases("books", "b")]

        public async Task SearchBookAsync(CommandContext ctx,
                                         [RemainingText, Description("Query.")] string query)
        {
            if (this.Service.IsDisabled)
                throw new ServiceDisabledException(ctx);

            Common.GoodreadsSearchInfo res = await this.Service.SearchBooksAsync(query);
            await ctx.Client.GetInteractivity().SendPaginatedMessageAsync(ctx.Channel, ctx.User, res.ToDiscordPages());
        }
        #endregion
    }
}
