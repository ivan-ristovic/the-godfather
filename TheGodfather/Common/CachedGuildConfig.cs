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

        public LinkfilterSettings LinkfilterSettings { get; set; }
        
        public RatelimitSettings RatelimitSettings { get; set; }

        public bool LoggingEnabled
            => this.LogChannelId != 0;


        public static CachedGuildConfig Default => new CachedGuildConfig {
            Currency = null,
            LinkfilterSettings = new LinkfilterSettings(),
            LogChannelId = 0,
            Prefix = null,
            RatelimitSettings = new RatelimitSettings(),
            ReactionResponse = false,
            SuggestionsEnabled = false
        };
    }
}
