using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGodfather.Database.Models
{
    public enum ScheduledTaskType : byte
    {
        Unknown = 0,
        SendMessage = 1,
        Unban = 2,
        Unmute = 3
    }

    public static class ScheduledTaskTypeExtensions
    {
        public static string ToTypeString(this ExemptedEntityType type)
            => Enum.GetName(typeof(ExemptedEntityType), type) ?? "Unknown";
    }

    public abstract class ScheduledTask
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("uid")]
        public long UserIdDb { get; set; }
        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        [Column("execution_time", TypeName = "timestamptz")]
        public DateTimeOffset ExecutionTime { get; set; }


        [NotMapped]
        public bool IsExecutionTimeReached
            => this.TimeUntilExecution.CompareTo(TimeSpan.Zero) < 0;

        [NotMapped]
        public abstract TimeSpan TimeUntilExecution { get; }
    }

    [Table("scheduled_tasks")]
    public class GuildTask : ScheduledTask
    {
        [ForeignKey("GuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }
        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("rid")]
        public long? RoleIdDb { get; set; }
        [NotMapped]
        public ulong RoleId { get => (ulong)this.RoleIdDb.GetValueOrDefault(); set => this.RoleIdDb = (long)value; }

        [Column("type")]
        public ScheduledTaskType Type { get; set; }


        [NotMapped]
        public override TimeSpan TimeUntilExecution
            => this.ExecutionTime - DateTimeOffset.Now;

        public virtual GuildConfig GuildConfig { get; set; } = null!;
    }

    [Table("reminders")]
    public class Reminder : ScheduledTask
    {
        [Column("cid")]
        public long? ChannelIdDb { get; set; }
        [NotMapped]
        public ulong ChannelId { get => (ulong)this.ChannelIdDb.GetValueOrDefault(); set => this.ChannelIdDb = (long)value; }

        [Column("message"), Required, MaxLength(256)]
        public string Message { get; set; } = null!;

        [Column("is_repeating")]
        public bool IsRepeating { get; set; } = false;

        [Column("repeat_interval", TypeName = "interval")]
        public TimeSpan? RepeatIntervalDb { get; set; }
        [NotMapped]
        public TimeSpan RepeatInterval => this.RepeatIntervalDb ?? TimeSpan.FromMilliseconds(-1);


        [NotMapped]
        public override TimeSpan TimeUntilExecution {
            get {
                DateTimeOffset now = DateTimeOffset.Now;
                if (this.ExecutionTime > now || !this.IsRepeating)
                    return this.ExecutionTime - now;
                TimeSpan diff = now - this.ExecutionTime;
                return TimeSpan.FromTicks(this.RepeatInterval.Ticks - diff.Ticks % this.RepeatInterval.Ticks);
            }
        }
    }
}
