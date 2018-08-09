#region USING_DIRECTIVES
using Newtonsoft.Json;
#endregion

namespace TheGodfather.Services.Common
{
    public sealed class DatabaseConfig
    {
        [JsonProperty("database")]
        public string DatabaseName { get; private set; }

        [JsonProperty("hostname")]
        public string Hostname { get; private set; }

        [JsonProperty("password")]
        public string Password { get; private set; }

        [JsonProperty("port")]
        public int Port { get; private set; }

        [JsonProperty("username")]
        public string Username { get; private set; }


        [JsonIgnore]
        public static DatabaseConfig Default => new DatabaseConfig {
            DatabaseName = "gf",
            Hostname = "localhost",
            Password = "<insert password>",
            Port = 5432,
            Username = "<insert username>"
        };
    }
}