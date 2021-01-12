using Newtonsoft.Json;

namespace TheGodfather.Services.Common
{
    public sealed class LavalinkConfig
    {
        [JsonProperty("hostname")]
        public string Hostname { get; set; } = "localhost";

        [JsonProperty("port")]
        public int Port { get; set; } = 2333;

        [JsonProperty("password")]
        public string Password { get; set; } = "youshallnotpass";

        [JsonProperty("retry")]
        public int RetryAmount { get; set; } = 0;
    }
}
