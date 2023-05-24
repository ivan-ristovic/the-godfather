#nullable disable
using Newtonsoft.Json;

namespace TheGodfather.Modules.Search.Common;

public class IpInfo
{
    [JsonIgnore]
    public bool Success => this.Status.Equals("success", StringComparison.InvariantCultureIgnoreCase);


    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("message")]
    public string ErrorMessage { get; set; }

    [JsonProperty("query")]
    public string Ip { get; set; }

    [JsonProperty("countryCode")]
    public string CountryCode { get; set; }

    [JsonProperty("country")]
    public string CountryName { get; set; }

    [JsonProperty("continentCode")]
    public string ContinentCode { get; set; }

    [JsonProperty("continent")]
    public string Continent { get; set; }

    [JsonProperty("regionCode")]
    public string RegionCode { get; set; }

    [JsonProperty("regionName")]
    public string RegionName { get; set; }

    [JsonProperty("region")]
    public string Region { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("district")]
    public string District { get; set; }
    
    [JsonProperty("zip")]
    public string ZipCode { get; set; }

    [JsonProperty("lat")]
    public float Latitude { get; set; }

    [JsonProperty("lon")]
    public float Longitude { get; set; }

    [JsonProperty("isp")]
    public string Isp { get; set; }

    [JsonProperty("as")]
    public string As { get; set; }

    [JsonProperty("org")]
    public string Organization { get; set; }
    
    [JsonProperty("hosting")]
    public bool Hosting { get; set; }

    [JsonProperty("proxy")]
    public bool Proxy { get; set; }

    [JsonProperty("mobile")]
    public bool Mobile { get; set; }

    [JsonProperty("asname")]
    public string ASName { get; set; }

    [JsonProperty("reverse")]
    public string Reverse { get; set; }
}