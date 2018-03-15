using System;

namespace TheGodfather.Services.Common
{
    public enum SavedTaskType
    {
        Remind = 0,
        Unban = 1
    }

    public class SavedTask
    {
        public SavedTaskType Type { get; set; }
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
        public DateTime DispatchAt { get; set; }
        public string Comment { get; set; }
    }
}
