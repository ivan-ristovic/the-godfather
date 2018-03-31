#region USING_DIRECTIVES
using System;

using TheGodfather.Common;

using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Voice.Common
{
    public class SongInfo
    {
        public string Provider { get; set; }
        public string Query { get; set; }
        public string Queuer { get; set; }
        public string Title { get; set; }
        public string Uri { get; set; }
        public string Thumbnail { get; set; }
        public TimeSpan TotalTime { get; set; } = TimeSpan.Zero;
        public string VideoId { get; set; }


        public DiscordEmbed Embed()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.Headphones} {Title}",
                Color = DiscordColor.Red,
                ThumbnailUrl = Thumbnail,
                Url = Query
            };
            emb.AddField("Duration", $"{TotalTime:hh\\:mm\\:ss}", inline: true)
               .AddField("Added by", Queuer ?? "???", inline: true);
            emb.WithFooter($"Provider: {Provider}");
            return emb.Build();
        }
    }
}