#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace TheGodfather.Database.Entities
{
    public class DatabaseSpecialRole
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
        public ulong RoleId { get => (ulong)this.RoleIdDb; set => this.RoleId = value; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }

    [Table("self_roles")]
    public class DatabaseSelfRole : DatabaseSpecialRole
    {

    }

    [Table("auto_roles")]
    public class DatabaseAutoRole : DatabaseSpecialRole
    {

    }
}
