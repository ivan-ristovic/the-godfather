using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("blocked_users")]
    public partial class DatabaseBlockedUsers
    {
        public long Uid { get; set; }
        public string Reason { get; set; }
    }
}
