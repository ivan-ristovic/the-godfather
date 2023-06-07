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

        string lang = this.Localization.GetGuildCulture(ctx.Guild?.Id).TwoLetterISOLanguageName;
        WeatherResponse? data = await this.Service.GetAsync(query, lang);
        if (data is null) {
            await ctx.FailAsync(TranslationKey.cmd_err_weather);
            return;
        }

        await ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.AddLocalizedField(TranslationKey.str_w_location(Emojis.Globe), $"{data.Location.Name + ", " + data.Location.Country}", true);
            emb.AddLocalizedField(TranslationKey.str_w_coordinates(Emojis.Ruler), $"{data.Lat}, {data.Lon}", true);
            this.AddCurrentDataToEmbed(emb, data.Current);
        });
    }
    #endregion

    #region weather forecast
    [Command("forecast")]
    [Aliases("f", "daily")]
    public async Task ForecastAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_query)] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_query);

        string lang = this.Localization.GetGuildCulture(ctx.Guild?.Id).TwoLetterISOLanguageName;
        WeatherResponse? data = await this.Service.GetAsync(query, lang);
        if (data is null) {
            await ctx.FailAsync(TranslationKey.cmd_err_weather);
            return;
        }

        await ctx.PaginateAsync(data.Daily, (emb, d) => {
            var date = DateTimeOffset.FromUnixTimeSeconds(d.Dt);
            emb.WithLocalizedTitle(TranslationKey.fmt_weather_f(date.DayOfWeek, this.Localization.GetLocalizedTimeString(ctx.Guild?.Id, date, "d")));
            emb.AddLocalizedField(TranslationKey.str_w_location(Emojis.Globe), $"{data.Location.Name + ", " + data.Location.Country}", true);
            emb.AddLocalizedField(TranslationKey.str_w_coordinates(Emojis.Ruler), $"{data.Lat}, {data.Lon}", true);
            this.AddDailyDataToEmbed(emb, d);
            return emb;
        }, this.ModuleColor);
    }
    #endregion

    #region weather graph
    [Command("graph")]
    [Aliases("g")]
    public async Task GraphAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_query)] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_query);

        string? data = this.Service.GraphFor(query);
        if (data is null) {
            await ctx.FailAsync(TranslationKey.cmd_err_weather);
            return;
        }

        await ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithImageUrl(data);
            emb.WithLocalizedFooter(TranslationKey.fmt_powered_by(WeatherService.GraphServiceUrl), null);
        });
    }
    #endregion

    #region internals
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
    #endregion
}