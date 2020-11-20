using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("blocked_users")]
    public class BlockedUser
    {
        public const int ReasonLimit = 64;

        [Key, Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        [Column("reason"), MaxLength(ReasonLimit)]
        public string? Reason { get; set; }
    }

    [Table("blocked_channels")]
    public class BlockedChannel
    {
        public const int ReasonLimit = 64;

        [Key, Column("cid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ChannelIdDb { get; set; }
        [NotMapped]
        public ulong ChannelId { get => (ulong)this.ChannelIdDb; set => this.ChannelIdDb = (long)value; }

        [Column("reason"), MaxLength(ReasonLimit)]
        public string? Reason { get; set; }
    }
}
