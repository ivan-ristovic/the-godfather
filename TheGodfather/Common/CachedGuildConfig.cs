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
        public string Prefix { get; set; }
        public bool ReactionResponse { get; set; }
        public bool SuggestionsEnabled { get; set; }

        public bool LoggingEnabled
            => this.LogChannelId != 0;


        public static CachedGuildConfig Default => new CachedGuildConfig {
            BlockBooterWebsites = true,
            BlockDiscordInvites = false,
            BlockDisturbingWebsites = true,
            BlockIpLoggingWebsites = true,
            BlockUrlShorteners = true,
            LinkfilterEnabled = false,
            LogChannelId = 0,
            Prefix = null,
            ReactionResponse = false,
            SuggestionsEnabled = false
        };
    }
}
