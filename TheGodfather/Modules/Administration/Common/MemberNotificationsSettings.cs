namespace TheGodfather.Modules.Administration.Common
{
    public class MemberUpdateMessagesSettings
    {
        public ulong WelcomeChannelId { get; set; }
        public ulong LeaveChannelId { get; set; }
        public string WelcomeMessage { get; set; }
        public string LeaveMessage { get; set; }
    }
}
