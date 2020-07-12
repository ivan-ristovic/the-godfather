using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("cmd_rules")]
    public class CommandRule
    {
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("cid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ChannelIdDb { get; set; }
        [NotMapped]
        public ulong ChannelId { get => (ulong)this.ChannelIdDb; set => this.ChannelIdDb = (long)value; }

        [Column("command"), Required, MaxLength(32)]
        public string Command { get; set; } = null!;

        [Column("allow"), Required]
        public bool Allowed { get; set; }


        public virtual GuildConfig GuildConfig { get; set; } = null!;


        public bool IsMatchFor(ulong gid, ulong cid)
            => this.GuildId == gid && (this.ChannelId == cid || this.ChannelId == 0);
    }
}