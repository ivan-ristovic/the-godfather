using System.Globalization;
using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Exceptions;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("cryptocurrency")]
[Alias("crypto")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class CryptoCurrencyModule : TheGodfatherModule
{
    private readonly CryptoCurrencyService crypto;


    public CryptoCurrencyModule(LocalizationService localization, CryptoCurrencyService crypto)
        : base(localization, new Color(0x1abc9c))
    {
        this.crypto = crypto;
    }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("")]
    [Priority(0)]
    public async Task ExecuteGroupAsync([Remainder] string currency)
    {
        CryptoResponseData? res;
        try {
            res = await this.crypto.SearchAsync(currency);
        } catch (SearchServiceException<CryptoResponseStatus> e) {
            throw new LocalizedCommandException(TranslationKey.cmd_err_bad_req($"{e.Details.ErrorCode}: {e.Details.ErrorMessage}"));
        }

        if (res is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_res_none);

        await this.RespondWithLocalizedEmbedAsync(emb => this.AddDataToEmbed(res, emb));
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync(int from = 0)
    {
        if (from is < 0 or > CryptoCurrencyService.CurrencyPoolSize)
            throw new LocalizedCommandException(TranslationKey.cmd_err_index(0, CryptoCurrencyService.CurrencyPoolSize));

        IReadOnlyList<CryptoResponseData>? res;
        try {
            res = await this.crypto.GetAllAsync(from);
        } catch (SearchServiceException<CryptoResponseStatus> e) {
            throw new LocalizedCommandException(TranslationKey.cmd_err_bad_req($"{e.Details.ErrorCode}: {e.Details.ErrorMessage}"));
        }

        if (res is null || !res.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_res_none);

        await this.RespondPagedAsync(res, (emb, data) => this.AddDataToEmbed(data, emb));
    }

    private LocalizedEmbedBuilder AddDataToEmbed(CryptoResponseData res, LocalizedEmbedBuilder emb)
    {
        CultureInfo culture = this.Localization.GetGuildCulture(this.Context.Guild?.Id);
        string percentMonth = FormatDecimal(res.Quotes.USD.PercentChangeMonth);
        string percentWeek = FormatDecimal(res.Quotes.USD.PercentChangeWeek);
        string percentDay = FormatDecimal(res.Quotes.USD.PercentChangeDay);
        string percentHour = FormatDecimal(res.Quotes.USD.PercentChangeHour);

        emb.WithTitle($"{res.Name} ({res.Symbol})");
        emb.WithColor(this.ModuleColor);
        emb.WithThumbnail(this.crypto.GetCoinUrl(res));
        emb.WithUrl(this.crypto.GetSlugUrl(res));
        emb.WithImageUrl(this.crypto.GetWeekGraphUrl(res));
        emb.AddLocalizedField(TranslationKey.str_crypto_market_cap, $"{FormatAmount(res.Quotes.USD.MarketCap)}$", true);
        emb.AddLocalizedField(TranslationKey.str_crypto_price, $"{FormatAmount(res.Quotes.USD.Price)}$", true);
        if (res.Quotes.USD.VolumeDay is not null)
            emb.AddLocalizedField(TranslationKey.str_crypto_volume_24h, $"{FormatAmount(res.Quotes.USD.VolumeDay.Value)}$", true);
        emb.AddLocalizedField(TranslationKey.str_crypto_change, $"{percentHour}%/{percentDay}%/{percentWeek}%/{percentMonth}%", true);
        emb.WithLocalizedFooter(TranslationKey.fmt_last_updated_at(this.Localization.GetLocalizedTime(this.Context.Guild?.Id, res.UpdatedAt)), null);
        return emb;

        string FormatDecimal(string value)
            => decimal.TryParse(value, out decimal percent) ? percent.ToString("F2", culture.NumberFormat) : value;

        string FormatAmount(double value)
            => value.ToString("F2", culture.NumberFormat);
    }
}
