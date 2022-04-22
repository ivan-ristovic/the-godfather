using System.IO;
using Newtonsoft.Json;
using Serilog.Events;

namespace TheGodfather.Services.Common;

public sealed class BotConfig
{
    public const string DefaultLocale= "en-GB";
    public const string DefaultPrefix = "!";


    [JsonProperty("db-config")]
    public DbConfig DatabaseConfig { get; set; } = new();

    [JsonProperty("lava-config")]
    public LavalinkConfig LavalinkConfig { get; set; } = new();

    [JsonProperty("db_sync_interval")]
    public int DatabaseSyncInterval { get; set; } = 600;

    [JsonProperty("token")]
    public string? Token { get; set; }

    [JsonProperty("prefix")]
    public string Prefix { get; set; } = DefaultPrefix;

    [JsonProperty("shard-count")]
    public int ShardCount { get; set; } = 1;

    [JsonProperty("feed_check_interval")]
    public int FeedCheckInterval { get; set; } = 300;

    [JsonProperty("feed_check_start_delay")]
    public int FeedCheckStartDelay { get; set; } = 30;

    [JsonProperty("locale")]
    public string Locale { get; set; } = DefaultLocale;

    [JsonProperty("log-level")]
    public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

    [JsonProperty("log-path")]
    public string LogPath { get; set; } = Path.Combine("data", "logs", "gf.log");

    [JsonProperty("log-file-rolling")]
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;

    [JsonProperty("log-to-file")]
    public bool LogToFile { get; set; }

    [JsonProperty("log-buffer")]
    public bool UseBufferedFileLogger { get; set; }

    [JsonProperty("log-max-files")]
    public int? MaxLogFiles { get; set; }

    [JsonProperty("log-template")]
    public string? CustomLogTemplate { get; set; }

    [JsonProperty("backup-path")]
    public string BackupPath { get; set; } = Path.Combine("data", "backup");

    [JsonProperty("key-omdb")]
    public string? OMDbKey { get; set; }

    [JsonProperty("key-steam")]
    public string? SteamKey { get; set; }

    [JsonProperty("key-weather")]
    public string? WeatherKey { get; set; }

    [JsonProperty("key-youtube")]
    public string? YouTubeKey { get; set; }

    [JsonProperty("key-giphy")]
    public string? GiphyKey { get; set; }

    [JsonProperty("key-goodreads")]
    public string? GoodreadsKey { get; set; }

    [JsonProperty("key-imgur")]
    public string? ImgurKey { get; set; }

    [JsonProperty("key-crypto")]
    public string? CryptoKey { get; set; }

    [JsonProperty("logger-special-rules")]
    public List<SpecialLoggingRule> SpecialLoggerRules { get; set; } = new();


    public sealed class SpecialLoggingRule
    {
        [JsonProperty("app")]
        public string? Application { get; set; }

        [JsonProperty("level")]
        public LogEventLevel MinLevel { get; set; }
    }
}