namespace TheGodfather.Modules.Administration.Common
{
    public class LinkfilterSettings
    {
        public bool Enabled { get; set; } = false;
        public bool BlockBooterWebsites { get; set; } = true;
        public bool BlockDiscordInvites { get; set; } = false;
        public bool BlockDisturbingWebsites { get; set; } = true;
        public bool BlockIpLoggingWebsites { get; set; } = true;
        public bool BlockUrlShorteners { get; set; } = true;
    }
}
