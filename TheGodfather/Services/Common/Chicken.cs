#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

using TheGodfather.Common;

using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Services.Common
{
    public class Chicken
    {
        public static readonly int Price = 1000;

        public ulong OwnerId { get; set; }
        public string Name { get; set; }
        public short Strength { get; set; }


        public DiscordEmbed Embed(DiscordUser owner)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.Chicken} {Name}",
                Color = DiscordColor.Aquamarine
            };

            emb.AddField("Owner", owner.Mention, inline: true);
            emb.AddField("Strength", Strength.ToString(), inline: true);

            emb.WithFooter("Chickens will rule the world someday");

            return emb.Build();
        }
    }
}
