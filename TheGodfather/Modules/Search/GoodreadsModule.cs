using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
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
                                     [RemainingText, Description(TranslationKey.desc_query)] string query)
            => this.SearchBookAsync(ctx, query);
        #endregion

        #region goodreads book
        [Command("book")]
        [Aliases("books", "b")]
        public async Task SearchBookAsync(CommandContext ctx,
                                         [RemainingText, Description(TranslationKey.desc_query)] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_query);

            GoodreadsSearchInfo? res = await this.Service.SearchBooksAsync(query);
            if (res is null) {
                await ctx.FailAsync(TranslationKey.cmd_err_res_none);
                return;
            }

            await ctx.PaginateAsync(res.Results, (emb, r) => {
                emb.WithTitle(r.Book.Title);
                emb.WithThumbnail(r.Book.ImageUrl);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_author, r.Book.Author.Name, inline: true);
                emb.AddLocalizedField(TranslationKey.str_rating, r.AverageRating, inline: true);
                emb.AddLocalizedField(TranslationKey.str_books_count, r.BooksCount, inline: true);
                if (DateTimeOffset.TryParse($"{r.PublicationDayString}.{r.PublicationMonthString}.{r.PublicationYearString}", out DateTimeOffset dt))
                    emb.AddLocalizedField(TranslationKey.str_published, dt.Humanize(culture: this.Localization.GetGuildCulture(ctx.Guild.Id)), inline: true);
                emb.AddLocalizedField(TranslationKey.str_work_id, r.Id, inline: true);
                emb.AddLocalizedField(TranslationKey.str_book_id, r.Book.Id, inline: true);
                emb.AddLocalizedField(TranslationKey.str_reviews, r.TextReviewsCount, inline: true);
                emb.WithLocalizedFooter(TranslationKey.str_footer_gr(res.QueryTime), null);
                return emb;
            });
        }
        #endregion
    }
}
