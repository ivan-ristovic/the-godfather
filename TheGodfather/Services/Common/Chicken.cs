#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using TheGodfather.Common;

using DSharpPlus;
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

    public class ChickenStats
    {
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
        public short Vitality
        {
            get => _vitality;
            set {
                if (value > MaxVitality)
                    _vitality = MaxVitality;
                else if (value < 0)
                    _vitality = 0;
                else
                    _vitality = value;
            }
        }
        public short MaxVitality { get; set; }

        private short _strength;
        private short _vitality;


        public override string ToString()
            => $"STR: {Formatter.Bold(Strength.ToString())} | VIT: {Formatter.Bold(Vitality.ToString())} / {Formatter.Bold(MaxVitality.ToString())}";
    }

    public class Chicken
    {
        public static readonly ImmutableDictionary<ChickenType, ChickenStats> StartingStats = new Dictionary<ChickenType, ChickenStats>() {
            { ChickenType.Default, new ChickenStats() { Strength = 50, MaxVitality = 100, Vitality = 100 } },
            { ChickenType.WellFed, new ChickenStats() { Strength = 90, MaxVitality = 150, Vitality = 150 } },
            { ChickenType.Trained, new ChickenStats() { Strength = 125, MaxVitality = 200, Vitality = 200 } },
            { ChickenType.SteroidEmpowered, new ChickenStats() { Strength = 150, MaxVitality = 250, Vitality = 250 } },
            { ChickenType.Alien, new ChickenStats() { Strength = 200, MaxVitality = 300, Vitality = 300 } },
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
        public ChickenStats Stats { get; set; }
        public long SellPrice => PriceForAttribute(Stats.Strength);
        public long TrainStrengthPrice => PriceForAttribute((short)(Stats.Strength + 4)) - PriceForAttribute(Stats.Strength);
        public long TrainVitalityPrice => PriceForAttribute((short)(Stats.MaxVitality + 4)) - PriceForAttribute(Stats.MaxVitality);



        private static long PriceForAttribute(short attr)
            => (long)Math.Pow(10, 1 + attr / (double)50);


        public bool TrainStrength()
        {
            if (GFRandom.Generator.GetBool()) {
                Stats.Strength += 5;
                return true;
            } else {
                Stats.Strength -= 3;
                return false;
            }
        }

        public bool TrainVitality()
        {
            if (GFRandom.Generator.GetBool()) {
                Stats.MaxVitality += 4;
                return true;
            } else {
                Stats.MaxVitality -= 3;
                if (Stats.Vitality > Stats.MaxVitality)
                    Stats.Vitality = Stats.MaxVitality;
                return false;
            }
        }

        public Chicken Fight(Chicken other)
        {
            int chance = 50 + Stats.Strength - other.Stats.Strength;

            if (Stats.Strength > other.Stats.Strength) {
                if (chance > 99)
                    chance = 99;
            } else {
                if (chance < 1)
                    chance = 1;
            }

            return GFRandom.Generator.Next(100) < chance ? this : other;
        }

        public short DetermineStrengthGain(Chicken loser)
        {
            short str1 = Stats.Strength;
            short str2 = loser.Stats.Strength;

            if (str1 > str2)
                return (short)Math.Max(7 - (str1 - str2) / 5, 1);
            else if (str2 > str1)
                return (short)((str2 - str1) / 5 + 5);
            else
                return 5;
        }

        public DiscordEmbed Embed(DiscordUser owner)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.Chicken} {Name}",
                Color = DiscordColor.Aquamarine
            };

            emb.AddField("Owner", owner.Mention, inline: true);
            emb.AddField("Credit value", SellPrice.ToString(), inline: true);
            emb.AddField("Stats", Stats.ToString());

            emb.WithFooter("Chickens will rule the world someday");

            return emb.Build();
        }
    }
}
