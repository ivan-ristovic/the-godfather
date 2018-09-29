#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("blocked_users")]
    public sealed class DatabaseBlockedUser
    {
        [Column("uid"), Key]
        public long UserId { get; set; }

        [Column("reason")]
        public string Reason { get; set; }
    }
}
