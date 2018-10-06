using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("guild_ranks")]
    public class DatabaseGuildRank
    {
        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId => (ulong)this.GuildIdDb;

        [Column("rank"), Required]
        public short Rank { get; set; }

        [Column("name"), Required]
        public string Name { get; set; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }
}
