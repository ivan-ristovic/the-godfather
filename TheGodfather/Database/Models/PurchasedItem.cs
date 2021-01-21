using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("purchased_items")]
    public class PurchasedItem : IEquatable<PurchasedItem>
    {
        [ForeignKey("Item")]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ItemId { get; set; }

        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }


        public virtual PurchasableItem Item { get; set; } = null!;


        public bool Equals(PurchasedItem? other)
            => other is { } && this.ItemId == other.ItemId && this.UserId == other.UserId;

        public override bool Equals(object? obj)
            => this.Equals(obj as PurchasedItem);

        public override int GetHashCode()
            => HashCode.Combine(this.ItemId, this.UserId);
    }
}
