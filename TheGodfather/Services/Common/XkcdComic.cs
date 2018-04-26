#region USING_DIRECTIVES
using System;
using Newtonsoft.Json;

using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Services.Common
{
    public class XkcdComic
    {
        [JsonProperty("month")]
        public string Month { get; set; }

        [JsonProperty("num")]
        public int Num { get; set; }

        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("safe_title")]
        public string Title { get; set; }

        [JsonProperty("img")]
        public string ImageUrl { get; set; }

        [JsonProperty("alt")]
        public string Alt { get; set; }


        public DiscordEmbed Embed()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"xkcd #{Num} : {Title}",
                ImageUrl = ImageUrl,
                Url = $"{XkcdService.XkcdUrl}/{Num}"
            };

            if (DateTime.TryParse(Month + Year, out var date))
                emb.WithTimestamp(date);

            return emb.Build();
        }
    }
}
