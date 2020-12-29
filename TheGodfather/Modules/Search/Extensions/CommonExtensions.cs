#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Humanizer;
using TheGodfather.Common;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Search.Extensions
{
    public static class CommonExtensions
    {
        private static readonly string _unknown = Formatter.Italic("Unknown");


        public static DiscordEmbedBuilder ToDiscordEmbed(this WeatherData data, DiscordColor? color = null)
        {
            var emb = new DiscordEmbedBuilder();

            if (!(color is null))
                emb.WithColor(color.Value);

            emb.AddField($"{Emojis.Globe} Location", $"[{data.Name + ", " + data.Sys.Country}](https://openweathermap.org/city/{ data.Id })", inline: true);
            emb.AddField($"{Emojis.Ruler} Coordinates", $"{data.Coord.Lat}, {data.Coord.Lon}", inline: true);
            emb.AddField($"{Emojis.Cloud} Condition", string.Join(", ", data.Weather.Select(w => w.Main)), inline: true);
            emb.AddField($"{Emojis.Drops} Humidity", $"{data.Main.Humidity}%", inline: true);
            emb.AddField($"{Emojis.Thermometer} Temperature", $"{data.Main.Temp:F1}°C", inline: true);
            emb.AddField($"{Emojis.Thermometer} Min/Max Temp", $"{data.Main.TempMin:F1}°C / {data.Main.TempMax:F1}°C", inline: true);
            emb.AddField($"{Emojis.Wind} Wind speed", data.Wind.Speed + " m/s", inline: true);

            emb.WithThumbnail(WeatherService.GetWeatherIconUrl(data.Weather.FirstOrDefault()));

            emb.WithFooter("Powered by openweathermap.org");

            return emb;
        }

        public static DiscordEmbedBuilder AddToDiscordEmbed(this PartialWeatherData data, DiscordEmbedBuilder emb)
        {
            emb.AddField($"{Emojis.Cloud} Condition", string.Join(", ", data.Weather.Select(w => w.Main)), inline: true);
            emb.AddField($"{Emojis.Drops} Humidity", $"{data.Main.Humidity}%", inline: true);
            emb.AddField($"{Emojis.Thermometer} Temperature", $"{data.Main.Temp:F1}°C", inline: true);
            emb.AddField($"{Emojis.Thermometer} Min/Max Temp", $"{data.Main.TempMin:F1}°C / {data.Main.TempMax:F1}°C", inline: true);
            emb.AddField($"{Emojis.Wind} Wind speed", data.Wind.Speed + " m/s", inline: true);
            emb.WithThumbnail(WeatherService.GetWeatherIconUrl(data.Weather.FirstOrDefault()));
            emb.WithFooter("Powered by openweathermap.org");
            return emb;
        }

        public static IReadOnlyList<DiscordEmbedBuilder> ToDiscordEmbedBuilders(this Forecast forecast, int amount = 7)
        {
            var embeds = new List<DiscordEmbedBuilder>();

            for (int i = 0; i < forecast.WeatherDataList.Count && i < amount; i++) {
                PartialWeatherData data = forecast.WeatherDataList[i];
                DateTime date = DateTime.UtcNow.AddDays(i + 1);

                var emb = new DiscordEmbedBuilder {
                    Title = $"Forecast for {date.DayOfWeek}, {date.Date.ToUniversalTime().ToShortDateString()}",
                    Color = DiscordColor.Aquamarine
                };

                emb.AddField($"{Emojis.Globe} Location", $"[{forecast.City.Name + ", " + forecast.City.Country}]({WeatherService.GetCityUrl(forecast.City)})", inline: true);
                emb.AddField($"{Emojis.Ruler} Coordinates", $"{forecast.City.Coord.Lat}, {forecast.City.Coord.Lon}", inline: true);

                emb = data.AddToDiscordEmbed(emb);
                embeds.Add(emb);
            }

            return embeds.AsReadOnly();
        }

        public static DiscordEmbedBuilder ToDiscordEmbedBuilder(this XkcdComic comic, DiscordColor? color = null)
        {
            var emb = new DiscordEmbedBuilder {
                Title = $"xkcd #{comic.Id} : {comic.Title}",
                ImageUrl = comic.ImageUrl,
                Url = XkcdService.CreateUrlForComic(comic.Id)
            };

            if (!(color is null))
                emb.WithColor(color.Value);

            emb.WithFooter($"Publish date: {comic.Month}/{comic.Year}");

            return emb;
        }
    }
}
