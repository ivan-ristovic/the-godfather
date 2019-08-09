using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DSharpPlus.Entities;

namespace TheGodfather.Database.Entities
{
    [Table("bot_statuses")]
    public class DatabaseBotStatus
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("status"), Required, MaxLength(64)]
        public string Status { get; set; }

        [Column("activity_type"), Required]
        public ActivityType Activity { get; set; } = ActivityType.Playing;
    }
}
