#region USING_DIRECTIVES
using DSharpPlus.Entities;
using System;
using TheGodfather.Common;
#endregion

namespace TheGodfather.Modules.Music.Common
{
    public class SongInfo
    {
        public string Provider { get; set; }
        public string Query { get; set; }
        public string Queuer { get; set; }
        public string Title { get; set; }
        public string Uri { get; set; }
        public string Thumbnail { get; set; }
        public string VideoId { get; set; }
        public TimeSpan TotalTime { get; set; } = TimeSpan.Zero;


        public DiscordEmbed ToDiscordEmbed()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.Headphones} {this.Title}",
                Color = DiscordColor.Red,
                ThumbnailUrl = this.Thumbnail,
                Url = Query
            };

            emb.AddField("Duration", $"{this.TotalTime:hh\\:mm\\:ss}", inline: true);
            emb.AddField("Added by", this.Queuer ?? "???", inline: true);

            emb.WithFooter($"Provider: {this.Provider}");

            return emb.Build();
        }
    }
}