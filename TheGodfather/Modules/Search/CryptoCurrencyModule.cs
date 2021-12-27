using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Exceptions;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Search
{
    [Group("cryptocurrency"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("crypto")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class CryptoCurrencyModule : TheGodfatherServiceModule<CryptoCurrencyService>
    {
        #region cryptocurrency
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(0)]
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

            await ctx.RespondWithLocalizedEmbedAsync(emb => this.AddDataToEmbed(ctx, res, emb));
        }
        #endregion

        #region cryptocurrency list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("desc-start-index")] int from = 0)
        {
            if (from < 0 || from > CryptoCurrencyService.CurrencyPoolSize)
                throw new CommandFailedException(ctx, "cmd-err-index", 0, CryptoCurrencyService.CurrencyPoolSize);

            IReadOnlyList<CryptoResponseData>? res = null;
            try {
                res = await this.Service.GetAllAsync(start: from);
            } catch (SearchServiceException<CryptoResponseStatus> e) {
                throw new CommandFailedException(ctx, "cmd-err-bad-req", $"{e.Details.ErrorCode}: {e.Details.ErrorMessage}");
            }

            if (res is null || !res.Any())
                throw new CommandFailedException(ctx, "cmd-err-res-none");

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
            emb.AddLocalizedField("str-crypto-market-cap", $"{FormatAmount(res.Quotes.USD.MarketCap)}$", inline: true);
            emb.AddLocalizedField("str-crypto-price", $"{FormatAmount(res.Quotes.USD.Price)}$", inline: true);
            if (res.Quotes.USD.VolumeDay is { })
                emb.AddLocalizedField("str-crypto-volume-24h", $"{FormatAmount(res.Quotes.USD.VolumeDay.Value)}$", inline: true);
            emb.AddLocalizedField("str-crypto-change", $"{percentHour}%/{percentDay}%/{percentWeek}%/{percentMonth}%", inline: true);
            emb.WithLocalizedFooter("fmt-last-updated-at", null, this.Localization.GetLocalizedTime(ctx.Guild.Id, res.UpdatedAt));
            
            return emb;


            string FormatDecimal(string value)
                => decimal.TryParse(value, out decimal percent) ? percent.ToString("F2", culture.NumberFormat) : value;

            string FormatAmount(double value)
                => value.ToString("F2", culture.NumberFormat);
        }
        #endregion
    }
}
