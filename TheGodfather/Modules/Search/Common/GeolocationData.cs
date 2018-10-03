#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;

using Newtonsoft.Json;
#endregion

namespace TheGodfather.Modules.Search.Common
{
    public class IpInfo
    {
        [JsonIgnore]
        private static readonly string _unknown = Formatter.Italic("Unknown");

        [JsonIgnore]
        public bool Success => this.Status == "success";


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

        [JsonProperty("regionCode")]
        public string RegionCode { get; set; }

        [JsonProperty("region")]
        public string RegionName { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

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


        public DiscordEmbed ToDiscordEmbed(DiscordColor? color = null)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"IP geolocation info for {this.Ip}",
            };
            
            if (!(color is null))
                emb.WithColor(color.Value);

            emb.AddField("Location", $"{this.City}, {this.RegionName} {this.RegionCode}, {this.CountryName} {this.CountryCode}");
            emb.AddField("Exact location", $"({this.Latitude}, {this.Longitude})", inline: true);
            emb.AddField("ISP", this.Isp ?? _unknown, inline: true);
            emb.AddField("Organization", this.Organization ?? _unknown, inline: true);
            emb.AddField("AS number", this.As ?? _unknown, inline: true);

            emb.WithFooter("Powered by ip-api.");

            return emb.Build();
        }
    }
}
