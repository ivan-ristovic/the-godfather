using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    public class DatabaseSpecialRole
    {
        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId => (ulong)this.GuildIdDb;

        [Column("rid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RoleIdDb { get; set; }
        [NotMapped]
        public ulong RoleId => (ulong)this.RoleIdDb;


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
