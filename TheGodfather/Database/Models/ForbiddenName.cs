using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Database.Models
{
    [Table("forbidden_names")]
    public class ForbiddenName
    {
        public const int NameLimit = 128;

        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("GuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("name_regex"), Required, MaxLength(NameLimit)]
        public string RegexString { get; set; } = null!;

        [Column("action"), Required, MaxLength(NameLimit)]
        public PunishmentAction? ActionOverride { get; set; } = null;

        [NotMapped]
        public Regex Regex => this.RegexLazy ??= this.RegexString.ToRegex(this.Options);

        [NotMapped]
        public Regex? RegexLazy { get; set; }

        [NotMapped]
        public RegexOptions Options { get; set; } = RegexOptions.IgnoreCase;


        public virtual GuildConfig GuildConfig { get; set; } = null!;
    }
}
