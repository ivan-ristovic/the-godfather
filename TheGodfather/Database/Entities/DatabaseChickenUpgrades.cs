using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("chicken_upgrades")]
    public partial class DatabaseChickenUpgrades
    {
        public DatabaseChickenUpgrades()
        {
            this.ChickenActiveUpgrades = new HashSet<DatabaseChickenActiveUpgrades>();
        }

        public int Wid { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
        public short UpgradesStat { get; set; }
        public int Modifier { get; set; }

        public virtual ICollection<DatabaseChickenActiveUpgrades> ChickenActiveUpgrades { get; set; }
    }
}
