using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search
{
    [Group("goodreads"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("gr")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class GoodreadsModule : TheGodfatherServiceModule<GoodreadsService>
    {
        #region goodreads
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-query")] string query)
            => this.SearchBookAsync(ctx, query);
        #endregion

        #region goodreads book
        [Command("book")]
        [Aliases("books", "b")]
        public async Task SearchBookAsync(CommandContext ctx,
                                         [RemainingText, Description("desc-query")] string query)
        {
            if (this.Service.IsDisabled)
                throw new ServiceDisabledException(ctx);

            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException(ctx, "cmd-err-query");

            GoodreadsSearchInfo? res = await this.Service.SearchBooksAsync(query);
            if (res is null) {
                await ctx.FailAsync("cmd-err-res-none");
                return;
            }

            await ctx.PaginateAsync(res.Results, (emb, r) => {
                emb.WithTitle(r.Book.Title);
                emb.WithThumbnail(r.Book.ImageUrl);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedTitleField("str-author", r.Book.Author.Name, inline: true);
                emb.AddLocalizedTitleField("str-rating", r.AverageRating, inline: true);
                emb.AddLocalizedTitleField("str-books-count", r.BooksCount, inline: true);
                if (DateTimeOffset.TryParse($"{r.PublicationDayString}.{r.PublicationMonthString}.{r.PublicationYearString}", out DateTimeOffset dt))
                    emb.AddLocalizedTitleField("str-published", dt.Humanize(culture: this.Localization.GetGuildCulture(ctx.Guild.Id)), inline: true);
                emb.AddLocalizedTitleField("str-work-id", r.Id, inline: true);
                emb.AddLocalizedTitleField("str-book-id", r.Book.Id, inline: true);
                emb.AddLocalizedTitleField("str-reviews", r.TextReviewsCount, inline: true);
                emb.WithLocalizedFooter("str-footer-gr", null, res.QueryTime);
                return emb;
            });
        }
        #endregion
    }
}
