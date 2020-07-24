using System.Globalization;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Services.Common
{
    public sealed class CachedGuildConfig
    {
        public string Currency { get; set; } = "credits";
        public string Prefix { get; set; } = "!";
        public string Locale { 
            get => this.locale;
            set {
                this.locale = value;
                this.culture = new CultureInfo(value);
            }
        }
        public string TimezoneId { get; set; } = "Central Europe Standard Time";
        public ulong LogChannelId { get; set; } = 0;
        public bool SuggestionsEnabled { get; set; } = false;
        public bool ReactionResponse { get; set; } = false;
        public LinkfilterSettings LinkfilterSettings { get; set; } = new LinkfilterSettings();
        public AntispamSettings AntispamSettings { get; set; } = new AntispamSettings();
        public RatelimitSettings RatelimitSettings { get; set; } = new RatelimitSettings();

        public bool LoggingEnabled => this.LogChannelId != default;
        public CultureInfo Culture {
            get {
                if (this.culture is null)
                    this.culture = new CultureInfo(this.Locale);
                return this.culture;
            }
        }


        private CultureInfo? culture;
        private string locale = "en-US";
    }
}
