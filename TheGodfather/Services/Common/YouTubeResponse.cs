using System.Collections.Generic;
using Newtonsoft.Json;

namespace TheGodfather.Services.Common
{
    public sealed class YoutubeResponse
    {
        [JsonProperty("items")]
        public List<Dictionary<string, string>> Items { get; set; }
    }
}
