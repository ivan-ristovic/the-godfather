#region USING_DIRECTIVES
using System.Collections.Generic;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Services.Common
{
    public class QuoteApiResponse
    {
        [JsonProperty("success")]
        public QuoteApiSuccess Success { get; set; }

        [JsonProperty("contents")]
        public QuoteApiContent Contents { get; set; }
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
        [JsonProperty("quote")]
        public string Content { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("permalink")]
        public string Permalink { get; set; }

        [JsonProperty("background")]
        public string BackgroundImageUrl { get; set; }


        public DiscordEmbed Embed(string title = null)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = title ?? "Quote",
                Description = Formatter.Italic($"\"{Content}\""),
                Color = DiscordColor.SpringGreen,
                ThumbnailUrl = BackgroundImageUrl,
                Url = Permalink
            };
            emb.AddField("Author", Author);
            emb.WithFooter("Powered by theysaidso.com");

            return emb.Build();
        }
    }
}
