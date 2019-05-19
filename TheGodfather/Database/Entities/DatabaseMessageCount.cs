#region USING_DIRECTIVES
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

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
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        [Column("message_count")]
        public int MessageCountDb { get; set; }
        [NotMapped]
        public uint MessageCount { get => (uint)this.MessageCountDb; set => this.MessageCountDb = (int)value; }
    }
}
