using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("user_info")]
    public class DatabaseMessageCount
    {
        [Key]
        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId => (ulong)this.UserIdDb;

        [Column("message_count")]
        public int MessageCount { get; set; }
    }
}
