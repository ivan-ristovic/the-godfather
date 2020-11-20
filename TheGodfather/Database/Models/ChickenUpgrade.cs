using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("chicken_upgrades")]
    public class ChickenUpgrade
    {
        public const int NameLimit = 32;


        public ChickenUpgrade()
        {
            this.BoughtUpgrades = new HashSet<ChickenBoughtUpgrade>();
        }


        [Key, Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("name"), Required, MaxLength(NameLimit)]
        public string Name { get; set; } = null!;

        [Column("cost")]
        public long Cost { get; set; }

        [Column("stat")]
        public ChickenStatUpgrade UpgradesStat { get; set; }

        [Column("mod")]
        public int Modifier { get; set; }


        public virtual ICollection<ChickenBoughtUpgrade> BoughtUpgrades { get; set; }
    }
}
