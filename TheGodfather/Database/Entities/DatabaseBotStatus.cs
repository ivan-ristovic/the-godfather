using DSharpPlus.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Entities
{
    [Table("bot_statuses")]
    public class DatabaseBotStatus
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("status"), Required]
        public string Status { get; set; }

        [Column("activity_type"), Required]
        public ActivityType Activity { get; set; }
    }
}
