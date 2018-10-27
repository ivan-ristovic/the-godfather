#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

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
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("triggers"), Required]
        public string[] Triggers { get; set; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }

    [Table("reactions_emoji")]
    public class DatabaseEmojiReaction : DatabaseReaction
    {
        [Column("reaction"), Required, MaxLength(128)]
        public string Reaction { get; set; }
    }

    [Table("reactions_text")]
    public class DatabaseTextReaction : DatabaseReaction
    {
        [Column("response"), Required, MaxLength(128)]
        public string Response { get; set; }
    }
}
