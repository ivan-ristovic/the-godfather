#region USING_DIRECTIVES
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheGodfather.Common;
#endregion

namespace TheGodfather.Database.Entities
{
    [Table("reminders")]
    public class DatabaseReminder
    {

        public static DatabaseReminder FromSavedTaskInfo(SavedTaskInfo tinfo)
        {
            var smti = tinfo as SendMessageTaskInfo;
            if (smti is null)
                return null;

            var dbr = new DatabaseReminder() {
                ChannelId = smti.ChannelId,
                ExecutionTime = tinfo.ExecutionTime.UtcDateTime,
                IsRepeating = smti.IsRepeating,
                Message = smti.Message,
                RepeatIntervalDb = smti.RepeatingInterval,
                UserId = smti.InitiatorId
            };

            return dbr;
        }


        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("uid")]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        [Column("cid")]
        public long? ChannelIdDb { get; set; }
        [NotMapped]
        public ulong ChannelId { get => (ulong)this.ChannelIdDb.GetValueOrDefault(); set => this.ChannelIdDb = (long)value; }

        [Column("message"), Required, MaxLength(256)]
        public string Message { get; set; }

        [Column("execution_time", TypeName = "timestamptz")]
        public DateTimeOffset ExecutionTime { get; set; }

        [Column("is_repeating")]
        public bool IsRepeating { get; set; } = false;

        [Column("repeat_interval", TypeName = "interval")]
        public TimeSpan? RepeatIntervalDb { get; set; }
        [NotMapped]
        public TimeSpan RepeatInterval => this.RepeatIntervalDb ?? TimeSpan.FromMilliseconds(-1);
    }
}
