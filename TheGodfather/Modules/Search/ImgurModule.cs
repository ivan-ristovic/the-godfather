using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Imgur.API.Enums;
using Imgur.API.Models;
using Imgur.API.Models.Impl;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("imgur")][Module(ModuleType.Searches)][NotBlocked]
[Aliases("img", "im", "i")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class ImgurModule : TheGodfatherServiceModule<ImgurService>
{
    #region imgur
    [GroupCommand][Priority(1)]
    public async Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_res_num)] int amount,
        [RemainingText][Description(TranslationKey.desc_sub)] string sub)
    {
        if (string.IsNullOrWhiteSpace(sub))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_sub_none);

        IEnumerable<IGalleryItem>? res = await this.Service.GetItemsFromSubAsync(
            sub,
            amount,
            SubredditGallerySortOrder.Top,
            TimeWindow.Day
        );

        await this.PrintImagesAsync(ctx, res);
    }

    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_sub)] string sub,
        [Description(TranslationKey.desc_res_num)] int n = 1)
        => this.ExecuteGroupAsync(ctx, n, sub);
    #endregion

    #region imgur latest
    [Command("latest")][Priority(1)]
    [Aliases("l", "new", "newest")]
    public async Task LatestAsync(CommandContext ctx,
        [Description(TranslationKey.desc_res_num)] int amount,
        [RemainingText][Description(TranslationKey.desc_sub)] string sub)
    {
        if (string.IsNullOrWhiteSpace(sub)) 
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_sub_none);

        IEnumerable<IGalleryItem>? res = await this.Service.GetItemsFromSubAsync(sub, amount, SubredditGallerySortOrder.Time, TimeWindow.Day);
        await this.PrintImagesAsync(ctx, res);
    }

    [Command("latest")][Priority(0)]
    public Task LatestAsync(CommandContext ctx,
        [Description(TranslationKey.desc_sub)] string sub,
        [Description(TranslationKey.desc_res_num)] int n)
        => this.LatestAsync(ctx, n, sub);
    #endregion

    #region imgur top
    [Command("top")][Priority(3)]
    [Aliases("t")]
    public async Task TopAsync(CommandContext ctx,
        [Description(TranslationKey.desc_timewindow)] TimeWindow timespan,
        [Description(TranslationKey.desc_res_num)] int amount,
        [RemainingText][Description(TranslationKey.desc_sub)] string sub)
    {
        if (string.IsNullOrWhiteSpace(sub)) 
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_sub_none);

        IEnumerable<IGalleryItem>? res = await this.Service.GetItemsFromSubAsync(sub, amount, SubredditGallerySortOrder.Time, timespan);
        await this.PrintImagesAsync(ctx, res);
    }

    [Command("top")][Priority(2)]
    public Task TopAsync(CommandContext ctx,
        [Description(TranslationKey.desc_timewindow)] TimeWindow timespan,
        [Description(TranslationKey.desc_sub)] string sub,
        [Description(TranslationKey.desc_res_num)] int amount = 1)
        => this.TopAsync(ctx, timespan, amount, sub);

    [Command("top")][Priority(1)]
    public Task TopAsync(CommandContext ctx,
        [Description(TranslationKey.desc_res_num)] int amount,
        [Description(TranslationKey.desc_timewindow)] TimeWindow timespan,
        [RemainingText][Description(TranslationKey.desc_sub)] string sub)
        => this.TopAsync(ctx, timespan, amount, sub);

    [Command("top")][Priority(0)]
    public Task TopAsync(CommandContext ctx,
        [Description(TranslationKey.desc_res_num)] int amount,
        [RemainingText][Description(TranslationKey.desc_sub)] string sub)
        => this.TopAsync(ctx, TimeWindow.Day, amount, sub);

    #endregion


    #region internals
    private Task PrintImagesAsync(CommandContext ctx, IEnumerable<IGalleryItem>? res)
    {
        if (res is null || !res.Any()) 
            return ctx.FailAsync(TranslationKey.cmd_err_res_none);

        return ctx.PaginateAsync(res.Take(20), (emb, r) => {
            emb.WithColor(this.ModuleColor);
            if (r is GalleryImage img) {
                if ((img.Nsfw ?? false) && !ctx.Channel.IsNsfwOrNsfwName())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_nsfw);
                emb.WithTitle(img.Title);
                emb.WithDescription(img.Description, false);
                emb.WithImageUrl(img.Link);
                emb.WithLocalizedTimestamp(img.DateTime);
                emb.AddLocalizedField(TranslationKey.str_views, img.Views, true);
                emb.AddLocalizedField(TranslationKey.str_score, img.Score, true, false);
                emb.AddLocalizedField(TranslationKey.str_votes, $"{img.Points ?? 0} pts | {img.Ups ?? 0} ⬆️ {img.Downs ?? 0} ⬇️", true);
                emb.AddLocalizedField(TranslationKey.str_comments, img.CommentCount, true, false);
            } else if (r is GalleryAlbum album) {
                if ((album.Nsfw ?? false) & !ctx.Channel.IsNsfwOrNsfwName())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_nsfw);
                emb.WithTitle(album.Title);
                emb.WithDescription(album.Description, false);
                emb.WithImageUrl(album.Link);
                emb.WithLocalizedTimestamp(album.DateTime);
                emb.AddLocalizedField(TranslationKey.str_views, album.Views, true);
                emb.AddLocalizedField(TranslationKey.str_score, album.Score, true, false);
                emb.AddLocalizedField(TranslationKey.str_votes, $"{album.Points ?? 0} pts | {album.Ups ?? 0} ⬆️ {album.Downs ?? 0} ⬇️", true);
                emb.AddLocalizedField(TranslationKey.str_comments, album.CommentCount, true, false);
            } else {
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_imgur);
            }
            return emb;
        });
    }
    #endregion
}