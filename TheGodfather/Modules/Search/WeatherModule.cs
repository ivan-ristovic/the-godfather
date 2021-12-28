using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Search;

[Group("weather")][Module(ModuleType.Searches)][NotBlocked]
[Aliases("w")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class WeatherModule : TheGodfatherServiceModule<WeatherService>
{
    #region weather
    [GroupCommand]
    public async Task ExecuteGroupAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_query)] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_query);

        CompleteWeatherData? data = await this.Service.GetCurrentDataAsync(query);
        if (data is null) {
            await ctx.FailAsync(TranslationKey.cmd_err_weather);
            return;
        }

        await ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            string locationStr = $"[{data.Name + ", " + data.Sys.Country}]({WeatherService.GetCityUrl(data.Id)})";
            emb.AddLocalizedField(TranslationKey.str_w_location(Emojis.Globe), locationStr, true);
            emb.AddLocalizedField(TranslationKey.str_w_coordinates(Emojis.Ruler), $"{data.Coord.Lat}, {data.Coord.Lon}", true);
            this.AddDataToEmbed(emb, data);
        });
    }
    #endregion

    #region weather forecast
    [Command("forecast")][Priority(1)]
    [Aliases("f")]
    public async Task ForecastAsync(CommandContext ctx,
        [Description(TranslationKey.desc_amount_days)] int amount,
        [RemainingText][Description(TranslationKey.desc_query)] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_query);

        if (amount is < 1 or > 31)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_weather_f(1, 31));

        Forecast? data = await this.Service.GetForecastAsync(query);
        if (data is null || !data.WeatherDataList.Any()) {
            await ctx.FailAsync(TranslationKey.cmd_err_weather);
            return;
        }

        await ctx.PaginateAsync(data.WeatherDataList.Select((d, i) => (d, i)).Take(amount), (emb, r) => {
            DateTime date = DateTime.Now.AddDays(r.i + 1);
            emb.WithLocalizedTitle(TranslationKey.fmt_weather_f(date.DayOfWeek, date.Date.ToShortDateString()));
            string locationStr = $"[{data.City.Name + ", " + data.City.Country}]({WeatherService.GetCityUrl(data.City)})";
            emb.AddLocalizedField(TranslationKey.str_w_location(Emojis.Globe), locationStr, true);
            emb.AddLocalizedField(TranslationKey.str_w_coordinates(Emojis.Ruler), $"{data.City.Coord.Lat}, {data.City.Coord.Lon}", true);
            this.AddDataToEmbed(emb, r.d);
            return emb;
        }, this.ModuleColor);
    }

    [Command("forecast")][Priority(0)]
    public Task ForecastAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_query)] string query)
        => this.ForecastAsync(ctx, 7, query);
    #endregion


    #region internals
    private LocalizedEmbedBuilder AddDataToEmbed(LocalizedEmbedBuilder emb, PartialWeatherData data)
    {
        emb.WithColor(this.ModuleColor);
        emb.AddLocalizedField(TranslationKey.str_w_condition(Emojis.Cloud), data.Weather.Select(w => w.Main).JoinWith(", "), true);
        emb.AddLocalizedField(TranslationKey.str_w_humidity(Emojis.Drops), $"{data.Main.Humidity}%", true);
        emb.AddLocalizedField(TranslationKey.str_w_temp(Emojis.Thermometer), $"{data.Main.Temp:F1}°C", true);
        emb.AddLocalizedField(TranslationKey.str_w_temp_minmax(Emojis.Thermometer), $"{data.Main.TempMin:F1}°C / {data.Main.TempMax:F1}°C", true);
        emb.AddLocalizedField(TranslationKey.str_w_wind(Emojis.Wind), $"{data.Wind.Speed} m/s", true);
        emb.WithThumbnail(WeatherService.GetWeatherIconUrl(data.Weather[0]));
        emb.WithLocalizedFooter(TranslationKey.fmt_powered_by("openweathermap.org"), null);
        return emb;
    }
    #endregion
}