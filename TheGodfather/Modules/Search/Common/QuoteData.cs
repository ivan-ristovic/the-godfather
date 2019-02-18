#region USING_DIRECTIVES

using Newtonsoft.Json;

using System.Collections.Generic;
#endregion

namespace TheGodfather.Modules.Search.Common
{
    public class QuoteApiResponse
    {
        [JsonProperty("contents")]
        public QuoteApiContent Contents { get; set; }

        [JsonProperty("success")]
        public QuoteApiSuccess Success { get; set; }
    }

    public class QuoteApiSuccess
    {
        [JsonProperty("total")]
        public int Count { get; set; }
    }

    public class QuoteApiContent
    {
        [JsonProperty("quotes")]
        public List<Quote> Quotes { get; set; }
    }

    public class Quote
    {
        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("background")]
        public string BackgroundImageUrl { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("quote")]
        public string Content { get; set; }

        [JsonProperty("permalink")]
        public string Permalink { get; set; }
    }
}
