#region USING_DIRECTIVES
using TheGodfather.Modules.Administration.Common;
#endregion

namespace TheGodfather.Common
{
    public sealed class CachedGuildConfig
    {
        public string Currency { get; set; }
        public string Prefix { get; set; }
        public ulong LogChannelId { get; set; }

        public bool SuggestionsEnabled { get; set; }
        public bool ReactionResponse { get; set; }

        public bool LinkfilterEnabled { get; set; }
        public bool BlockBooterWebsites { get; set; }
        public bool BlockDiscordInvites { get; set; }
        public bool BlockDisturbingWebsites { get; set; }
        public bool BlockIpLoggingWebsites { get; set; }
        public bool BlockUrlShorteners { get; set; }
        
        public RatelimitSettings RatelimitSettings { get; set; }

        public bool LoggingEnabled
            => this.LogChannelId != 0;


        public static CachedGuildConfig Default => new CachedGuildConfig {
            BlockBooterWebsites = true,
            BlockDiscordInvites = false,
            BlockDisturbingWebsites = true,
            BlockIpLoggingWebsites = true,
            BlockUrlShorteners = true,
            Currency = null,
            LinkfilterEnabled = false,
            LogChannelId = 0,
            Prefix = null,
            RatelimitSettings = new RatelimitSettings(),
            ReactionResponse = false,
            SuggestionsEnabled = false
        };
    }
}
