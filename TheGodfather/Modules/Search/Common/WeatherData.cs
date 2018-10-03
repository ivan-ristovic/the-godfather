#region USING_DIRECTIVES
using DSharpPlus.Entities;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;

using TheGodfather.Common;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfather.Modules.Search.Common
{
    public class Coord
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class City
    {
        public Coord Coord { get; set; }
        public string Country { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Weather
    {
        public string Description { get; set; }
        public string Icon { get; set; }
        public int Id { get; set; }
        public string Main { get; set; }
    }

    public class Main
    {
        public double Temp { get; set; }
        public float Pressure { get; set; }
        public float Humidity { get; set; }

        [JsonProperty("temp_min")]
        public double TempMin { get; set; }

        [JsonProperty("temp_max")]
        public double TempMax { get; set; }
    }

    public class Wind
    {
        public double Speed { get; set; }
        public double Deg { get; set; }
    }

    public class Clouds
    {
        public int All { get; set; }
    }

    public class Sys
    {
        public int Type { get; set; }
        public int Id { get; set; }
        public double Message { get; set; }
        public string Country { get; set; }
        public double Sunrise { get; set; }
        public double Sunset { get; set; }
    }

    public class WeatherData
    {
        public Coord Coord { get; set; }
        public List<Weather> Weather { get; set; }
        public Main Main { get; set; }
        public int Visibility { get; set; }
        public Wind Wind { get; set; }
        public Clouds Clouds { get; set; }
        public int Dt { get; set; }
        public Sys Sys { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cod { get; set; }


        public DiscordEmbed ToDiscordEmbed(DiscordColor? color = null)
        {
            var emb = new DiscordEmbedBuilder();

            if (!(color is null))
                emb.WithColor(color.Value);

            emb.AddField($"{StaticDiscordEmoji.Globe} Location", $"[{this.Name + ", " + this.Sys.Country}](https://openweathermap.org/city/{ this.Id })", inline: true);
            emb.AddField($"{StaticDiscordEmoji.Ruler} Coordinates", $"{this.Coord.Lat}, {this.Coord.Lon}", inline: true);
            emb.AddField($"{StaticDiscordEmoji.Cloud} Condition", string.Join(", ", this.Weather.Select(w => w.Main)), inline: true);
            emb.AddField($"{StaticDiscordEmoji.Drops} Humidity", $"{this.Main.Humidity}%", inline: true);
            emb.AddField($"{StaticDiscordEmoji.Thermometer} Temperature", $"{this.Main.Temp:F1}°C", inline: true);
            emb.AddField($"{StaticDiscordEmoji.Thermometer} Min/Max Temp", $"{this.Main.TempMin:F1}°C / {this.Main.TempMax:F1}°C", inline: true);
            emb.AddField($"{StaticDiscordEmoji.Wind} Wind speed", this.Wind.Speed + " m/s", inline: true);

            emb.WithThumbnailUrl(WeatherService.GetWeatherIconUrl(this.Weather.FirstOrDefault()));

            emb.WithFooter("Powered by openweathermap.org");

            return emb.Build();
        }
    }

    public class PartialWeatherData
    {
        public List<Weather> Weather { get; set; }
        public Main Main { get; set; }
        public int Visibility { get; set; }
        public Wind Wind { get; set; }
        public Clouds Clouds { get; set; }
        public int Dt { get; set; }
        public Sys Sys { get; set; }
        public int Id { get; set; }
        public int Cod { get; set; }


        public DiscordEmbedBuilder InsertIntoDiscordEmbed(DiscordEmbedBuilder emb)
        {
            emb.AddField($"{StaticDiscordEmoji.Cloud} Condition", string.Join(", ", this.Weather.Select(w => w.Main)), inline: true);
            emb.AddField($"{StaticDiscordEmoji.Drops} Humidity", $"{this.Main.Humidity}%", inline: true);
            emb.AddField($"{StaticDiscordEmoji.Thermometer} Temperature", $"{this.Main.Temp:F1}°C", inline: true);
            emb.AddField($"{StaticDiscordEmoji.Thermometer} Min/Max Temp", $"{this.Main.TempMin:F1}°C / {this.Main.TempMax:F1}°C", inline: true);
            emb.AddField($"{StaticDiscordEmoji.Wind} Wind speed", this.Wind.Speed + " m/s", inline: true);
            emb.WithThumbnailUrl(WeatherService.GetWeatherIconUrl(this.Weather.FirstOrDefault()));
            emb.WithFooter("Powered by openweathermap.org");
            return emb;
        }
    }

    public class Forecast
    {
        public City City { get; set; }

        [JsonProperty("list")]
        public List<PartialWeatherData> WeatherDataList { get; set; }


        public IReadOnlyList<DiscordEmbed> ToDiscordEmbeds(int amount = 7)
        {
            var embeds = new List<DiscordEmbed>();

            for (int i = 0; i < this.WeatherDataList.Count && i < amount; i++) {
                var data = this.WeatherDataList[i];
                var date = DateTime.UtcNow.AddDays(i + 1);

                var emb = new DiscordEmbedBuilder() {
                    Title = $"Forecast for {date.DayOfWeek}, {date.Date.ToUniversalTime().ToShortDateString()}",
                    Color = DiscordColor.Aquamarine
                };

                emb.AddField($"{StaticDiscordEmoji.Globe} Location", $"[{this.City.Name + ", " + this.City.Country}]({WeatherService.GetCityUrl(this.City)})", inline: true);
                emb.AddField($"{StaticDiscordEmoji.Ruler} Coordinates", $"{this.City.Coord.Lat}, {this.City.Coord.Lon}", inline: true);

                emb = data.InsertIntoDiscordEmbed(emb);
                embeds.Add(emb.Build());
            }

            return embeds.AsReadOnly();
        }
    }
}
