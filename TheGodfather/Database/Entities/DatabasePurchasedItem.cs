using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("purchased_items")]
    public class DatabasePurchasedItem
    {
        [ForeignKey("DbPurchasableItem")]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ItemId { get; set; }

        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId => (ulong)this.UserIdDb;


        public virtual DatabasePurchasableItem DbPurchasableItem { get; set; }
    }
}
