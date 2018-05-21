#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using TheGodfather.Common;

using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Services.Common
{
    public enum ChickenType
    {
        Default = 0,
        WellFed = 1,
        Trained = 2,
        SteroidEmpowered = 3,
        Alien = 4
    }

    public class Chicken
    {
        public static readonly ImmutableDictionary<ChickenType, short> StartingStrength = new Dictionary<ChickenType, short>() {
            { ChickenType.Default, 50},
            { ChickenType.WellFed, 75},
            { ChickenType.Trained, 100},
            { ChickenType.SteroidEmpowered, 125},
            { ChickenType.Alien, 150}
        }.ToImmutableDictionary();
        public static readonly ImmutableDictionary<ChickenType, long> Price = new Dictionary<ChickenType, long>() {
            { ChickenType.Default, 1000},
            { ChickenType.WellFed, 10000},
            { ChickenType.Trained, 100000},
            { ChickenType.SteroidEmpowered, 1000000},
            { ChickenType.Alien, 10000000}
        }.ToImmutableDictionary();

        public DiscordUser Owner { get; set; }
        public ulong OwnerId { get; set; }
        public string Name { get; set; }
        public short Strength
        {
            get => _strength;
            set {
                if (value > 999)
                    _strength = 999;
                else if (value < 0)
                    _strength = 0;
                else
                    _strength = value;
            }
        }
        public long SellPrice => FindSellPriceForStrength(_strength);
        public long TrainPrice => FindSellPriceForStrength((short)(_strength + 4)) - FindSellPriceForStrength(_strength);

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

        private static long FindSellPriceForStrength(short str)
            => (long)Math.Pow(10, 1 + str / (double)50);


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
            emb.AddField("Credit value", SellPrice.ToString(), inline: true);

            emb.WithFooter("Chickens will rule the world someday");

            return emb.Build();
        }
    }
}
