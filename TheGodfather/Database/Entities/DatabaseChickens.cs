using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("chickens")]
    public partial class DatabaseChickens
    {
        public DatabaseChickens()
        {
            this.ChickenActiveUpgrades = new HashSet<DatabaseChickenActiveUpgrades>();
        }

        public long Uid { get; set; }
        public long Gid { get; set; }
        public string Name { get; set; }
        public int Strength { get; set; }
        public int Vitality { get; set; }
        public int MaxVitality { get; set; }

        public virtual DatabaseGuildConfig G { get; set; }
        public virtual ICollection<DatabaseChickenActiveUpgrades> ChickenActiveUpgrades { get; set; }
    }
}
