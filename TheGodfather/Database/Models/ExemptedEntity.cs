using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    public enum ExemptedEntityType : byte
    {
        Channel = 0,
        Member = 1,
        Role = 2
    }

    public static class EntityTypeExtensions
    {
        public static char ToFlag(this ExemptedEntityType entity)
        {
            return entity switch
            {
                ExemptedEntityType.Channel => 'c',
                ExemptedEntityType.Member => 'm',
                ExemptedEntityType.Role => 'r',
                _ => '?',
            };
        }

        public static string ToUserFriendlyString(this ExemptedEntityType entity)
            => Enum.GetName(typeof(ExemptedEntityType), entity) ?? "Unknown";
    }

    public class ExemptedEntity
    {
        [Column("xid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdDb { get; set; }
        [NotMapped]
        public ulong Id { get => (ulong)this.IdDb; set => this.IdDb = (long)value; }

        [ForeignKey("GuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("type")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ExemptedEntityType Type { get; set; }


        public virtual GuildConfig GuildConfig { get; set; } = null!;
    }

    [Table("exempt_antispam")]
    public class ExemptedAntispamEntity : ExemptedEntity
    {

    }

    [Table("exempt_logging")]
    public class ExemptedLoggingEntity : ExemptedEntity
    {

    }

    [Table("exempt_ratelimit")]
    public class ExemptedRatelimitEntity : ExemptedEntity
    {

    }
}
