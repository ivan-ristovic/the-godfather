using System.Globalization;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Exceptions;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Search;

[Group("cryptocurrency")][Module(ModuleType.Searches)][NotBlocked]
[Aliases("crypto")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class CryptoCurrencyModule : TheGodfatherServiceModule<CryptoCurrencyService>
{
    #region cryptocurrency
    [GroupCommand][Priority(1)]
    public Task ExecuteGroupAsync(CommandContext ctx)
        => this.ListAsync(ctx);

    [GroupCommand][Priority(0)]
    public async Task ExecuteGroupAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_currency_name)] string currency)
    {
        CryptoResponseData? res;
        try {
            res = await this.Service.SearchAsync(currency);
        } catch (SearchServiceException<CryptoResponseStatus> e) {
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_bad_req($"{e.Details.ErrorCode}: {e.Details.ErrorMessage}"));
        }

        if (res is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_res_none);

        await ctx.RespondWithLocalizedEmbedAsync(emb => this.AddDataToEmbed(ctx, res, emb));
    }
    #endregion

    #region cryptocurrency list
    [Command("list")]
    [Aliases("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync(CommandContext ctx,
        [Description(TranslationKey.desc_start_index)] int from = 0)
    {
        if (from < 0 || from > CryptoCurrencyService.CurrencyPoolSize)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_index(0, CryptoCurrencyService.CurrencyPoolSize));

        IReadOnlyList<CryptoResponseData>? res;
        try {
            res = await this.Service.GetAllAsync(from);
        } catch (SearchServiceException<CryptoResponseStatus> e) {
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_bad_req($"{e.Details.ErrorCode}: {e.Details.ErrorMessage}"));
        }

        if (res is null || !res.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_res_none);

        await ctx.PaginateAsync(res, (emb, data) => this.AddDataToEmbed(ctx, data, emb));
    }
    #endregion


    #region internals
    private LocalizedEmbedBuilder AddDataToEmbed(CommandContext ctx, CryptoResponseData res, LocalizedEmbedBuilder emb)
    {
        CultureInfo culture = this.Localization.GetGuildCulture(ctx.Guild.Id);
        string percentMonth = FormatDecimal(res.Quotes.USD.PercentChangeMonth);
        string percentWeek = FormatDecimal(res.Quotes.USD.PercentChangeWeek);
        string percentDay = FormatDecimal(res.Quotes.USD.PercentChangeDay);
        string percentHour = FormatDecimal(res.Quotes.USD.PercentChangeHour);

        emb.WithTitle($"{res.Name} ({res.Symbol})");
        emb.WithColor(this.ModuleColor);
        emb.WithThumbnail(this.Service.GetCoinUrl(res));
        emb.WithUrl(this.Service.GetSlugUrl(res));
        emb.WithImageUrl(this.Service.GetWeekGraphUrl(res));
        emb.AddLocalizedField(TranslationKey.str_crypto_market_cap, $"{FormatAmount(res.Quotes.USD.MarketCap)}$", true);
        emb.AddLocalizedField(TranslationKey.str_crypto_price, $"{FormatAmount(res.Quotes.USD.Price)}$", true);
        if (res.Quotes.USD.VolumeDay is { })
            emb.AddLocalizedField(TranslationKey.str_crypto_volume_24h, $"{FormatAmount(res.Quotes.USD.VolumeDay.Value)}$", true);
        emb.AddLocalizedField(TranslationKey.str_crypto_change, $"{percentHour}%/{percentDay}%/{percentWeek}%/{percentMonth}%", true);
        emb.WithLocalizedFooter(TranslationKey.fmt_last_updated_at(this.Localization.GetLocalizedTime(ctx.Guild.Id, res.UpdatedAt)), null);
            
        return emb;


        string FormatDecimal(string value)
            => decimal.TryParse(value, out decimal percent) ? percent.ToString("F2", culture.NumberFormat) : value;

        string FormatAmount(double value)
            => value.ToString("F2", culture.NumberFormat);
    }
    #endregion
}