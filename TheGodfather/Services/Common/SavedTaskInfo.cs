using System;

namespace TheGodfather.Services.Common
{
    public enum SavedTaskType : byte
    {
        SendMessage = 0,
        Unban = 1,
        Unmute = 2
    }

    public static class SavedTaskTypeExtensions
    {
        public static string ToTypeString(this SavedTaskType type)
        {
            switch (type) {
                case SavedTaskType.SendMessage: return "Send Message";
                case SavedTaskType.Unban: return "Unban";
                case SavedTaskType.Unmute: return "Unmute";
                default: return "Unknown";
            }
        }
    }

    public abstract class SavedTaskInfo
    {
        public DateTimeOffset ExecutionTime { get; set; }

        public virtual TimeSpan TimeUntilExecution 
            => this.ExecutionTime - DateTimeOffset.Now;

        public bool IsExecutionTimeReached
            => this.TimeUntilExecution.CompareTo(TimeSpan.Zero) < 0;
    }

    public sealed class SendMessageTaskInfo : SavedTaskInfo
    {
        public ulong ChannelId { get; set; }
        public string Message { get; set; }
        public ulong InitiatorId { get; set; }
        public bool IsRepeating { get; set; }
        public TimeSpan RepeatingInterval { get; set; }
        public override TimeSpan TimeUntilExecution 
        {
            get {
                DateTimeOffset now = DateTimeOffset.Now;
                if (this.ExecutionTime > now || !this.IsRepeating)
                    return this.ExecutionTime - now;
                TimeSpan diff = now - this.ExecutionTime;
                return TimeSpan.FromTicks(this.RepeatingInterval.Ticks - diff.Ticks % this.RepeatingInterval.Ticks);
            }
        }


        public SendMessageTaskInfo(ulong cid, ulong uid, string message, DateTimeOffset when, bool repeating = false, TimeSpan? interval = null)
        {
            this.ChannelId = cid;
            this.InitiatorId = uid;
            this.Message = message;
            this.ExecutionTime = when;
            this.IsRepeating = repeating;
            if (this.IsRepeating)
                this.RepeatingInterval = interval ?? TimeSpan.FromMilliseconds(-1);
            else
                this.RepeatingInterval = TimeSpan.FromMilliseconds(-1);
        }
    }

    public sealed class UnbanTaskInfo : SavedTaskInfo
    {
        public ulong GuildId { get; set; }
        public ulong UnbanId { get; set; }


        public UnbanTaskInfo(ulong gid, ulong uid, DateTimeOffset? when = null)
        {
            this.GuildId = gid;
            this.UnbanId = uid;
            if (when.HasValue)
                this.ExecutionTime = when.Value;
            else
                this.ExecutionTime = DateTimeOffset.Now + TimeSpan.FromDays(1);
        }
    }

    public sealed class UnmuteTaskInfo : SavedTaskInfo
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public ulong MuteRoleId { get; set; }


        public UnmuteTaskInfo(ulong gid, ulong uid, ulong rid, DateTimeOffset? when = null)
        {
            this.GuildId = gid;
            this.UserId = uid;
            this.MuteRoleId = rid;
            if (when.HasValue)
                this.ExecutionTime = when.Value;
            else
                this.ExecutionTime = DateTime.UtcNow + TimeSpan.FromHours(1);
        }
    }
}
