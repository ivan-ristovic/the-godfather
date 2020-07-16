using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("memes")]
    public class Meme
    {
        [ForeignKey("GuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("name"), Required, MaxLength(32)]
        public string Name { get; set; } = null!;

        [Column("url"), Required, MaxLength(128)]
        public string Url { get; set; } = null!;


        public virtual GuildConfig GuildConfig { get; set; } = null!;
    }
}
