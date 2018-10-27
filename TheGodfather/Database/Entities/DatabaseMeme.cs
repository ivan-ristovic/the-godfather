#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

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
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("name"), Required, MaxLength(32)]
        public string Name { get; set; }

        [Column("url"), Required, MaxLength(128)]
        public string Url { get; set; }


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }
}
