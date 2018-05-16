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

        public DiscordUser Owner { get; set; }
        public ulong OwnerId { get; set; }
        public string Name { get; set; }
        public short Strength { get; set; }


        public static short DetermineGain(short str1, short str2)
        {
            if (str1 > str2)
                return (short)Math.Max(7 - (str1 - str2) / 5, 1);
            else if (str2 > str1)
                return (short)((str2 - str1) / 5 + 5);
            else
                return 5;
        }


        public bool Train()
        {
            if (GFRandom.Generator.GetBool()) {
                Strength += 4;
                return true;
            } else {
                Strength -= 3;
                if (Strength < 0)
                    Strength = 0;
                return false;
            }
        }

        public Chicken Fight(Chicken other)
        {
            int chance = 50 + Strength - other.Strength;

            if (Strength > other.Strength) {
                if (chance > 95)
                    chance = 95;
            } else {
                if (chance < 5)
                    chance = 5;
            }

            return GFRandom.Generator.Next(100) < chance ? this : other;
        }

        public DiscordEmbed Embed(DiscordUser owner)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.Chicken} {Name}",
                Color = DiscordColor.Aquamarine
            };

            emb.AddField("Owner", owner.Mention, inline: true);
            emb.AddField("Strength", Strength.ToString(), inline: true);
            emb.AddField("Credit value", (500 + Strength * 10).ToString(), inline: true);

            emb.WithFooter("Chickens will rule the world someday");

            return emb.Build();
        }
    }
}
