#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
#endregion

namespace TheGodfather.Services.Common
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


        public DiscordEmbed ToDiscordEmbed(string altTitle = null)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = string.IsNullOrWhiteSpace(altTitle) ? "Quote" : altTitle,
                Description = Formatter.Italic($"\"{this.Content}\""),
                Color = DiscordColor.SpringGreen,
                ThumbnailUrl = BackgroundImageUrl,
                Url = Permalink
            };
            emb.AddField("Author", this.Author);

            emb.WithFooter("Powered by theysaidso.com");

            return emb.Build();
        }
    }
}
