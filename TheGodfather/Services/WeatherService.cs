#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Common;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
#endregion

namespace TheGodfather.Services
{
    public class WeatherService : HttpService
    {
        private string _key;


        public WeatherService(string key)
        {
            _key = key;
        }


        public async Task<DiscordEmbed> GetEmbeddedCurrentWeatherDataAsync(string query)
        {
            WeatherData data = null;
            try {
                var response = await _http.GetStringAsync($"http://api.openweathermap.org/data/2.5/weather?q={ query }&appid={ _key }&units=metric")
                    .ConfigureAwait(false);
                data = JsonConvert.DeserializeObject<WeatherData>(response);
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return null;
            }

            return data.Embed();
        }

        public async Task<IReadOnlyList<DiscordEmbed>> GetEmbeddedWeatherForecastAsync(string query, int amount = 7)
        {
            try {
                var response = await _http.GetStringAsync($"http://api.openweathermap.org/data/2.5/forecast?q={ query }&appid={ _key }&units=metric")
                    .ConfigureAwait(false);
                var forecast = JsonConvert.DeserializeObject<Forecast>(response);
                return forecast.GetEmbeds(amount);
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return null;
            }
        }
    }
}
