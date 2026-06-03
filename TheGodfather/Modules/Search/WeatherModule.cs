using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search;

[Group("weather")]
[Alias("w")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class WeatherModule : TheGodfatherModule
{
    private readonly WeatherService weather;


    public WeatherModule(LocalizationService localization, WeatherService weather)
        : base(localization, new Color(0x1abc9c))
    {
        this.weather = weather;
    }


    [Command("")]
    public async Task ExecuteGroupAsync([Remainder] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new LocalizedCommandException(TranslationKey.cmd_err_query);

        string lang = this.Localization.GetGuildCulture(this.Context.Guild?.Id).TwoLetterISOLanguageName;
        WeatherResponse? data = await this.weather.GetAsync(query, lang);
        if (data is null) {
            await this.FailAsync(TranslationKey.cmd_err_weather);
            return;
        }

        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.AddLocalizedField(TranslationKey.str_w_location(Emojis.Globe), $"{data.Location.Name}, {data.Location.Country}", true);
            emb.AddLocalizedField(TranslationKey.str_w_coordinates(Emojis.Ruler), $"{data.Lat}, {data.Lon}", true);
            this.AddCurrentDataToEmbed(emb, data.Current);
        });
    }

    [Command("forecast")]
    [Alias("f", "daily")]
    public async Task ForecastAsync([Remainder] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new LocalizedCommandException(TranslationKey.cmd_err_query);

        string lang = this.Localization.GetGuildCulture(this.Context.Guild?.Id).TwoLetterISOLanguageName;
        WeatherResponse? data = await this.weather.GetAsync(query, lang);
        if (data is null) {
            await this.FailAsync(TranslationKey.cmd_err_weather);
            return;
        }

        await this.RespondPagedAsync(data.Daily, (emb, daily) => {
            var date = DateTimeOffset.FromUnixTimeSeconds(daily.Dt);
            emb.WithLocalizedTitle(TranslationKey.fmt_weather_f(date.DayOfWeek, this.Localization.GetLocalizedTimeString(this.Context.Guild?.Id, date, "d")));
            emb.AddLocalizedField(TranslationKey.str_w_location(Emojis.Globe), $"{data.Location.Name}, {data.Location.Country}", true);
            emb.AddLocalizedField(TranslationKey.str_w_coordinates(Emojis.Ruler), $"{data.Lat}, {data.Lon}", true);
            this.AddDailyDataToEmbed(emb, daily);
            return emb;
        });
    }

    [Command("graph")]
    [Alias("g")]
    public async Task GraphAsync([Remainder] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new LocalizedCommandException(TranslationKey.cmd_err_query);

        string? data = this.weather.GraphFor(query);
        if (data is null) {
            await this.FailAsync(TranslationKey.cmd_err_weather);
            return;
        }

        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithImageUrl(data);
            emb.WithLocalizedFooter(TranslationKey.fmt_powered_by(WeatherService.GraphServiceUrl), null);
        });
    }

    private LocalizedEmbedBuilder AddCurrentDataToEmbed(LocalizedEmbedBuilder emb, Current data)
    {
        emb.WithColor(this.ModuleColor);
        emb.WithDescription(data.Weather.First().Description.Humanize());
        emb.AddLocalizedField(TranslationKey.str_w_condition(Emojis.Cloud), data.Weather.Select(w => w.Main).JoinWith(", "), true);
        emb.AddLocalizedField(TranslationKey.str_w_humidity(Emojis.Drops), $"{data.Humidity}%", true);
        emb.AddLocalizedField(TranslationKey.str_w_temp(Emojis.Thermometer), $"{data.Temp:F1}°C (~{data.FeelsLike:F1}°C)", true);
        emb.AddLocalizedField(TranslationKey.str_w_wind(Emojis.Wind), $"{data.WindSpeed} m/s", true);
        emb.WithThumbnail(WeatherService.GetWeatherIconUrl(data.Weather[0]));
        emb.WithLocalizedFooter(TranslationKey.fmt_powered_by(WeatherService.ServiceUrl), null);
        return emb;
    }

    private LocalizedEmbedBuilder AddDailyDataToEmbed(LocalizedEmbedBuilder emb, Daily data)
    {
        emb.WithColor(this.ModuleColor);
        emb.WithDescription(data.Summary.Humanize());
        emb.AddLocalizedField(TranslationKey.str_w_condition(Emojis.Cloud), data.Weather.Select(w => w.Main).JoinWith(", "), true);
        emb.AddLocalizedField(TranslationKey.str_w_humidity(Emojis.Drops), $"{data.Humidity}%", true);
        emb.AddLocalizedField(TranslationKey.str_w_temp(Emojis.Thermometer), $"{data.Temp.Morn:F1}/{data.Temp.Day:F1}/{data.Temp.Eve:F1}/{data.Temp.Night:F1} °C", true);
        emb.AddLocalizedField(TranslationKey.str_w_temp_minmax(Emojis.Thermometer), $"{data.Temp.Min:F1}°C / {data.Temp.Max:F1}°C", true);
        emb.AddLocalizedField(TranslationKey.str_w_wind(Emojis.Wind), $"{data.WindSpeed} m/s", true);
        emb.WithThumbnail(WeatherService.GetWeatherIconUrl(data.Weather[0]));
        emb.WithLocalizedFooter(TranslationKey.fmt_powered_by(WeatherService.ServiceUrl), null);
        return emb;
    }
}
