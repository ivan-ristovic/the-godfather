#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
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

        public GoodreadsModule(GoodreadsService goodreads, SharedData shared, DBService db)
            : base(goodreads, shared, db)
        {
            this.ModuleColor = DiscordColor.DarkGray;
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Query.")] string query)
            => SearchBookAsync(ctx, query);


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
            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, res.ToDiscordPages());
        }
        #endregion
    }
}
