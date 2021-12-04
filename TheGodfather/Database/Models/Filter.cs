using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using TheGodfather.Extensions;

namespace TheGodfather.Database.Models
{
    [Table("filters")]
    public class Filter
    {
        public const int FilterLimit = 128;

        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("GuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("trigger"), Required, MaxLength(FilterLimit)]
        public string RegexString { get; set; } = "";

        [Column("action")]
        public Action OnHitAction { get; set; } = Action.Delete;

        [NotMapped]
        public Regex Regex => this.RegexLazy ??= this.RegexString.ToRegex(this.Options);

        [NotMapped]
        public Regex? RegexLazy { get; set; }

        [NotMapped]
        public RegexOptions Options { get; set; } = RegexOptions.IgnoreCase;


        public virtual GuildConfig GuildConfig { get; set; } = null!;


        public enum Action : byte
        {
            Delete = 0,
            SanitizeOnly = 1,
            DeleteAndTemporaryMute = 2,
            DeleteAndPermanentMute = 3,
            DeleteAndKick = 4,
            DeleteAndTemporaryBan = 5,
            DeleteAndPermanentBan = 6,
        }
    }
}
