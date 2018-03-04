#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Entities;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Services
{
    public class WeatherService : IGodfatherService
    {
        private string _key;


        public WeatherService(string key)
        {
            _key = key;
        }


        public async Task<DiscordEmbed> GetEmbeddedWeatherDataAsync(string query)
        {
            var emb = new DiscordEmbedBuilder() {
                Color = DiscordColor.Aquamarine
            };
            WeatherData data = null;
            try {
                var handler = new HttpClientHandler {
                    AllowAutoRedirect = false
                };
                using (var hc = new HttpClient(handler)) {
                    var response = await hc.GetStringAsync($"http://api.openweathermap.org/data/2.5/weather?q={query}&appid={_key}&units=metric")
                        .ConfigureAwait(false);
                    data = JsonConvert.DeserializeObject<WeatherData>(response);
                }
            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
                return null;
            }

            emb.AddField($"{DiscordEmoji.FromUnicode("\U0001f30d")} Location", $"[{data.Name + ", " + data.Sys.Country}](https://openweathermap.org/city/{data.Id})", inline: true)
               .AddField($"{DiscordEmoji.FromUnicode("\U0001f4cf")} Coordinates", $"{data.Coord.Lat}, {data.Coord.Lon}", inline: true)
               .AddField($"{DiscordEmoji.FromUnicode("\u2601")} Condition", string.Join(", ", data.Weather.Select(w => w.Main)), inline: true)
               .AddField($"{DiscordEmoji.FromUnicode("\U0001f4a6")} Humidity", $"{data.Main.Humidity}%", inline: true)
               .AddField($"{DiscordEmoji.FromUnicode("\U0001f321")} Temperature", $"{data.Main.Temp:F1}°C", inline: true)
               .AddField($"{DiscordEmoji.FromUnicode("\U0001f321")} Min/Max Temp", $"{data.Main.TempMin:F1}°C - {data.Main.TempMax:F1}°C", inline: true)
               .AddField($"{DiscordEmoji.FromUnicode("\U0001f4a8")} Wind speed", data.Wind.Speed + " m/s", inline: true)
               .WithThumbnailUrl($"http://openweathermap.org/img/w/{data.Weather[0].Icon}.png")
               .WithFooter("Powered by openweathermap.org");

            return emb.Build();
        }
    }
}
