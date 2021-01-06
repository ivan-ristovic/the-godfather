using Newtonsoft.Json;

namespace TheGodfather.Services.Common
{
    public sealed class LavalinkConfig
    {
        [JsonProperty("hostname")]
        public string Hostname { get; private set; } = "localhost";

        [JsonProperty("port")]
        public int Port { get; private set; } = 2333;

        [JsonProperty("password")]
        public string Password { get; private set; } = "youshallnotpass";
    }
}
