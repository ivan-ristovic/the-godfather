#region USING_DIRECTIVES
using System;
using System.IO;
using Newtonsoft.Json;

using DSharpPlus;
#endregion

namespace TheGodfather.Entities
{
    public sealed class BotConfig
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("shard-count")]
        public int ShardCount { get; private set; }

        [JsonProperty("prefix")]
        public string DefaultPrefix { get; private set; }

        [JsonProperty("log-level")]
        public LogLevel LogLevel { get; private set; }

        [JsonProperty("key-giphy")]
        public string GiphyKey { get; private set; }

        [JsonProperty("key-steam")]
        public string SteamKey { get; private set; }

        [JsonProperty("key-imgur")]
        public string ImgurKey { get; private set; }

        [JsonProperty("key-weather")]
        public string WeatherKey { get; private set; }

        [JsonProperty("key-youtube")]
        public string YouTubeKey { get; private set; }

        [JsonProperty("db-config")]
        public DatabaseConfig DatabaseConfig { get; private set; }


        public static BotConfig Load()
        {
            BotConfig cfg = null;
            if (File.Exists("Resources/config.json")) {
                try {
                    cfg = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("Resources/config.json"));
                } catch (Exception e) {
                    Console.WriteLine("EXCEPTION OCCURED WHILE LOADING CONFIG FILE: " + Environment.NewLine + e.ToString());
                    return null;
                }
            } else {
                return null;
            }

            return cfg;
        }

        [JsonIgnore]
        public static BotConfig Default
        {
            get {
                return new BotConfig {
                    Token = "<insert token here>",
                    ShardCount = 1,
                    DefaultPrefix = "!",
                    LogLevel = LogLevel.Info,
                    GiphyKey = "<insert GIPHY API key>",
                    SteamKey = "<insert Steam API key>",
                    ImgurKey = "<insert Imgur API key>",
                    WeatherKey = "<insert OpenWeatherMaps API key>",
                    YouTubeKey = "<insert YouTube API key>",
                    DatabaseConfig = DatabaseConfig.Default
                };
            }
        }
    }

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
