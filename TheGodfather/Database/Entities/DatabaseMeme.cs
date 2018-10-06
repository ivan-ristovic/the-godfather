using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("memes")]
    public class DatabaseMeme
    {
        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId => (ulong)this.GuildIdDb;

        [Column("name"), Required]
        public string Name { get; set; }

        [Column("url"), Required]
        public string Url { get; set; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }
}
