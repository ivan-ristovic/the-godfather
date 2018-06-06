#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
        public int TotalStrength
            => BareStrength + (Upgrades?.Where(u => u.UpgradesStat == UpgradedStat.Strength).Sum(u => u.Modifier) ?? 0);
        public int TotalVitality
        {
            get {
                var total = BareVitality + (Upgrades?.Where(u => u.UpgradesStat == UpgradedStat.Vitality).Sum(u => u.Modifier) ?? 0);
                return (total > TotalMaxVitality) ? TotalMaxVitality : total;
            }
        }
        public int TotalMaxVitality
        {
            get => BareMaxVitality + (Upgrades?.Where(u => u.UpgradesStat == UpgradedStat.MaxVitality).Sum(u => u.Modifier) ?? 0);
        }
        public int BareStrength
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
        public int BareVitality {
            get => _vitality;
            set {
                if (value > BareMaxVitality)
                    _vitality = BareMaxVitality;
                else if (value < 0)
                    _vitality = 0;
                else
                    _vitality = value;
            }
        }
        public int BareMaxVitality
        {
            get => _maxvitality;
            set {
                _maxvitality = value;
                if (_maxvitality < _vitality)
                    _vitality = _maxvitality;
            }
        }
        public IReadOnlyList<ChickenUpgrade> Upgrades { get; internal set; }

        private int _strength;
        private int _vitality;
        private int _maxvitality;


        public override string ToString()
            => $"STR: {Formatter.Bold(TotalStrength.ToString())} (bare: {Formatter.Bold(BareStrength.ToString())})\n" +
               $"VIT: {Formatter.Bold(TotalVitality.ToString())} / {Formatter.Bold(TotalMaxVitality.ToString())}";

        public string ToShortString()
            => $"STR: {Formatter.Bold(TotalStrength.ToString())} VIT: {Formatter.Bold(TotalVitality.ToString())}";
    }

    public class Chicken
    {
        public static readonly ImmutableDictionary<ChickenType, ChickenStats> StartingStats = new Dictionary<ChickenType, ChickenStats>() {
            { ChickenType.Default, new ChickenStats() { BareStrength = 50, BareMaxVitality = 100, BareVitality = 100 } },
            { ChickenType.WellFed, new ChickenStats() { BareStrength = 100, BareMaxVitality = 150, BareVitality = 150 } },
            { ChickenType.Trained, new ChickenStats() { BareStrength = 150, BareMaxVitality = 200, BareVitality = 200 } },
            { ChickenType.SteroidEmpowered, new ChickenStats() { BareStrength = 200, BareMaxVitality = 250, BareVitality = 250 } },
            { ChickenType.Alien, new ChickenStats() { BareStrength = 250, BareMaxVitality = 300, BareVitality = 300 } },
        }.ToImmutableDictionary();
        public static long Price(ChickenType type)
            => PriceForAttribute(StartingStats[type].BareStrength);

        public DiscordUser Owner { get; set; }
        public ulong OwnerId { get; set; }
        public string Name { get; set; }
        public ChickenStats Stats { get; set; }
        public long SellPrice => PriceForAttribute(Stats.BareStrength);
        public long TrainStrengthPrice => PriceForAttribute(Stats.BareStrength + 3) - PriceForAttribute(Stats.BareStrength);
        public long TrainVitalityPrice => PriceForAttribute(Stats.BareMaxVitality + 3) - PriceForAttribute(Stats.BareMaxVitality);


        private static long PriceForAttribute(int attr)
            => (long)Math.Pow(10, 2 + attr / (double)50);


        public bool TrainStrength()
        {
            if (GFRandom.Generator.GetBool()) {
                Stats.BareStrength += 5;
                return true;
            } else {
                Stats.BareStrength -= 3;
                return false;
            }
        }

        public bool TrainVitality()
        {
            if (GFRandom.Generator.GetBool()) {
                Stats.BareMaxVitality += 4;
                return true;
            } else {
                Stats.BareMaxVitality -= 3;
                return false;
            }
        }

        public Chicken Fight(Chicken other)
        {
            int chance = 50 + Stats.TotalStrength - other.Stats.TotalStrength;

            if (Stats.TotalStrength > other.Stats.TotalStrength) {
                if (chance > 99)
                    chance = 99;
            } else {
                if (chance < 1)
                    chance = 1;
            }

            return GFRandom.Generator.Next(100) < chance ? this : other;
        }

        public int DetermineStrengthGain(Chicken loser)
        {
            int str1 = Stats.TotalStrength;
            int str2 = loser.Stats.TotalStrength;

            if (str1 > str2)
                return Math.Max(7 - (str1 - str2) / 5, 1);
            else if (str2 > str1)
                return (str2 - str1) / 5 + 5;
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
            emb.AddField("Stats", Stats.ToString(), inline: true);
            if (Stats.Upgrades.Any())
                emb.AddField("Upgrades", string.Join(", ", Stats.Upgrades.Select(u => u.Name)), inline: true);

            emb.WithFooter("Chickens will rule the world someday", owner.AvatarUrl);

            return emb.Build();
        }
    }
}
