using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using TheGodfather.Common;

namespace TheGodfather.Database.Models
{
    public enum ChickenType
    {
        Default = 0,
        WellFed = 1,
        Trained = 2,
        SteroidEmpowered = 3,
        Alien = 4
    }

    public enum ChickenStatUpgrade
    {
        Strength = 0,
        Vitality = 1,
        MaxVitality = 2
    }

    public static class UpgradedStatExtensions
    {
        public static string ToShortString(this ChickenStatUpgrade stat)
        {
            return stat switch
            {
                ChickenStatUpgrade.Strength => "STR",
                ChickenStatUpgrade.Vitality => "HP",
                ChickenStatUpgrade.MaxVitality => "MAXHP",
                _ => "?",
            };
        }
    }

    [Table("chickens")]
    public class Chicken
    {
        public static readonly ImmutableDictionary<ChickenType, ChickenStats> StartingStats = new Dictionary<ChickenType, ChickenStats> {
            { ChickenType.Default, new ChickenStats { BareStrength = 50, BareMaxVitality = 100, BareVitality = 100 } },
            { ChickenType.WellFed, new ChickenStats { BareStrength = 100, BareMaxVitality = 150, BareVitality = 150 } },
            { ChickenType.Trained, new ChickenStats { BareStrength = 150, BareMaxVitality = 200, BareVitality = 200 } },
            { ChickenType.SteroidEmpowered, new ChickenStats { BareStrength = 200, BareMaxVitality = 250, BareVitality = 250 } },
            { ChickenType.Alien, new ChickenStats { BareStrength = 250, BareMaxVitality = 300, BareVitality = 300 } },
        }.ToImmutableDictionary();

        public static long Price(ChickenType type)
            => PriceForAttribute(StartingStats[type].BareStrength);

        private static long PriceForAttribute(int attr)
            => (long)Math.Pow(10, 2 + attr / (double)50);


        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("name"), Required, MaxLength(32)]
        public string Name { get; set; } = null!;

        [Column("str")]
        public int BareStrength { get; set; }

        [Column("vit")]
        public int Vitality { get; set; }

        [Column("max_vit")]
        public int BareMaxVitality { get; set; }


        [NotMapped]
        public ChickenStats Stats { get; set; }

        [NotMapped]
        public DiscordUser? Owner { get; set; }

        [NotMapped]
        public long SellPrice => PriceForAttribute(this.Stats.BareStrength);

        [NotMapped]
        public long TrainStrengthPrice
            => PriceForAttribute(this.Stats.BareStrength + 3) - PriceForAttribute(this.Stats.BareStrength);

        [NotMapped]
        public long TrainVitalityPrice
            => PriceForAttribute(this.Stats.BareMaxVitality + 3) - PriceForAttribute(this.Stats.BareMaxVitality);


        public virtual GuildConfig GuildConfig { get; set; } = null!;
        public virtual ICollection<ChickenBoughtUpgrade> Upgrades { get; set; }


        public Chicken()
        {
            this.Upgrades = new HashSet<ChickenBoughtUpgrade>();
            this.Stats = new ChickenStats {
                BareStrength = this.BareStrength,
                BareMaxVitality = this.BareMaxVitality,
                BareVitality = this.Vitality,
                Upgrades = this.Upgrades.ToList().AsReadOnly()
            };
        }

        public Chicken(ChickenType type)
        {
            this.Upgrades = new HashSet<ChickenBoughtUpgrade>();
            this.Stats = StartingStats[type];
            this.BareMaxVitality = this.Stats.BareMaxVitality;
            this.BareStrength = this.Stats.BareStrength;
            this.Vitality = this.Stats.BareVitality;
        }


        public bool TrainStrength()
        {
            if (new SecureRandom().NextBool()) {
                this.Stats.BareStrength += 5;
                return true;
            } else {
                this.Stats.BareStrength -= 3;
                return false;
            }
        }

        public bool TrainVitality()
        {
            if (new SecureRandom().NextBool()) {
                this.Stats.BareMaxVitality += 4;
                return true;
            } else {
                this.Stats.BareMaxVitality -= 3;
                return false;
            }
        }

        public Chicken Fight(Chicken other)
        {
            int chance = 50 + this.Stats.TotalStrength - other.Stats.TotalStrength;

            if (this.Stats.TotalStrength > other.Stats.TotalStrength) {
                if (chance > 99)
                    chance = 99;
            } else {
                if (chance < 1)
                    chance = 1;
            }

            return new SecureRandom().Next(100) < chance ? this : other;
        }

        public int DetermineStrengthGain(Chicken loser)
        {
            int str1 = this.Stats.TotalStrength;
            int str2 = loser.Stats.TotalStrength;
            return str1 > str2
                ? Math.Max(7 - (str1 - str2) / 5, 1)
                : str2 > str1 ? (str2 - str1) / 5 + 5 : 5;
        }
    }

    public sealed class ChickenStats
    {
        public int BareStrength {
            get => this.strength;
            set => this.strength = value > 999 ? 999 : (value < 0 ? 0 : value);
        }
        public int BareVitality {
            get => this.vitality;
            set {
                this.vitality = value > this.BareMaxVitality
                    ? this.BareMaxVitality
                    : value < 0 ? 0 : value;
            }
        }
        public int BareMaxVitality {
            get => this.maxvitality;
            set {
                this.maxvitality = value;
                if (this.maxvitality < this.vitality)
                    this.vitality = this.maxvitality;
            }
        }
        public int TotalStrength {
            get {
                int? upgradedStrength = this.Upgrades
                    ?.Where(u => u.Upgrade.UpgradesStat == ChickenStatUpgrade.Strength)
                    .Sum(u => u.Upgrade.Modifier);
                return this.BareStrength + (upgradedStrength ?? 0);
            }
        }
        public int TotalVitality {
            get {
                int? upgradedVitality = this.Upgrades
                    ?.Where(u => u.Upgrade.UpgradesStat == ChickenStatUpgrade.Vitality)
                    .Sum(u => u.Upgrade.Modifier);
                int total = this.BareVitality + (upgradedVitality ?? 0);
                return (total > this.TotalMaxVitality) ? this.TotalMaxVitality : total;
            }
        }
        public int TotalMaxVitality {
            get {
                int? upgradedMaxVitality = this.Upgrades
                    ?.Where(u => u.Upgrade.UpgradesStat == ChickenStatUpgrade.MaxVitality)
                    .Sum(u => u.Upgrade.Modifier);
                return this.BareMaxVitality + (upgradedMaxVitality ?? 0);
            }
        }
        public IReadOnlyList<ChickenBoughtUpgrade>? Upgrades { get; internal set; }

        private int strength;
        private int vitality;
        private int maxvitality;


        public string ToShortString()
            => $"STR: {Formatter.Bold(this.TotalStrength.ToString())} VIT: {Formatter.Bold(this.TotalVitality.ToString())}";

        public override string ToString()
            => $"STR: {Formatter.Bold(this.TotalStrength.ToString())} (bare: {Formatter.Bold(this.BareStrength.ToString())})\n" +
               $"VIT: {Formatter.Bold(this.TotalVitality.ToString())} / {Formatter.Bold(this.TotalMaxVitality.ToString())}";
    }
}
