using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("memes")]
    public class Meme
    {
        public const int NameLimit = 32;
        public const int UrlLimit = 128;

        [ForeignKey("GuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("name"), Required, MaxLength(NameLimit)]
        public string Name { get; set; } = null!;

        [Column("url"), Required, MaxLength(UrlLimit)]
        public string Url { get; set; } = null!;


        public virtual GuildConfig GuildConfig { get; set; } = null!;
    }
}
