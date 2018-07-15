namespace TheGodfather.Modules.Chickens.Common
{
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
            switch (stat) {
                case ChickenStatUpgrade.Strength: return "STR";
                case ChickenStatUpgrade.Vitality: return "HP";
                case ChickenStatUpgrade.MaxVitality: return "MAXHP";
            }

            return "?";
        }
    }

    public class ChickenUpgrade
    {
        public int Id { get; set; }
        public int Modifier { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
        public ChickenStatUpgrade UpgradesStat { get; set; }
    }
}
