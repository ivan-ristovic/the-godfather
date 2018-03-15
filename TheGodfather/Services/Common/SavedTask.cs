using System;

namespace TheGodfather.Services.Common
{
    public enum SavedTaskType
    {
        SendMessage = 0,
        Unban = 1
    }

    public class SavedTask
    {
        public SavedTaskType Type { get; set; }
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }
        public DateTime ExecutionTime { get; set; }
        public string Comment { get; set; }

        public TimeSpan TimeUntilExecution
            => ExecutionTime.ToUniversalTime() - DateTime.UtcNow;

        public bool IsExecutionTimeReached
            => TimeUntilExecution.CompareTo(TimeSpan.Zero) < 0;
    }
}
