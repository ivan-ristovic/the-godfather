#region USING_DIRECTIVES
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
#endregion

namespace TheGodfather.Modules.Chickens.Common
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
        public static readonly ImmutableDictionary<ChickenType, ChickenStats> StartingStats = new Dictionary<ChickenType, ChickenStats>() {
            { ChickenType.Default, new ChickenStats() { BareStrength = 50, BareMaxVitality = 100, BareVitality = 100 } },
            { ChickenType.WellFed, new ChickenStats() { BareStrength = 100, BareMaxVitality = 150, BareVitality = 150 } },
            { ChickenType.Trained, new ChickenStats() { BareStrength = 150, BareMaxVitality = 200, BareVitality = 200 } },
            { ChickenType.SteroidEmpowered, new ChickenStats() { BareStrength = 200, BareMaxVitality = 250, BareVitality = 250 } },
            { ChickenType.Alien, new ChickenStats() { BareStrength = 250, BareMaxVitality = 300, BareVitality = 300 } },
        }.ToImmutableDictionary();
        public static long Price(ChickenType type)
            => PriceForAttribute(StartingStats[type].BareStrength);

        private static long PriceForAttribute(int attr)
            => (long)Math.Pow(10, 2 + attr / (double)50);


        public static Chicken FromDatabase(DatabaseContextBuilder dbb, ulong gid, ulong uid)
        {
            Chicken chicken = null;
            using (DatabaseContext db = dbb.CreateContext()) {
                DatabaseChicken dbc = db.Chickens
                    .Include(c => c.DbUpgrades)
                        .ThenInclude(u => u.DbChickenUpgrade)
                    .SingleOrDefault(c => c.GuildId == gid && c.UserId == uid);
                chicken = FromDatabaseChicken(dbc);
            }
            return chicken;
        }

        public static Chicken FromDatabaseChicken(DatabaseChicken dbc)
        {
            if (dbc == null)
                return null;

            return new Chicken() {
                GuildId = dbc.GuildId,
                Name = dbc.Name,
                OwnerId = dbc.UserId,
                Stats = new ChickenStats() {
                    BareStrength = dbc.Strength,
                    BareMaxVitality = dbc.MaxVitality,
                    BareVitality = dbc.Vitality,
                    Upgrades = dbc.DbUpgrades.Select(u => new ChickenUpgrade() {
                        Id = u.Id,
                        Modifier = u.DbChickenUpgrade.Modifier,
                        Name = u.DbChickenUpgrade.Name,
                        Price = u.DbChickenUpgrade.Cost,
                        UpgradesStat = u.DbChickenUpgrade.UpgradesStat
                    }).ToList().AsReadOnly()
                }
            };
        }

        public DiscordUser Owner { get; set; }
        public ulong OwnerId { get; set; }
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public ChickenStats Stats { get; set; }
        public long SellPrice => PriceForAttribute(this.Stats.BareStrength);
        public long TrainStrengthPrice 
            => PriceForAttribute(this.Stats.BareStrength + 3) - PriceForAttribute(this.Stats.BareStrength);
        public long TrainVitalityPrice 
            => PriceForAttribute(this.Stats.BareMaxVitality + 3) - PriceForAttribute(this.Stats.BareMaxVitality);


        public bool TrainStrength()
        {
            if (GFRandom.Generator.GetBool()) {
                this.Stats.BareStrength += 5;
                return true;
            } else {
                this.Stats.BareStrength -= 3;
                return false;
            }
        }

        public bool TrainVitality()
        {
            if (GFRandom.Generator.GetBool()) {
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

            return GFRandom.Generator.Next(100) < chance ? this : other;
        }

        public int DetermineStrengthGain(Chicken loser)
        {
            int str1 = this.Stats.TotalStrength;
            int str2 = loser.Stats.TotalStrength;

            if (str1 > str2)
                return Math.Max(7 - (str1 - str2) / 5, 1);
            else if (str2 > str1)
                return (str2 - str1) / 5 + 5;
            else
                return 5;
        }

        public DiscordEmbed ToDiscordEmbed(DiscordUser owner)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.Chicken} {this.Name}",
                Color = DiscordColor.Yellow
            };

            emb.AddField("Owner", owner.Mention, inline: true);
            emb.AddField("Credit value", $"{this.SellPrice:n0}", inline: true);
            emb.AddField("Stats", this.Stats.ToString(), inline: true);
            if (this.Stats.Upgrades.Any())
                emb.AddField("Upgrades", string.Join(", ", this.Stats.Upgrades.Select(u => u.Name)), inline: true);

            emb.WithFooter("Chickens will rule the world someday", owner.AvatarUrl);

            return emb.Build();
        }

        public DatabaseChicken ToDatabaseChicken()
        {
            return new DatabaseChicken() {
                DbUpgrades = this.Stats.Upgrades.Select(u => new DatabaseChickenBoughtUpgrade() {
                    GuildIdDb = (long)this.GuildId,
                    Id = u.Id,
                    UserIdDb = (long)this.OwnerId
                }).ToList(),
                GuildIdDb = (long)this.GuildId,
                MaxVitality = this.Stats.BareMaxVitality,
                Name = this.Name,
                Strength = this.Stats.BareStrength,
                UserIdDb = (long)this.OwnerId,
                Vitality = this.Stats.BareVitality
            };
        }
    }
}
