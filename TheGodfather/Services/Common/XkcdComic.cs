#region USING_DIRECTIVES
using DSharpPlus.Entities;
using Newtonsoft.Json;
#endregion

namespace TheGodfather.Services.Common
{
    public class XkcdComic
    {
        [JsonProperty("num")]
        public int Id { get; set; }

        [JsonProperty("img")]
        public string ImageUrl { get; set; }

        [JsonProperty("month")]
        public string Month { get; set; }

        [JsonProperty("safe_title")]
        public string Title { get; set; }

        [JsonProperty("year")]
        public string Year { get; set; }


        public DiscordEmbed ToDiscordEmbed()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"xkcd #{this.Id} : {this.Title}",
                ImageUrl = ImageUrl,
                Url = XkcdService.CreateUrlForComic(this.Id)
            };

            emb.WithFooter($"Publish date: {this.Month}/{this.Year}");

            return emb.Build();
        }
    }
}
