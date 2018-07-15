#region USING_DIRECTIVES
using DSharpPlus;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace TheGodfather.Modules.Chickens.Common
{
    public class ChickenStats
    {
        public int BareStrength {
            get => this.strength;
            set {
                if (value > 999)
                    this.strength = 999;
                else if (value < 0)
                    this.strength = 0;
                else
                    this.strength = value;
            }
        }
        public int BareVitality {
            get => this.vitality;
            set {
                if (value > this.BareMaxVitality)
                    this.vitality = this.BareMaxVitality;
                else if (value < 0)
                    this.vitality = 0;
                else
                    this.vitality = value;
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
                    ?.Where(u => u.UpgradesStat == ChickenStatUpgrade.Strength)
                    .Sum(u => u.Modifier);
                return this.BareStrength + (upgradedStrength ?? 0);
            }
        }
        public int TotalVitality {
            get {
                int? upgradedVitality = this.Upgrades
                    ?.Where(u => u.UpgradesStat == ChickenStatUpgrade.Vitality)
                    .Sum(u => u.Modifier);
                int total = this.BareVitality + (upgradedVitality ?? 0);
                return (total > this.TotalMaxVitality) ? this.TotalMaxVitality : total;
            }
        }
        public int TotalMaxVitality {
            get {
                int? upgradedMaxVitality = this.Upgrades
                    ?.Where(u => u.UpgradesStat == ChickenStatUpgrade.MaxVitality)
                    .Sum(u => u.Modifier);
                return this.BareMaxVitality + (upgradedMaxVitality ?? 0);
            }
        }
        public IReadOnlyList<ChickenUpgrade> Upgrades { get; internal set; }

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
