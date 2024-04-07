using Newtonsoft.Json;

namespace TheGodfather.Services.Common;

public sealed class LavalinkConfig
{
    [JsonProperty("enable")]
    public bool Enable { get; set; }

    [JsonProperty("hostname")]
    public string Hostname { get; set; } = "localhost";

    [JsonProperty("label")]
    public string Label { get; set; } = "LL Node";

    [JsonProperty("port")]
    public int Port { get; set; } = 2333;

    [JsonProperty("ready-timeout-s")]
    public int ReadyTimeout { get; set; } = 10;

    [JsonProperty("resume-timeout-s")]
    public int ResumptionTimeout { get; set; } = 60;

    [JsonProperty("password")]
    public string Password { get; set; } = "youshallnotpass";
}