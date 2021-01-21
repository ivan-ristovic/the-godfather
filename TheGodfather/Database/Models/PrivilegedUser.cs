using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("privileged_users")]
    public class PrivilegedUser : IEquatable<PrivilegedUser>
    {
        [Key]
        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }


        public bool Equals(PrivilegedUser? other)
            => !(other is null) && this.UserId == other.UserId;

        public override bool Equals(object? obj)
            => this.Equals(obj as PrivilegedUser);

        public override int GetHashCode()
            => this.UserId.GetHashCode();
    }
}
