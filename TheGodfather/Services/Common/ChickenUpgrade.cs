namespace TheGodfather.Services.Common
{
    public enum UpgradedStat
    {
        Strength = 0,
        Vitality = 1,
        MaxVitality = 2
    }

    public static class UpgradedStatExtensions
    {
        public static string ToStatString(this UpgradedStat stat)
        {
            switch (stat) {
                case UpgradedStat.Strength: return "STR";
                case UpgradedStat.Vitality: return "HP";
                case UpgradedStat.MaxVitality: return "MAXHP";
            }

            return "?";
        }
    }


    public class ChickenUpgrade
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
        public UpgradedStat UpgradesStat { get; set; }
        public int Modifier { get; set; }
    }
}
