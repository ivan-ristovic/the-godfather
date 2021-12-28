#nullable disable
using Newtonsoft.Json;

namespace TheGodfather.Modules.Search.Common;

public class CryptoResponse
{
    [JsonProperty("status")]
    public CryptoResponseStatus Status { get; set; }

    [JsonProperty("data")]
    public List<CryptoResponseData> Data { get; set; }


    public bool IsSuccess => this.Status.ErrorCode == 0;
}

public class CryptoResponseStatus
{
    [JsonProperty("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonProperty("error_code")]
    public int ErrorCode { get; set; }

    [JsonProperty("error_message")]
    public string ErrorMessage { get; set; }
}

public class CryptoResponseData
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("symbol")]
    public string Symbol { get; set; }

    [JsonProperty("slug")]
    public string Slug { get; set; }

    [JsonProperty("cmc_rank")]
    public int Rank { get; set; }

    [JsonProperty("last_updated")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonProperty("quote")]
    public CryptoResponseQuotes Quotes { get; set; }
}

public class CryptoResponseQuotes
{
    [JsonProperty("USD")]
    public CryptoResponseQuote USD { get; set; }
}

public class CryptoResponseQuote
{
    [JsonProperty("price")]
    public double Price { get; set; }

    [JsonProperty("market_cap")]
    public double MarketCap { get; set; }

    [JsonProperty("percent_change_1h")]
    public string PercentChangeHour { get; set; }

    [JsonProperty("percent_change_24h")]
    public string PercentChangeDay { get; set; }

    [JsonProperty("percent_change_7d")]
    public string PercentChangeWeek { get; set; }

    [JsonProperty("percent_change_30d")]
    public string PercentChangeMonth { get; set; }

    [JsonProperty("volume_24h")]
    public double? VolumeDay { get; set; }
}