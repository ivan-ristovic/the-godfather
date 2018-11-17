#region USING_DIRECTIVES
using Newtonsoft.Json;
using static TheGodfather.Database.DatabaseContextBuilder;
#endregion

namespace TheGodfather.Database
{
    public sealed class DatabaseConfig
    {
        [JsonProperty("database")]
        public string DatabaseName { get; set; }

        [JsonProperty("provider")]
        public DatabaseProvider Provider { get; set; }

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
            Provider = DatabaseProvider.SQLite,
            Hostname = "localhost",
            Password = "<insert password>",
            Port = 5432,
            Username = "<insert username>"
        };
    }
}