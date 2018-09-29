#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("blocked_channels")]
    public sealed class DatabaseBlockedChannel
    {
        [Column("cid"), Key]
        public int ChannelId { get; set; }

        [Column("reason")]
        public string Reason { get; set; }
    }
}
