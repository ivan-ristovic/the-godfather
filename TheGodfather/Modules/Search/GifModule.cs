using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search
{
    [Group("gif"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("giphy")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class GifModule : TheGodfatherServiceModule<GiphyService>
    {
        #region gif
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description(TranslationKey.desc_query)] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_query);

            GiphyDotNet.Model.GiphyImage.Data[]? res = await this.Service.SearchGifAsync(query);
            if (res?.Any() ?? false)
                await ctx.RespondAsync(res.First().Url);
            else
                await ctx.FailAsync(TranslationKey.cmd_err_res_none);
        }
        #endregion

        #region gif random
        [Command("random")]
        [Aliases("r", "rand", "rnd", "rng")]
        public async Task RandomAsync(CommandContext ctx)
        {
            GiphyDotNet.Model.GiphyRandomImage.Data? res = await this.Service.GetRandomGifAsync();
            if (res is null)
                await ctx.FailAsync(TranslationKey.cmd_err_res_none);
            else
                await ctx.RespondAsync(res?.Url);
        }
        #endregion

        #region gif trending
        [Command("trending")]
        [Aliases("t", "tr", "trend")]
        public async Task TrendingAsync(CommandContext ctx,
                                       [Description(TranslationKey.desc_res_num)] int amount = 5)
        {
            GiphyDotNet.Model.GiphyImage.Data[]? res = await this.Service.GetTrendingGifsAsync(amount);

            if (res is null || !res.Any()) {
                await ctx.FailAsync(TranslationKey.cmd_err_res_none);
                return;
            }

            await ctx.PaginateAsync(res, (emb, r) => {
                emb.WithLocalizedTitle(TranslationKey.str_trending);
                emb.WithDescription(r.Caption, unknown: false);
                emb.WithColor(this.ModuleColor);
                emb.WithImageUrl(r.Images.DownsizedLarge.Url);
                emb.AddLocalizedField(TranslationKey.str_posted_by, r.Username, inline: true);
                emb.AddLocalizedField(TranslationKey.str_rating, r.Rating, inline: true);
                if (DateTimeOffset.TryParse(r.TrendingDatetime, out DateTimeOffset dt))
                    emb.WithLocalizedTimestamp(dt);
                emb.WithUrl(r.Url);
                return emb;
            });
        }
        #endregion
    }

    [Group("sticker"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("stickers")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class StickerModule : TheGodfatherServiceModule<GiphyService>
    {
        #region sticker
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description(TranslationKey.desc_query)] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_query);

            GiphyDotNet.Model.GiphyImage.Data[]? res = await this.Service.SearchStickerAsync(query);
            if (res?.Any() ?? false)
                await ctx.RespondAsync(res.First().Url);
            else
                await ctx.FailAsync(TranslationKey.cmd_err_res_none);
        }
        #endregion

        #region sticker random
        [Command("random")]
        [Aliases("r", "rand", "rnd", "rng")]
        public async Task RandomAsync(CommandContext ctx)
        {
            GiphyDotNet.Model.GiphyRandomImage.Data? res = await this.Service.GetRandomStickerAsync();
            if (res is null)
                await ctx.FailAsync(TranslationKey.cmd_err_res_none);
            else
                await ctx.RespondAsync(res?.Url);
        }
        #endregion

        #region sticker trending
        [Command("trending")]
        [Aliases("t", "tr", "trend")]
        public async Task TrendingAsync(CommandContext ctx,
                                       [Description(TranslationKey.desc_res_num)] int amount = 5)
        {
            GiphyDotNet.Model.GiphyImage.Data[]? res = await this.Service.GetTrendingStickerssAsync(amount);

            if (res is null || !res.Any()) {
                await ctx.FailAsync(TranslationKey.cmd_err_res_none);
                return;
            }

            await ctx.PaginateAsync(res, (emb, r) => {
                emb.WithLocalizedTitle(TranslationKey.str_trending);
                emb.WithDescription(r.Caption, unknown: false);
                emb.WithColor(this.ModuleColor);
                emb.WithImageUrl(r.Images.DownsizedLarge.Url);
                emb.AddLocalizedField(TranslationKey.str_posted_by, r.Username, inline: true);
                emb.AddLocalizedField(TranslationKey.str_rating, r.Rating, inline: true);
                if (DateTimeOffset.TryParse(r.TrendingDatetime, out DateTimeOffset dt))
                    emb.WithLocalizedTimestamp(dt);
                emb.WithUrl(r.Url);
                return emb;
            });
        }
        #endregion
    }
}
