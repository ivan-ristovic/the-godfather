using System.Collections.Generic;
using Newtonsoft.Json;
using Serilog.Events;
using TheGodfather.Database;

namespace TheGodfather.Services.Common
{
    public sealed class BotConfig
    {
        [JsonProperty("db-config")]
        public DbConfig DatabaseConfig { get; set; } = new DbConfig();

        [JsonProperty("db_sync_interval")]
        public int DatabaseSyncInterval { get; set; } = 600;

        [JsonProperty("prefix")]
        public string Prefix { get; set; } = "!";

        [JsonProperty("feed_check_interval")]
        public int FeedCheckInterval { get; set; } = 300;

        [JsonProperty("feed_check_start_delay")]
        public int FeedCheckStartDelay { get; set; } = 30;

        [JsonProperty("key-giphy")]
        public string GiphyKey { get; set; } = "<insert GIPHY API key>";

        [JsonProperty("key-goodreads")]
        public string GoodreadsKey { get; set; } = "<insert Goodreads API key>";

        [JsonProperty("key-imgur")]
        public string ImgurKey { get; set; } = "<insert Imgur API key>";

        [JsonProperty("locale")]
        public string Locale { get; set; } = "en-US";

        [JsonProperty("log-level")]
        public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

        [JsonProperty("log-path")]
        public string LogPath { get; set; } = "gf.log";

        [JsonProperty("log-to-file")]
        public bool LogToFile { get; set; } = false;

        [JsonProperty("key-omdb")]
        public string OMDbKey { get; set; } = "<insert OMDb API key>";

        [JsonProperty("shard-count")]
        public int ShardCount { get; set; } = 1;

        [JsonProperty("key-steam")]
        public string SteamKey { get; set; } = "<insert Steam API key>";

        [JsonProperty("key-weather")]
        public string WeatherKey { get; set; } = "<insert OpenWeather API key>";

        [JsonProperty("key-youtube")]
        public string YouTubeKey { get; set; } = "<insert YouTube API key>";

        [JsonProperty("token")]
        public string Token { get; set; } = "<insert Bot token>";

        [JsonProperty("logger-special-rules")]
        public List<SpecialLoggingRule> SpecialLoggerRules { get; set; } = new List<SpecialLoggingRule>();


        public sealed class SpecialLoggingRule
        {
            [JsonProperty("app")]
            public string Application { get; set; } = "";

            [JsonProperty("level")]
            public LogEventLevel MinLevel { get; set; }
        }
    }
}
