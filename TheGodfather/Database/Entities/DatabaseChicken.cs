using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("chickens")]
    public class DatabaseChicken
    {

        public static DatabaseChicken CreateDummy(ulong gid, ulong uid)
            => new DatabaseChicken() { GuildIdDb = (long)gid, UserIdDb = (long)uid };


        public DatabaseChicken()
        {
            this.DbUpgrades = new HashSet<DatabaseChickenBoughtUpgrade>();
        }


        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId => (ulong)this.UserIdDb;

        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId => (ulong)this.GuildIdDb;

        [Column("name"), Required]
        [MaxLength(32)]
        public string Name { get; set; }

        [Column("str")]
        public int Strength { get; set; }

        [Column("vit")]
        public int Vitality { get; set; }

        [Column("max_vit")]
        public int MaxVitality { get; set; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
        public virtual ICollection<DatabaseChickenBoughtUpgrade> DbUpgrades { get; set; }
    }
}
