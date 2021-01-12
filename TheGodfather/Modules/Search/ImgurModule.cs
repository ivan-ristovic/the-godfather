using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Imgur.API.Enums;
using Imgur.API.Models;
using Imgur.API.Models.Impl;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search
{
    [Group("imgur"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("img", "im", "i")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class ImgurModule : TheGodfatherServiceModule<ImgurService>
    {
        #region imgur
        [GroupCommand, Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("desc-res-num")] int amount,
                                           [RemainingText, Description("desc-sub")] string sub)
        {
            if (string.IsNullOrWhiteSpace(sub)) {
                await ctx.FailAsync("cmd-err-sub");
                return;
            }

            IEnumerable<IGalleryItem>? res = await this.Service.GetItemsFromSubAsync(
                sub,
                amount,
                SubredditGallerySortOrder.Top,
                TimeWindow.Day
            );

            await this.PrintImagesAsync(ctx, res);
        }

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-sub")] string sub,
                                     [Description("desc-res-num")] int n = 1)
            => this.ExecuteGroupAsync(ctx, n, sub);
        #endregion

        #region imgur latest
        [Command("latest"), Priority(1)]
        [Aliases("l", "new", "newest")]
        public async Task LatestAsync(CommandContext ctx,
                                     [Description("desc-res-num")] int amount,
                                     [RemainingText, Description("desc-sub")] string sub)
        {
            if (string.IsNullOrWhiteSpace(sub)) {
                await ctx.FailAsync("cmd-err-sub");
                return;
            }

            IEnumerable<IGalleryItem>? res = await this.Service.GetItemsFromSubAsync(sub, amount, SubredditGallerySortOrder.Time, TimeWindow.Day);
            await this.PrintImagesAsync(ctx, res);
        }

        [Command("latest"), Priority(0)]
        public Task LatestAsync(CommandContext ctx,
                               [Description("desc-sub")] string sub,
                               [Description("desc-res-num")] int n)
            => this.LatestAsync(ctx, n, sub);
        #endregion

        #region imgur top
        [Command("top"), Priority(3)]
        [Aliases("t")]
        public async Task TopAsync(CommandContext ctx,
                                  [Description("desc-timewindow")] TimeWindow timespan,
                                  [Description("desc-res-num")] int amount,
                                  [RemainingText, Description("desc-sub")] string sub)
        {
            if (string.IsNullOrWhiteSpace(sub)) {
                await ctx.FailAsync("cmd-err-sub");
                return;
            }

            IEnumerable<IGalleryItem>? res = await this.Service.GetItemsFromSubAsync(sub, amount, SubredditGallerySortOrder.Time, timespan);
            await this.PrintImagesAsync(ctx, res);
        }

        [Command("top"), Priority(2)]
        public Task TopAsync(CommandContext ctx,
                            [Description("desc-timewindow")] TimeWindow timespan,
                            [Description("desc-sub")] string sub,
                            [Description("desc-res-num")] int amount = 1)
            => this.TopAsync(ctx, timespan, amount, sub);

        [Command("top"), Priority(1)]
        public Task TopAsync(CommandContext ctx,
                            [Description("desc-res-num")] int amount,
                            [Description("desc-timewindow")] TimeWindow timespan,
                            [RemainingText, Description("desc-sub")] string sub)
            => this.TopAsync(ctx, timespan, amount, sub);

        [Command("top"), Priority(0)]
        public Task TopAsync(CommandContext ctx,
                            [Description("desc-res-num")] int amount,
                            [RemainingText, Description("desc-sub")] string sub)
            => this.TopAsync(ctx, TimeWindow.Day, amount, sub);

        #endregion


        #region internals
        private Task PrintImagesAsync(CommandContext ctx, IEnumerable<IGalleryItem>? res)
        {
            if (res is null || !res.Any()) 
                return ctx.FailAsync("cmd-err-res-none");

            return ctx.PaginateAsync(res.Take(20), (emb, r) => {
                emb.WithColor(this.ModuleColor);
                if (r is GalleryImage img) {
                    if ((img.Nsfw ?? false) && !ctx.Channel.IsNsfwOrNsfwName())
                        throw new CommandFailedException(ctx, "cmd-err-nsfw");
                    emb.WithTitle(img.Title);
                    emb.WithDescription(img.Description, unknown: false);
                    emb.WithImageUrl(img.Link);
                    emb.WithLocalizedTimestamp(img.DateTime);
                    emb.AddLocalizedTitleField("str-views", img.Views, inline: true);
                    emb.AddLocalizedTitleField("str-score", img.Score, inline: true, unknown: false);
                    emb.AddLocalizedTitleField("str-votes", $"{img.Points ?? 0} pts | {img.Ups ?? 0} ⬆️ {img.Downs ?? 0} ⬇️", inline: true);
                    emb.AddLocalizedTitleField("str-comments", img.CommentCount, inline: true, unknown: false);
                } else if (r is GalleryAlbum album) {
                    if ((album.Nsfw ?? false) & !ctx.Channel.IsNsfwOrNsfwName())
                        throw new CommandFailedException(ctx, "cmd-err-nsfw");
                    emb.WithTitle(album.Title);
                    emb.WithDescription(album.Description, unknown: false);
                    emb.WithImageUrl(album.Link);
                    emb.WithLocalizedTimestamp(album.DateTime);
                    emb.AddLocalizedTitleField("str-views", album.Views, inline: true);
                    emb.AddLocalizedTitleField("str-score", album.Score, inline: true, unknown: false);
                    emb.AddLocalizedTitleField("str-votes", $"{album.Points ?? 0} pts | {album.Ups ?? 0} ⬆️ {album.Downs ?? 0} ⬇️", inline: true);
                    emb.AddLocalizedTitleField("str-comments", album.CommentCount, inline: true, unknown: false);
                } else {
                    throw new CommandFailedException(ctx, "cmd-err-imgur");
                }
                return emb;
            });
        }
        #endregion
    }
}
