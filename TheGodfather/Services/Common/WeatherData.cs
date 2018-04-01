#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

using TheGodfather.Common;

using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Services.Common
{
    public class Coord
    {
        public double Lon { get; set; }
        public double Lat { get; set; }
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Coord Coord { get; set; }
        public string Country { get; set; }
    }

    public class Weather
    {
        public int Id { get; set; }
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
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


        public DiscordEmbed Embed()
        {
            var emb = new DiscordEmbedBuilder() {
                Color = DiscordColor.Aquamarine
            };

            emb.AddField($"{StaticDiscordEmoji.Globe} Location", $"[{Name + ", " + Sys.Country}](https://openweathermap.org/city/{ Id })", inline: true)
               .AddField($"{StaticDiscordEmoji.Ruler} Coordinates", $"{Coord.Lat}, {Coord.Lon}", inline: true)
               .AddField($"{StaticDiscordEmoji.Cloud} Condition", string.Join(", ", Weather.Select(w => w.Main)), inline: true)
               .AddField($"{StaticDiscordEmoji.Drops} Humidity", $"{Main.Humidity}%", inline: true)
               .AddField($"{StaticDiscordEmoji.Thermometer} Temperature", $"{Main.Temp:F1}°C", inline: true)
               .AddField($"{StaticDiscordEmoji.Thermometer} Min/Max Temp", $"{Main.TempMin:F1}°C / {Main.TempMax:F1}°C", inline: true)
               .AddField($"{StaticDiscordEmoji.Wind} Wind speed", Wind.Speed + " m/s", inline: true)
               .WithThumbnailUrl($"http://openweathermap.org/img/w/{ Weather[0].Icon }.png")
               .WithFooter("Powered by openweathermap.org");

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


        public DiscordEmbedBuilder Embed(DiscordEmbedBuilder emb)
        {
            emb.AddField($"{StaticDiscordEmoji.Cloud} Condition", string.Join(", ", Weather.Select(w => w.Main)), inline: true)
               .AddField($"{StaticDiscordEmoji.Drops} Humidity", $"{Main.Humidity}%", inline: true)
               .AddField($"{StaticDiscordEmoji.Thermometer} Temperature", $"{Main.Temp:F1}°C", inline: true)
               .AddField($"{StaticDiscordEmoji.Thermometer} Min/Max Temp", $"{Main.TempMin:F1}°C / {Main.TempMax:F1}°C", inline: true)
               .AddField($"{StaticDiscordEmoji.Wind} Wind speed", Wind.Speed + " m/s", inline: true)
               .WithThumbnailUrl($"http://openweathermap.org/img/w/{ Weather[0].Icon }.png")
               .WithFooter("Powered by openweathermap.org");
            return emb;
        }
    }

    public class Forecast
    {
        public City City { get; set; }

        [JsonProperty("list")]
        public List<PartialWeatherData> WeatherDataList { get; set; }


        public IReadOnlyList<DiscordEmbed> GetEmbeds(int amount = 7)
        {
            List<DiscordEmbed> embeds = new List<DiscordEmbed>();

            for (int i = 0; i < WeatherDataList.Count && i < amount; i++) {
                var data = WeatherDataList[i];
                var date = DateTime.UtcNow.AddDays(i + 1);

                var emb = new DiscordEmbedBuilder() {
                    Title = $"Forecast for {date.DayOfWeek}, {date.Date.ToUniversalTime().ToShortDateString()}",
                    Color = DiscordColor.Aquamarine
                };

                emb.AddField($"{StaticDiscordEmoji.Globe} Location", $"[{City.Name + ", " + City.Country}](https://openweathermap.org/city/{ City.Id })", inline: true)
                   .AddField($"{StaticDiscordEmoji.Ruler} Coordinates", $"{City.Coord.Lat}, {City.Coord.Lon}", inline: true);

                emb = data.Embed(emb);
                embeds.Add(emb.Build());
            }

            return embeds.AsReadOnly();
        }
    }
}
