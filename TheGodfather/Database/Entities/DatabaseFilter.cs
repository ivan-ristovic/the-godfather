using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("filters")]
    public class DatabaseFilter
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId => (ulong)this.GuildIdDb;

        [Column("trigger"), Required]
        public string Trigger { get; set; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }
}
