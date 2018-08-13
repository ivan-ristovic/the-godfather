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
        
        public bool RatelimitEnabled { get; set; }
        public PunishmentActionType RatelimitAction { get; set; }
        public short RatelimitSensitivity { get; set; }

        public bool AntifloodEnabled { get; set; }
        public PunishmentActionType AntifloodAction { get; set; }
        public short AntifloodCooldown { get; set; }
        public short AntifloodSensitivity { get; set; }

        public bool LoggingEnabled
            => this.LogChannelId != 0;


        public static CachedGuildConfig Default => new CachedGuildConfig {
            AntifloodAction = PunishmentActionType.PermanentBan,
            AntifloodCooldown = 10,
            AntifloodEnabled = false,
            AntifloodSensitivity = 3,
            BlockBooterWebsites = true,
            BlockDiscordInvites = false,
            BlockDisturbingWebsites = true,
            BlockIpLoggingWebsites = true,
            BlockUrlShorteners = true,
            Currency = null,
            LinkfilterEnabled = false,
            LogChannelId = 0,
            Prefix = null,
            RatelimitAction = PunishmentActionType.Mute,
            RatelimitEnabled = false,
            RatelimitSensitivity = 5,
            ReactionResponse = false,
            SuggestionsEnabled = false
        };
    }
}
