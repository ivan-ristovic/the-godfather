using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("chicken_bought_upgrades")]
    public class ChickenBoughtUpgrade : IEquatable<ChickenBoughtUpgrade>
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

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


        public virtual Chicken Chicken { get; set; } = null!;
        public virtual ChickenUpgrade Upgrade { get; set; } = null!;


        public bool Equals(ChickenBoughtUpgrade? other)
            => other is { } && this.GuildId == other.GuildId && this.UserId == other.UserId && this.Id == other.Id;

        public override bool Equals(object? other)
            => this.Equals(other as ChickenBoughtUpgrade);

        public override int GetHashCode()
            => (this.GuildId, this.UserId, this.Id).GetHashCode();
    }
}
