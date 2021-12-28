#nullable disable
using Newtonsoft.Json;

namespace TheGodfather.Modules.Search.Common;

public class UrbanDictData
{
    [JsonProperty("tags")]
    public string[] Tags { get; set; }

    [JsonProperty("result_type")]
    public string ResultType { get; set; }

    [JsonProperty("list")]
    public UrbanDictList[] List { get; set; }

    [JsonProperty("sounds")]
    public string[] Sounds { get; set; }
}

public class UrbanDictList
{
    [JsonProperty("definition")]
    public string Definition { get; set; }

    [JsonProperty("permalink")]
    public string Permalink { get; set; }

    [JsonProperty("thumbs_up")]
    public int ThumbsUp { get; set; }

    [JsonProperty("author")]
    public string Author { get; set; }

    [JsonProperty("word")]
    public string Word { get; set; }

    [JsonProperty("defid")]
    public int DefinitionId { get; set; }

    [JsonProperty("current_vote")]
    public string CurrentVote { get; set; }

    [JsonProperty("example")]
    public string Example { get; set; }

    [JsonProperty("thumbs_down")]
    public int ThumbsDown { get; set; }
}