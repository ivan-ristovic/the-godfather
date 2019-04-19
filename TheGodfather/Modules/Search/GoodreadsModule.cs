#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
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
    [UsageExamples("!goodreads Ender's Game")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class GoodreadsModule : TheGodfatherServiceModule<GoodreadsService>
    {

        public GoodreadsModule(GoodreadsService goodreads, SharedData shared, DatabaseContextBuilder db)
            : base(goodreads, shared, db)
        {
            this.ModuleColor = DiscordColor.DarkGray;
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Query.")] string query)
            => this.SearchBookAsync(ctx, query);


        #region COMMAND_GOODREADS_BOOK
        [Command("book")]
        [Description("Search Goodreads books by title, author, or ISBN.")]
        [Aliases("books", "b")]
        [UsageExamples("!goodreads book Ender's Game")]
        public async Task SearchBookAsync(CommandContext ctx,
                                         [RemainingText, Description("Query.")] string query)
        {
            if (this.Service.IsDisabled())
                throw new ServiceDisabledException();

            var res = await this.Service.SearchBooksAsync(query);
            await ctx.Client.GetInteractivity().SendPaginatedMessageAsync(ctx.Channel, ctx.User, res.ToDiscordPages());
        }
        #endregion
    }
}
