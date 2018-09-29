using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("blocked_channels")]
    public partial class DatabaseBlockedChannels
    {
        public long Cid { get; set; }
        public string Reason { get; set; }
    }
}
