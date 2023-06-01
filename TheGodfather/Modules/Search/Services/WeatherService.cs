using System.Net;
using Newtonsoft.Json;
using TheGodfather.Modules.Search.Common;

namespace TheGodfather.Modules.Search.Services;

public class WeatherService : TheGodfatherHttpService
{
    public const string ServiceUrl = "openweathermap.org";
    public const string GraphServiceUrl = "wttr.in";
    
    private const string WeatherGraphUrl = $"https://v2.{GraphServiceUrl}";
    private const string WeatherServiceUrl = $"https://{ServiceUrl}";
    private const string WeatherApiUrl = $"http://api.{ServiceUrl}/data/2.5";

    public override bool IsDisabled => string.IsNullOrWhiteSpace(this.key);

    private readonly string? key;


    public WeatherService(BotConfigService cfg)
    {
        this.key = cfg.CurrentConfiguration.WeatherKey;
    }


    public static string GetCityUrl(City city)
        => GetCityUrl(city.Id);

    public static string GetCityUrl(int id)
        => $"{WeatherServiceUrl}/city/{id}";

    public static string GetWeatherIconUrl(Weather weather)
        => $"{WeatherServiceUrl}/img/w/{weather.Icon}.png";

    public async Task<CompleteWeatherData?> GetCurrentDataAsync(string query)
    {
        if (this.IsDisabled || string.IsNullOrWhiteSpace(query))
            return null;

        try {
            string url = $"{WeatherApiUrl}/weather?q={WebUtility.UrlEncode(query)}&appid={this.key}&units=metric";
            string response = await _http.GetStringAsync(url).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<CompleteWeatherData>(response);
        } catch {
            return null;
        }
    }

    public async Task<Forecast?> GetForecastAsync(string query)
    {
        if (this.IsDisabled || string.IsNullOrWhiteSpace(query))
            return null;

        try {
            string url = $"{WeatherApiUrl}/forecast?q={WebUtility.UrlEncode(query)}&appid={this.key}&units=metric";
            string response = await _http.GetStringAsync(url).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<Forecast>(response);
        } catch {
            return null;
        }
    }
    
    public string? GraphFor(string query)
    {
        if (this.IsDisabled || string.IsNullOrWhiteSpace(query))
            return null;

        return $"{WeatherGraphUrl}/{WebUtility.UrlEncode(query)}.png";
    }
}