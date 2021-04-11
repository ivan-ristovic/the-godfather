using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Exceptions;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search
{
    [Group("cryptocurrency"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("crypto")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class CryptoCurrencyModule : TheGodfatherServiceModule<CryptoCurrencyService>
    {
        #region cryptocurrency
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("desc-currency-name")] string currency)
        {
            CryptoResponseData? res = null;
            try {
                res = await this.Service.SearchAsync(currency);
            } catch (SearchServiceException<CryptoResponseStatus> e) {
                throw new CommandFailedException(ctx, "cmd-err-bad-req", $"{e.Details.ErrorCode}: {e.Details.ErrorMessage}");
            }

            if (res is null)
                throw new CommandFailedException(ctx, "cmd-err-res-none");

            CultureInfo culture = this.Localization.GetGuildCulture(ctx.Guild.Id);
            string percentMonth = FormatDecimal(res.Quotes.USD.PercentChangeMonth);
            string percentWeek = FormatDecimal(res.Quotes.USD.PercentChangeWeek);
            string percentDay = FormatDecimal(res.Quotes.USD.PercentChangeDay);
            string percentHour = FormatDecimal(res.Quotes.USD.PercentChangeHour);

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle($"{res.Name} ({res.Symbol})");
                emb.WithColor(this.ModuleColor);
                emb.WithThumbnail(this.Service.GetCoinUrl(res));
                emb.WithUrl(this.Service.GetSlugUrl(res));
                emb.WithImageUrl(this.Service.GetWeekGraphUrl(res));
                emb.AddLocalizedTitleField("str-crypto-market-cap", $"{FormatAmount(res.Quotes.USD.MarketCap)}$", inline: true);
                emb.AddLocalizedTitleField("str-crypto-price", $"{FormatAmount(res.Quotes.USD.Price)}$", inline: true);
                if (res.Quotes.USD.VolumeDay is { })
                    emb.AddLocalizedTitleField("str-crypto-volume-24h", $"{FormatAmount(res.Quotes.USD.VolumeDay.Value)}$", inline: true);
                emb.AddLocalizedTitleField("str-crypto-change", $"{percentHour}%/{percentDay}%/{percentWeek}%/{percentMonth}%", inline: true);
                emb.WithLocalizedFooter("fmt-last-updated-at", null, res.UpdatedAt.ToString(culture.DateTimeFormat));
            });


            string FormatDecimal(string value)
                => decimal.TryParse(value, out decimal percent) ? percent.ToString("F2", culture.NumberFormat) : value;

            string FormatAmount(double value)
                => value.ToString("F2", culture.NumberFormat);
        }
        #endregion
    }
}
