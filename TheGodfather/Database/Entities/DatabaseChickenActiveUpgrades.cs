using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("chicken_active_upgrades")]
    public partial class DatabaseChickenActiveUpgrades
    {
        public long Uid { get; set; }
        public long Gid { get; set; }
        public int Wid { get; set; }

        public virtual DatabaseChickens Chickens { get; set; }
        public virtual DatabaseChickenUpgrades W { get; set; }
    }
}
