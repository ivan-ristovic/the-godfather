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
        public Action OnHitAction { get; set; } = Action.DeleteMessage;

        [NotMapped]
        public Regex Regex => this.RegexLazy ??= this.RegexString.ToRegex(this.Options);

        [NotMapped]
        public Regex? RegexLazy { get; set; }

        [NotMapped]
        public RegexOptions Options { get; set; } = RegexOptions.IgnoreCase;


        public virtual GuildConfig GuildConfig { get; set; } = null!;


        public enum Action : byte
        {
            DeleteMessage = 0,
            Sanitize = 1,
            TemporaryMute = 2,
            PermanentMute = 3,
            Kick = 4,
            TemporaryBan = 5,
            PermanentBan = 6,
        }
    }
}
