using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    public class DatabaseReaction
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId => (ulong)this.GuildIdDb;

        [Column("triggers"), Required]
        public string[] Triggers { get; set; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }

    [Table("reactions_emoji")]
    public class DatabaseEmojiReaction : DatabaseReaction
    {
        [Column("reaction"), Required]
        public string Reaction { get; set; }
    }

    [Table("reactions_text")]
    public class DatabaseTextReaction : DatabaseReaction
    {
        [Column("response"), Required]
        public string Response { get; set; }
    }
}
