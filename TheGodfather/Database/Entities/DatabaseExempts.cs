#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations.Schema;

using TheGodfather.Modules.Administration.Common;
#endregion

namespace TheGodfather.Database.Entities
{
    public class DatabaseExemptedEntity
    {
        [Column("xid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdDb { get; set; }
        [NotMapped]
        public ulong Id { get => (ulong)this.IdDb; set => this.IdDb = (long)value; }

        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("type")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ExemptedEntityType Type { get; set; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }

    [Table("exempt_antispam")]
    public class DatabaseExemptAntispam : DatabaseExemptedEntity
    {

    }

    [Table("exempt_logging")]
    public class DatabaseExemptLogging : DatabaseExemptedEntity
    {

    }

    [Table("exempt_ratelimit")]
    public class DatabaseExemptRatelimit : DatabaseExemptedEntity
    {

    }
}
