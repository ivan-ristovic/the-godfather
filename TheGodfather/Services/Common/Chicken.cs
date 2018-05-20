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
        public static readonly int DefaultPrice = 1000;
        public static readonly int TrainPrice = 500;

        public DiscordUser Owner { get; set; }
        public ulong OwnerId { get; set; }
        public string Name { get; set; }
        public short Strength
        {
            get => _strength;
            set {
                if (value > 999)
                    value = 999;
                else if(value < 0)
                    value = 0;
            }
        }

        private short _strength;


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
                _strength += 4;
                return true;
            } else {
                _strength -= 3;
                if (_strength < 0)
                    _strength = 0;
                return false;
            }
        }

        public Chicken Fight(Chicken other)
        {
            int chance = 50 + _strength - other._strength;

            if (_strength > other._strength) {
                if (chance > 99)
                    chance = 99;
            } else {
                if (chance < 1)
                    chance = 1;
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
            emb.AddField("Strength", _strength.ToString(), inline: true);
            emb.AddField("Credit value", (500 + _strength * 10).ToString(), inline: true);

            emb.WithFooter("Chickens will rule the world someday");

            return emb.Build();
        }
    }
}
