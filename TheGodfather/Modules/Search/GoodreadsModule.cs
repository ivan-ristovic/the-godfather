using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("goodreads")]
[Alias("gr")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class GoodreadsModule : TheGodfatherModule
{
    private readonly GoodreadsService goodreads;


    public GoodreadsModule(LocalizationService localization, GoodreadsService goodreads)
        : base(localization, new Color(0x1abc9c))
    {
        this.goodreads = goodreads;
    }


    [Command("")]
    public Task ExecuteGroupAsync([Remainder] string query)
        => this.SearchBookAsync(query);

    [Command("book")]
    [Alias("books", "b")]
    public async Task SearchBookAsync([Remainder] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new LocalizedCommandException(TranslationKey.cmd_err_query);

        GoodreadsSearchInfo? res = await this.goodreads.SearchBooksAsync(query);
        if (res is null) {
            await this.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await this.RespondPagedAsync(res.Results, (emb, result) => {
            emb.WithTitle(result.Book.Title);
            emb.WithThumbnail(result.Book.ImageUrl);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_author, result.Book.Author.Name, true);
            emb.AddLocalizedField(TranslationKey.str_rating, result.AverageRating, true);
            emb.AddLocalizedField(TranslationKey.str_books_count, result.BooksCount, true);
            if (DateTimeOffset.TryParse($"{result.PublicationDayString}.{result.PublicationMonthString}.{result.PublicationYearString}", out DateTimeOffset dt))
                emb.AddLocalizedField(TranslationKey.str_published, dt.Humanize(culture: this.Localization.GetGuildCulture(this.Context.Guild?.Id)), true);
            emb.AddLocalizedField(TranslationKey.str_work_id, result.Id, true);
            emb.AddLocalizedField(TranslationKey.str_book_id, result.Book.Id, true);
            emb.AddLocalizedField(TranslationKey.str_reviews, result.TextReviewsCount, true);
            emb.WithLocalizedFooter(TranslationKey.str_footer_gr(res.QueryTime), null);
            return emb;
        });
    }
}
