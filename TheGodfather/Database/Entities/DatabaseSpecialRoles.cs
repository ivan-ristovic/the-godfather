#region USING_DIRECTIVES
using System;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace TheGodfather.Database.Entities
{
    public class DatabaseSpecialRole : IEquatable<DatabaseSpecialRole> 
    {
        [ForeignKey("DbGuildConfig")]
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


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }


        public bool Equals(DatabaseSpecialRole other)
            => !(other is null) && this.GuildId == other.GuildId && this.RoleId == other.RoleId;

        public override bool Equals(object other)
            => this.Equals(other as DatabaseSpecialRole);

        public override int GetHashCode() 
            => (this.GuildId, this.RoleId).GetHashCode();
    }

    [Table("auto_roles")]
    public class DatabaseAutoRole : DatabaseSpecialRole, IEquatable<DatabaseAutoRole>
    {
        public bool Equals(DatabaseAutoRole other)
            => base.Equals(other);
    }

    [Table("self_roles")]
    public class DatabaseSelfRole : DatabaseSpecialRole, IEquatable<DatabaseSelfRole>
    {
        public bool Equals(DatabaseSelfRole other) 
            => base.Equals(other);
    }
}
