namespace TheGodfather.Services.Common
{
    public enum ChickenStat
    {
        Strength = 0,
        Vitality = 1,
        MaxVitality = 2
    }

    public static class ChickenStatExtensions
    {
        public static string ToStatString(this ChickenStat stat)
        {
            switch (stat) {
                case ChickenStat.Strength: return "STR";
                case ChickenStat.Vitality: return "HP";
                case ChickenStat.MaxVitality: return "MAXHP";
            }

            return "?";
        }
    }


    public class ChickenUpgrade
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
        public ChickenStat UpgradesStat { get; set; }
        public short Modifier { get; set; }
    }
}
