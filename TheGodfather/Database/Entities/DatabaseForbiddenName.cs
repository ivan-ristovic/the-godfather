using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using TheGodfather.Extensions;

namespace TheGodfather.Database.Entities
{
    [Table("forbidden_names")]
    public class DatabaseForbiddenName
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("name_regex"), Required, MaxLength(64)]
        public string RegexString { get; set; }

        [NotMapped]
        public Regex Regex => this.RegexString.ToRegex();


        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }
}
