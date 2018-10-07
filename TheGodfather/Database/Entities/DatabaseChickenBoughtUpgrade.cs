using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("chicken_bought_upgrades")]
    public class DatabaseChickenBoughtUpgrade
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        
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

        
        public virtual DatabaseChicken DbChicken { get; set; }
        public virtual DatabaseChickenUpgrade DbChickenUpgrade { get; set; }
    }
}
