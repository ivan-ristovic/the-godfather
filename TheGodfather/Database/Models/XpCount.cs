using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("xp_count")]
    public class XpCount : IEquatable<XpCount>
    {
        [Key]
        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        [Column("xp")]
        public int XpDb { get; set; }
        [NotMapped]
        public uint Xp { get => (uint)this.XpDb; set => this.XpDb = (int)value; }


        public bool Equals(XpCount? other)
            => other is { } && this.UserId == other.UserId;

        public override bool Equals(object? other)
            => this.Equals(other as XpCount);

        public override int GetHashCode()
            => this.UserId.GetHashCode();
    }
}
