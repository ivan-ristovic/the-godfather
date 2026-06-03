using Discord.Commands;
using GiphyDotNet.Model.GiphyImage;
using TheGodfather.Attributes;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("gif")]
[Alias("giphy")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class GifModule : TheGodfatherModule
{
    private readonly GiphyService giphy;


    public GifModule(LocalizationService localization, GiphyService giphy)
        : base(localization, new Color(0x1abc9c))
    {
        this.giphy = giphy;
    }


    [Command("")]
    public async Task ExecuteGroupAsync([Remainder] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new LocalizedCommandException(TranslationKey.cmd_err_query);

        Data[]? res = await this.giphy.SearchGifAsync(query);
        await this.SendFirstGifAsync(res);
    }

    [Command("random")]
    [Alias("r", "rand", "rnd", "rng")]
    public async Task RandomAsync()
    {
        Data? res = await this.giphy.GetRandomGifAsync();
        await this.SendGifAsync(res);
    }

    [Command("trending")]
    [Alias("t", "tr", "trend")]
    public async Task TrendingAsync(int amount = 5)
    {
        Data[]? res = await this.giphy.GetTrendingGifsAsync(amount);
        await this.PaginateGiphyDataAsync(res);
    }

    private Task SendFirstGifAsync(Data[]? res)
        => this.SendGifAsync(res?.FirstOrDefault());

    private async Task SendGifAsync(Data? res)
    {
        if (res?.Url is null)
            await this.FailAsync(TranslationKey.cmd_err_res_none);
        else
            await this.ReplyAsync(res.Url);
    }

    private async Task PaginateGiphyDataAsync(Data[]? res)
    {
        if (res is null || !res.Any()) {
            await this.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await this.RespondPagedAsync(res, (emb, gif) => {
            emb.WithLocalizedTitle(TranslationKey.str_trending);
            emb.WithDescription(gif.Caption, false);
            emb.WithColor(this.ModuleColor);
            if (gif.Images?.DownsizedLarge?.Url is not null)
                emb.WithImageUrl(gif.Images.DownsizedLarge.Url);
            emb.AddLocalizedField(TranslationKey.str_posted_by, gif.Username, true);
            emb.AddLocalizedField(TranslationKey.str_rating, gif.Rating, true);
            if (DateTimeOffset.TryParse(gif.TrendingDatetime, out DateTimeOffset dt))
                emb.WithLocalizedTimestamp(dt);
            if (gif.Url is not null)
                emb.WithUrl(gif.Url);
            return emb;
        });
    }
}

[Group("sticker")]
[Alias("stickers")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class StickerModule : TheGodfatherModule
{
    private readonly GiphyService giphy;


    public StickerModule(LocalizationService localization, GiphyService giphy)
        : base(localization, new Color(0x1abc9c))
    {
        this.giphy = giphy;
    }


    [Command("")]
    public async Task ExecuteGroupAsync([Remainder] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new LocalizedCommandException(TranslationKey.cmd_err_query);

        Data[]? res = await this.giphy.SearchStickerAsync(query);
        await this.SendFirstGifAsync(res);
    }

    [Command("random")]
    [Alias("r", "rand", "rnd", "rng")]
    public async Task RandomAsync()
    {
        Data? res = await this.giphy.GetRandomStickerAsync();
        await this.SendGifAsync(res);
    }

    [Command("trending")]
    [Alias("t", "tr", "trend")]
    public async Task TrendingAsync(int amount = 5)
    {
        Data[]? res = await this.giphy.GetTrendingStickerssAsync(amount);
        await this.PaginateGiphyDataAsync(res);
    }

    private Task SendFirstGifAsync(Data[]? res)
        => this.SendGifAsync(res?.FirstOrDefault());

    private async Task SendGifAsync(Data? res)
    {
        if (res?.Url is null)
            await this.FailAsync(TranslationKey.cmd_err_res_none);
        else
            await this.ReplyAsync(res.Url);
    }

    private async Task PaginateGiphyDataAsync(Data[]? res)
    {
        if (res is null || !res.Any()) {
            await this.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await this.RespondPagedAsync(res, (emb, gif) => {
            emb.WithLocalizedTitle(TranslationKey.str_trending);
            emb.WithDescription(gif.Caption, false);
            emb.WithColor(this.ModuleColor);
            if (gif.Images?.DownsizedLarge?.Url is not null)
                emb.WithImageUrl(gif.Images.DownsizedLarge.Url);
            emb.AddLocalizedField(TranslationKey.str_posted_by, gif.Username, true);
            emb.AddLocalizedField(TranslationKey.str_rating, gif.Rating, true);
            if (DateTimeOffset.TryParse(gif.TrendingDatetime, out DateTimeOffset dt))
                emb.WithLocalizedTimestamp(dt);
            if (gif.Url is not null)
                emb.WithUrl(gif.Url);
            return emb;
        });
    }
}
