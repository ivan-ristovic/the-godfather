using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    public abstract class SpecialRole : IEquatable<SpecialRole>
    {
        [ForeignKey("GuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("rid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RoleIdDb { get; set; }
        [NotMapped]
        public ulong RoleId { get => (ulong)this.RoleIdDb; set => this.RoleIdDb = (long)value; }


        public virtual GuildConfig GuildConfig { get; set; } = null!;


        public bool Equals(SpecialRole? other)
            => other is { } && this.GuildId == other.GuildId && this.RoleId == other.RoleId;

        public override bool Equals(object? other)
            => this.Equals(other as SpecialRole);

        public override int GetHashCode()
            => HashCode.Combine(this.GuildId, this.RoleId);
    }

    [Table("auto_roles")]
    public class AutoRole : SpecialRole, IEquatable<AutoRole>
    {
        public bool Equals(AutoRole? other)
            => base.Equals(other);
    }

    [Table("self_roles")]
    public class SelfRole : SpecialRole, IEquatable<SelfRole>
    {
        public bool Equals(SelfRole? other)
            => base.Equals(other);
    }
}
