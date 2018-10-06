using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("purchasable_items")]
    public class DatabasePurchasableItem
    {

        public DatabasePurchasableItem()
        {
            this.Purchases = new HashSet<DatabasePurchasedItem>();
        }


        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId => (ulong)this.GuildIdDb;

        [Column("name"), Required]
        public string Name { get; set; }

        [Column("price"), Required]
        public long Price { get; set; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }

        public virtual ICollection<DatabasePurchasedItem> Purchases { get; set; }
    }
}
