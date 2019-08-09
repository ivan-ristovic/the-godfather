using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("privileged_users")]
    public class DatabasePrivilegedUser : IEquatable<DatabasePrivilegedUser>
    {
        [Key]
        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }


        public bool Equals(DatabasePrivilegedUser other)
            => !(other is null) && this.UserId == other.UserId;

        public override bool Equals(object other)
            => this.Equals(other as DatabasePrivilegedUser);

        public override int GetHashCode()
            => this.UserId.GetHashCode();
    }
}
