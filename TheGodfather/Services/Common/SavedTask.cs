using System;

namespace TheGodfather.Services.Common
{
    public enum SavedTaskType
    {
        SendMessage = 0,
        Unban = 1
    }

    public static class SavedTaskTypeExtensions
    {
        public static string ToTypeString(this SavedTaskType type)
        {
            switch (type) {
                case SavedTaskType.SendMessage: return "Send Message";
                case SavedTaskType.Unban: return "Unban";
                default: return "Unknown";
            }
        }
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
            => ExecutionTime - DateTime.UtcNow;

        public bool IsExecutionTimeReached
            => TimeUntilExecution.CompareTo(TimeSpan.Zero) < 0;
    }
}
