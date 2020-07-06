using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    [Table("blocked_users")]
    public sealed class BlockedUser
    {
        [Key, Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        [Column("reason"), MaxLength(64)]
        public string? Reason { get; set; }
    }

    [Table("blocked_channels")]
    public sealed class BlockedChannel
    {
        [Key, Column("cid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ChannelIdDb { get; set; }
        [NotMapped]
        public ulong ChannelId { get => (ulong)this.ChannelIdDb; set => this.ChannelIdDb = (long)value; }

        [Column("reason"), MaxLength(64)]
        public string? Reason { get; set; }
    }
}
