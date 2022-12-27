using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using GiphyDotNet.Model.GiphyImage;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("gif")][Module(ModuleType.Searches)][NotBlocked]
[Aliases("giphy")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class GifModule : TheGodfatherServiceModule<GiphyService>
{
    #region gif
    [GroupCommand]
    public async Task ExecuteGroupAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_query)] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_query);

        Data[]? res = await this.Service.SearchGifAsync(query);
        await ctx.SendFirstGifAsync(res);
    }
    #endregion

    #region gif random
    [Command("random")]
    [Aliases("r", "rand", "rnd", "rng")]
    public async Task RandomAsync(CommandContext ctx)
    {
        Data? res = await this.Service.GetRandomGifAsync();
        await ctx.SendGifAsync(res);
    }
    #endregion

    #region gif trending
    [Command("trending")]
    [Aliases("t", "tr", "trend")]
    public async Task TrendingAsync(CommandContext ctx,
        [Description(TranslationKey.desc_res_num)] int amount = 5)
    {
        Data[]? res = await this.Service.GetTrendingGifsAsync(amount);
        await ctx.PaginateGiphyData(res, this.ModuleColor);
    }
    #endregion
}

[Group("sticker")][Module(ModuleType.Searches)][NotBlocked]
[Aliases("stickers")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class StickerModule : TheGodfatherServiceModule<GiphyService>
{
    #region sticker
    [GroupCommand]
    public async Task ExecuteGroupAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_query)] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_query);

        Data[]? res = await this.Service.SearchStickerAsync(query);
        await ctx.SendFirstGifAsync(res);
    }
    #endregion

    #region sticker random
    [Command("random")]
    [Aliases("r", "rand", "rnd", "rng")]
    public async Task RandomAsync(CommandContext ctx)
    {
        Data? res = await this.Service.GetRandomStickerAsync();
        await ctx.SendGifAsync(res);
    }
    #endregion

    #region sticker trending
    [Command("trending")]
    [Aliases("t", "tr", "trend")]
    public async Task TrendingAsync(CommandContext ctx,
        [Description(TranslationKey.desc_res_num)] int amount = 5)
    {
        Data[]? res = await this.Service.GetTrendingStickerssAsync(amount);
        await ctx.PaginateGiphyData(res, this.ModuleColor);
    }
    #endregion
}