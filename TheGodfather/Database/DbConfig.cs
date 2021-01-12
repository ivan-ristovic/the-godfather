using Newtonsoft.Json;

namespace TheGodfather.Database
{
    public sealed class DbConfig
    {
        [JsonProperty("database")]
        public string DatabaseName { get; set; } = "gfdb";

        [JsonProperty("provider")]
        public DbProvider Provider { get; set; } = DbProvider.Sqlite;

        [JsonProperty("hostname")]
        public string Hostname { get; set; } = "localhost";

        [JsonProperty("password")]
        public string Password { get; set; } = "password";

        [JsonProperty("port")]
        public int Port { get; set; } = 5432;

        [JsonProperty("username")]
        public string Username { get; set; } = "username";
    }
}