#region USING_DIRECTIVES
using System;
using System.IO;
using Newtonsoft.Json;

using DSharpPlus;
#endregion

namespace TheGodfather.Common
{
    public sealed class BotConfig
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("shard-count")]
        public int ShardCount { get; private set; }

        [JsonProperty("prefix")]
        public string DefaultPrefix { get; private set; }

        [JsonProperty("db_sync_interval")]
        public int DbSyncInterval { get; private set; }

        [JsonProperty("feed_check_start_delay")]
        public int FeedCheckStartDelay { get; private set; }

        [JsonProperty("feed_check_interval")]
        public int FeedCheckInterval { get; private set; }

        [JsonProperty("log-level")]
        public LogLevel LogLevel { get; private set; }

        [JsonProperty("log-path")]
        public string LogPath { get; private set; }

        [JsonProperty("log-to-file")]
        public bool LogToFile { get; private set; }

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

        [JsonProperty("key-omdb")]
        public string OMDbKey { get; private set; }

        [JsonProperty("db-config")]
        public DatabaseConfig DatabaseConfig { get; private set; }


        public static BotConfig Load()
        {
            if (!File.Exists("Resources/config.json"))
                return null;
            try {
                var cfg = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("Resources/config.json"));
                return cfg;
            } catch (Exception e) {
                Console.WriteLine("EXCEPTION OCCURED WHILE LOADING CONFIG FILE: " + Environment.NewLine + e.ToString());
                return null;
            }
        }

        [JsonIgnore]
        public static BotConfig Default => new BotConfig {
            Token = "<insert bot token here>",
            ShardCount = 1,
            DefaultPrefix = "!",
            DbSyncInterval = 600,
            FeedCheckStartDelay = 30,
            FeedCheckInterval = 300,
            LogLevel = LogLevel.Info,
            LogPath = "log.txt",
            LogToFile = false,
            GiphyKey = "<insert GIPHY API key>",
            SteamKey = "<insert Steam API key>",
            ImgurKey = "<insert Imgur API key>",
            WeatherKey = "<insert OpenWeatherMaps API key>",
            YouTubeKey = "<insert YouTube API key>",
            OMDbKey = "<insert OMDb API key>",
            DatabaseConfig = DatabaseConfig.Default
        };
    }
}
