using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("birthdays")]
    public class Birthday : IEquatable<Birthday>
    {
        [ForeignKey("GuildConfig")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        [Column("cid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ChannelIdDb { get; set; }
        [NotMapped]
        public ulong ChannelId { get => (ulong)this.ChannelIdDb; set => this.ChannelIdDb = (long)value; }

        [Column("date", TypeName = "date")]
        public DateTime Date { get; set; } = DateTime.Now.Date;

        [Column("last_update_year")]
        public int LastUpdateYear { get; set; } = DateTime.Now.Year;


        public virtual GuildConfig GuildConfig { get; set; } = null!;


        public bool Equals(Birthday? other)
            => other is { } && this.GuildId == other.GuildId && this.ChannelId == other.ChannelId && this.UserId == other.UserId;

        public override bool Equals(object? other)
            => this.Equals(other as Birthday);

        public override int GetHashCode()
            => (this.GuildId, this.ChannelId, this.UserId).GetHashCode();
    }
}
