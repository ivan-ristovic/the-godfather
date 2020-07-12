using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using TheGodfather.Extensions;

namespace TheGodfather.Database.Models
{
    [Table("filters")]
    public class Filter
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("GuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("trigger"), Required, MaxLength(128)]
        public string TriggerString { get; set; } = "";

        [NotMapped]
        public Regex Trigger => this.triggerLazy ??= this.TriggerString.ToRegex();

        [NotMapped]
        private Regex? triggerLazy = null;


        public virtual GuildConfig GuildConfig { get; set; } = null!;
    }
}
