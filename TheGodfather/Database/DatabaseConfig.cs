using Newtonsoft.Json;

namespace TheGodfather.Database
{
    public sealed class DatabaseConfig
    {
        [JsonProperty("database")]
        public string DatabaseName { get; set; }

        [JsonProperty("provider")]
        public DatabaseManagementSystem Provider { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }


        [JsonIgnore]
        public static DatabaseConfig Default => new DatabaseConfig {
            DatabaseName = "gfdb",
            Provider = DatabaseManagementSystem.Sqlite,
            Hostname = "localhost",
            Password = "gfdb",
            Port = 5432,
            Username = ""
        };
    }
}