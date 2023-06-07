using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TheGodfather.Modules.Search.Common;

namespace TheGodfather.Modules.Search.Services;

public class WeatherService : TheGodfatherHttpService
{
    public static readonly TimeSpan DataUpdateInterval = TimeSpan.FromHours(1);
    public static readonly TimeSpan GeoUpdateInterval = TimeSpan.FromHours(12);

    public const string ServiceUrl = "openweathermap.org";
    public const string GraphServiceUrl = "wttr.in";
    
    private const string GraphApiUrl = $"https://v2.{GraphServiceUrl}";
    private const string WeatherServiceRoot = $"https://{ServiceUrl}";
    private const string GeocodingEndpoint = $"http://api.{ServiceUrl}/geo/1.0/direct";
    private const string WeatherApiUrl = $"https://api.{ServiceUrl}/data/3.0/onecall";

    public override bool IsDisabled => string.IsNullOrWhiteSpace(this.key);

    private readonly string? key;
    private readonly IMemoryCache dataCache;
    private readonly IMemoryCache geoCache;
    private readonly SemaphoreSlim dataSem;
    private readonly SemaphoreSlim geoSem;
    private readonly JsonSerializerSettings serializerSettings;

    public WeatherService(BotConfigService cfg)
    {
        this.key = cfg.CurrentConfiguration.WeatherKey;
        this.dataCache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = DataUpdateInterval });
        this.geoCache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = GeoUpdateInterval });
        this.dataSem = new SemaphoreSlim(1, 1);
        this.geoSem = new SemaphoreSlim(1, 1);
        this.serializerSettings = new JsonSerializerSettings {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() }
        };
    }

    public static string GetWeatherIconUrl(Weather weather)
        => $"{WeatherServiceRoot}/img/w/{weather.Icon}.png";

    public async Task<WeatherResponse?> GetAsync(string query, string lang)
    {
        if (this.IsDisabled || string.IsNullOrWhiteSpace(query))
            return null;

        query = query.ToLowerInvariant();
        GeocodingData? cachedGeo = null;
        
        await this.geoSem.WaitAsync();
        try {
            if (!this.geoCache.TryGetValue(query, out cachedGeo)) {
                string url = $"{GeocodingEndpoint}?q={WebUtility.UrlEncode(query)}&appid={this.key}&limit=1";
                string response = await _http.GetStringAsync(url).ConfigureAwait(false);
                List<GeocodingData>? data = JsonConvert.DeserializeObject<List<GeocodingData>>(response);
                if (data is null || data.Count == 0)
                    return null;
                cachedGeo = data[0];
                this.geoCache.Set(query, cachedGeo);
            }
        } catch (Exception e) when (e is HttpRequestException or JsonSerializationException) {
            Log.Error(e, "Failed to fetch/deserialize Geocoding API response");
        } finally {
            this.geoSem.Release();
        }

        if (cachedGeo is null) {
            return null;
        }

        WeatherResponse? weatherResponse = await this.GetAsync(cachedGeo.Lat, cachedGeo.Lon, lang);
        if (weatherResponse is not null) {
            weatherResponse.Location = cachedGeo;
            weatherResponse.Locale = lang;
        }
        return weatherResponse;
    }
    
    public async Task<WeatherResponse?> GetAsync(double lat, double lon, string lang)
    {
        if (this.IsDisabled)
            return null;

        WeatherResponse? cachedData = null;

        await this.dataSem.WaitAsync();
        try {
            if (!this.dataCache.TryGetValue((lat, lon), out cachedData) || cachedData?.Locale != lang) {
                string url = $"{WeatherApiUrl}?lat={lat}&lon={lon}&units=metric&lang={lang}&exclude=minutely,hourly&appid={this.key}";
                string response = await _http.GetStringAsync(url).ConfigureAwait(false);
                cachedData = JsonConvert.DeserializeObject<WeatherResponse>(response, this.serializerSettings);
                if (cachedData is not null) {
                    this.dataCache.Set((lat, lon), cachedData);
                }
            }
        } catch (Exception e) when (e is HttpRequestException or JsonSerializationException) {
            Log.Error(e, "Failed to fetch/deserialize Weather API response");
        } finally {
            this.dataSem.Release();
        }

        return cachedData;
    }
    
    public string? GraphFor(string query)
    {
        if (this.IsDisabled || string.IsNullOrWhiteSpace(query))
            return null;

        return $"{GraphApiUrl}/{WebUtility.UrlEncode(query)}.png";
    }
}