#region USING_DIRECTIVES
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Services
{
    public class WeatherService : TheGodfatherHttpService
    {
        private readonly string url = "http://api.openweathermap.org/data/2.5";
        private readonly string key;


        public WeatherService(string key)
        {
            this.key = key;
        }


        public static string GetCityUrl(City city)
        {
            if (city == null)
                throw new ArgumentException("City missing", "city");

            return $"https://openweathermap.org/city/{ city.Id }";
        }

        public static string GetWeatherIconUrl(Weather weather)
        {
            if (weather == null)
                throw new ArgumentException("Weather missing", "weather");

            return $"http://openweathermap.org/img/w/{ weather.Icon }.png";
        }


        public async Task<DiscordEmbed> GetEmbeddedCurrentWeatherDataAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query missing", "query");

            try {
                string response = await _http.GetStringAsync($"{this.url}/weather?q={query}&appid={this.key}&units=metric").ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<WeatherData>(response);
                return data.ToDiscordEmbed();
            } catch {
                return null;
            }
        }

        public async Task<IReadOnlyList<DiscordEmbed>> GetEmbeddedWeatherForecastAsync(string query, int amount = 7)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query missing", "query");
            
            if (amount < 1 || amount > 20)
                throw new ArgumentException("Days amount out of range (max 20)", "amount");

            try {
                string response = await _http.GetStringAsync($"{this.url}/forecast?q={query}&appid={this.key}&units=metric").ConfigureAwait(false);
                var forecast = JsonConvert.DeserializeObject<Forecast>(response);
                return forecast.ToDiscordEmbeds(amount);
            } catch {
                return null;
            }
        }
    }
}
