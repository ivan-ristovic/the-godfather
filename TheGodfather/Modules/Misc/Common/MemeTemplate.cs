#nullable disable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TheGodfather.Modules.Misc.Common
{
    public sealed class MemeTemplate
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lines")]
        public int Lines { get; set; }

        [JsonProperty("styles")]
        public List<string> Styles { get; set; }

        [JsonProperty("blank")]
        public string Url { get; set; }
    }
}
