using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("imgur")]
[Alias("img", "im", "i")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class ImgurModule : TheGodfatherModule
{
    private readonly ImgurService imgur;


    public ImgurModule(LocalizationService localization, ImgurService imgur)
        : base(localization, new Color(0x1abc9c))
    {
        this.imgur = imgur;
    }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(int amount, [Remainder] string sub)
        => this.SearchSubAsync(sub, amount, SubredditGallerySortOrder.Top, TimeWindow.Day);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync(string sub, int amount = 1)
        => this.SearchSubAsync(sub, amount, SubredditGallerySortOrder.Top, TimeWindow.Day);

    [Command("latest")]
    [Priority(1)]
    [Alias("l", "new", "newest")]
    public Task LatestAsync(int amount, [Remainder] string sub)
        => this.SearchSubAsync(sub, amount, SubredditGallerySortOrder.Time, TimeWindow.Day);

    [Command("latest")]
    [Priority(0)]
    public Task LatestAsync(string sub, int amount = 1)
        => this.LatestAsync(amount, sub);

    [Command("top")]
    [Priority(3)]
    [Alias("t")]
    public Task TopAsync(TimeWindow timespan, int amount, [Remainder] string sub)
        => this.SearchSubAsync(sub, amount, SubredditGallerySortOrder.Top, timespan);

    [Command("top")]
    [Priority(2)]
    public Task TopAsync(TimeWindow timespan, string sub, int amount = 1)
        => this.TopAsync(timespan, amount, sub);

    [Command("top")]
    [Priority(1)]
    public Task TopAsync(int amount, TimeWindow timespan, [Remainder] string sub)
        => this.TopAsync(timespan, amount, sub);

    [Command("top")]
    [Priority(0)]
    public Task TopAsync(int amount, [Remainder] string sub)
        => this.TopAsync(TimeWindow.Day, amount, sub);

    private async Task SearchSubAsync(string sub, int amount, SubredditGallerySortOrder sort, TimeWindow timeWindow)
    {
        if (string.IsNullOrWhiteSpace(sub))
            throw new LocalizedCommandException(TranslationKey.cmd_err_sub_none);

        IEnumerable<ImgurGalleryItem>? res = await this.imgur.GetItemsFromSubAsync(sub, amount, sort, timeWindow);
        await this.PrintImagesAsync(res);
    }

    private async Task PrintImagesAsync(IEnumerable<ImgurGalleryItem>? res)
    {
        if (res is null || !res.Any()) {
            await this.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await this.RespondPagedAsync(res.Take(20), (emb, item) => {
            emb.WithColor(this.ModuleColor);
            if (item is not ImgurGalleryImage and not ImgurGalleryAlbum)
                throw new LocalizedCommandException(TranslationKey.cmd_err_imgur);

            if ((item.Nsfw ?? false) && !this.IsNsfwChannel())
                throw new LocalizedCommandException(TranslationKey.cmd_err_nsfw);

            if (!string.IsNullOrWhiteSpace(item.Title))
                emb.WithTitle(item.Title);
            emb.WithDescription(item.Description, false);
            if (!string.IsNullOrWhiteSpace(item.Link))
                emb.WithImageUrl(item.Link);
            emb.WithLocalizedTimestamp(item.DateTime);
            emb.AddLocalizedField(TranslationKey.str_views, item.Views, true);
            emb.AddLocalizedField(TranslationKey.str_score, item.Score, true, false);
            emb.AddLocalizedField(TranslationKey.str_votes, $"{item.Points ?? 0} pts | {item.Ups ?? 0} up {item.Downs ?? 0} down", true);
            emb.AddLocalizedField(TranslationKey.str_comments, item.CommentCount, true, false);

            return emb;
        });
    }
}
