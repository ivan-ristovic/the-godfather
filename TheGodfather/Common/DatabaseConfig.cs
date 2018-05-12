using Newtonsoft.Json;

namespace TheGodfather.Common
{
    public sealed class DatabaseConfig
    {
        [JsonProperty("hostname")]
        public string Hostname { get; private set; }

        [JsonProperty("port")]
        public int Port { get; private set; }

        [JsonProperty("database")]
        public string Database { get; private set; }

        [JsonProperty("username")]
        public string Username { get; private set; }

        [JsonProperty("password")]
        public string Password { get; private set; }

        [JsonIgnore]
        public static DatabaseConfig Default
        {
            get {
                return new DatabaseConfig {
                    Hostname = "localhost",
                    Port = 5432,
                    Database = "gf",
                    Username = "<insert username>",
                    Password = "<insert password>"
                };
            }
        }
    }
}
