using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Search
{
    [Group("weather"), Module(ModuleType.Searches), NotBlocked]
    [Aliases("w")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class WeatherModule : TheGodfatherServiceModule<WeatherService>
    {
        #region weather
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("desc-query")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException(ctx, "cmd-err-query");

            CompleteWeatherData? data = await this.Service.GetCurrentDataAsync(query);
            if (data is null) {
                await ctx.FailAsync("cmd-err-weather");
                return;
            }

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                string locationStr = $"[{data.Name + ", " + data.Sys.Country}]({WeatherService.GetCityUrl(data.Id)})";
                emb.AddLocalizedTitleField("str-w-location", locationStr, inline: true, titleArgs: Emojis.Globe);
                emb.AddLocalizedTitleField("str-w-coordinates", $"{data.Coord.Lat}, {data.Coord.Lon}", inline: true, titleArgs: Emojis.Ruler);
                this.AddDataToEmbed(emb, data);
            });
        }
        #endregion

        #region weather forecast
        [Command("forecast"), Priority(1)]
        [Aliases("f")]
        public async Task ForecastAsync(CommandContext ctx,
                                       [Description("desc-amount-days")] int amount,
                                       [RemainingText, Description("desc-query")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException(ctx, "cmd-err-query");

            if (amount is < 1 or > 31)
                throw new InvalidCommandUsageException(ctx, "cmd-err-weather", 1, 31);

            Forecast? data = await this.Service.GetForecastAsync(query);
            if (data is null || !data.WeatherDataList.Any()) {
                await ctx.FailAsync("cmd-err-weather");
                return;
            }

            await ctx.PaginateAsync(data.WeatherDataList.Select((d, i) => (d, i)).Take(amount), (emb, r) => {
                DateTime date = DateTime.Now.AddDays(r.i + 1);
                emb.WithLocalizedTitle("fmt-weather-f", date.DayOfWeek, date.Date.ToShortDateString());
                string locationStr = $"[{data.City.Name + ", " + data.City.Country}]({WeatherService.GetCityUrl(data.City)})";
                emb.AddLocalizedTitleField("str-w-location", locationStr, inline: true, titleArgs: Emojis.Globe);
                emb.AddLocalizedTitleField("str-w-coordinates", $"{data.City.Coord.Lat}, {data.City.Coord.Lon}", inline: true, titleArgs: Emojis.Ruler);
                this.AddDataToEmbed(emb, r.d);
                return emb;
            }, this.ModuleColor);
        }

        [Command("forecast"), Priority(0)]
        public Task ForecastAsync(CommandContext ctx,
                                 [RemainingText, Description("desc-query")] string query)
            => this.ForecastAsync(ctx, 7, query);
        #endregion


        #region internals
        private LocalizedEmbedBuilder AddDataToEmbed(LocalizedEmbedBuilder emb, PartialWeatherData data)
        {
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedTitleField("str-w-condition", data.Weather.Select(w => w.Main).JoinWith(", "), inline: true, titleArgs: Emojis.Cloud);
            emb.AddLocalizedTitleField("str-w-humidity", $"{data.Main.Humidity}%", inline: true, titleArgs: Emojis.Drops);
            emb.AddLocalizedTitleField("str-w-temp", $"{data.Main.Temp:F1}°C", inline: true, titleArgs: Emojis.Thermometer);
            emb.AddLocalizedTitleField("str-w-temp-minmax", $"{data.Main.TempMin:F1}°C / {data.Main.TempMax:F1}°C", inline: true, titleArgs: Emojis.Thermometer);
            emb.AddLocalizedTitleField("str-w-wind", $"{data.Wind.Speed} m/s", inline: true, titleArgs: Emojis.Wind);
            emb.WithThumbnail(WeatherService.GetWeatherIconUrl(data.Weather[0]));
            emb.WithLocalizedFooter("fmt-powered-by", null, "openweathermap.org");
            return emb;
        }
        #endregion
    }
}
