namespace TheGodfather.Common
{
    public sealed class CachedGuildConfig
    {
        public bool BlockBooterWebsites { get; set; }
        public bool BlockDiscordInvites { get; set; }
        public bool BlockDisturbingWebsites { get; set; }
        public bool BlockIpLoggingWebsites { get; set; }
        public bool BlockUrlShorteners { get; set; }
        public bool LinkfilterEnabled { get; set; }
        public ulong LogChannelId { get; set; }
        public bool LoggingEnabled => LogChannelId != 0;
        public string Prefix { get; set; }
        public bool SuggestionsEnabled { get; set; }


        public static CachedGuildConfig Default
        {
            get {
                return new CachedGuildConfig {
                    BlockBooterWebsites = true,
                    BlockDiscordInvites = false,
                    BlockDisturbingWebsites = true,
                    BlockIpLoggingWebsites = true,
                    BlockUrlShorteners = true,
                    LinkfilterEnabled = false,
                    LogChannelId = 0,
                    Prefix = null,
                    SuggestionsEnabled = false
                };
            }
        }
    }
}
