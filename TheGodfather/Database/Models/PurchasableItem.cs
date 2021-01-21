using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("purchasable_items")]
    public class PurchasableItem : IEquatable<PurchasableItem>
    {
        public const int NameLimit = 128;
        public const long PriceLimit = 100_000_000_000;


        public PurchasableItem()
        {
            this.Purchases = new HashSet<PurchasedItem>();
        }


        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("GuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("name"), Required, MaxLength(NameLimit)]
        public string Name { get; set; } = null!;

        [Column("price"), Required]
        public long Price { get; set; }


        public virtual GuildConfig GuildConfig { get; set; } = null!;
        public virtual ICollection<PurchasedItem> Purchases { get; set; }


        public bool Equals(PurchasableItem? other)
            => other is { } && this.Id == other.Id;

        public override bool Equals(object? obj)
            => this.Equals(obj as PurchasableItem);

        public override int GetHashCode()
            => this.Id.GetHashCode();
    }
}
