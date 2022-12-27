using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using GiphyDotNet.Model.GiphyImage;

namespace TheGodfather.Modules.Search.Extensions;

internal static class CommandContextExtensions
{
    public static Task SendFirstGifAsync(this CommandContext ctx, Data[]? res)
    {
        return ctx.SendGifAsync(res?.FirstOrDefault());
    }
    
    public static async Task SendGifAsync(this CommandContext ctx, Data? res)
    {
        if (res?.Url is null)
            await ctx.FailAsync(TranslationKey.cmd_err_res_none);
        else
            await ctx.RespondAsync(res.Url);
    }
    
    public static async Task PaginateGiphyData(this CommandContext ctx, Data[]? res, DiscordColor moduleColor)
    {
        if (res is null || !res.Any()) {
            await ctx.FailAsync(TranslationKey.cmd_err_res_none);
            return;
        }

        await ctx.PaginateAsync(res, (emb, r) => {
            emb.WithLocalizedTitle(TranslationKey.str_trending);
            emb.WithDescription(r.Caption, false);
            emb.WithColor(moduleColor);
            if (r.Images?.DownsizedLarge?.Url is not null)
                emb.WithImageUrl(r.Images.DownsizedLarge.Url);
            emb.AddLocalizedField(TranslationKey.str_posted_by, r.Username, true);
            emb.AddLocalizedField(TranslationKey.str_rating, r.Rating, true);
            if (DateTimeOffset.TryParse(r.TrendingDatetime, out DateTimeOffset dt))
                emb.WithLocalizedTimestamp(dt);
            if (r.Url is not null)
                emb.WithUrl(r.Url);
            return emb;
        });
    }
}