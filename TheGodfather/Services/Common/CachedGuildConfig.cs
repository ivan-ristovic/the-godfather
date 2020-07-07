using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Services.Common
{
    public sealed class CachedGuildConfig
    {
        public string Currency { get; set; } = "credits";
        public string Prefix { get; set; } = "!";
        public string Locale { get; set; } = "en-US";
        public ulong LogChannelId { get; set; } = 0;
        public bool SuggestionsEnabled { get; set; } = false;
        public bool ReactionResponse { get; set; } = false;
        public LinkfilterSettings LinkfilterSettings { get; set; } = new LinkfilterSettings();
        public AntispamSettings AntispamSettings { get; set; } = new AntispamSettings();
        public RatelimitSettings RatelimitSettings { get; set; } = new RatelimitSettings();

        public bool LoggingEnabled => this.LogChannelId != default;
    }
}
