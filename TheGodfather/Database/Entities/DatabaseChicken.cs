#region USING_DIRECTIVES
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("chickens")]
    public class DatabaseChicken
    {

        public DatabaseChicken()
        {
            this.DbUpgrades = new HashSet<DatabaseChickenBoughtUpgrade>();
        }


        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("name"), Required, MaxLength(32)]
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
