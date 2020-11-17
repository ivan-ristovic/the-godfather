using System.Text;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class LinkfilterSettings
    {
        public bool Enabled { get; set; } = false;
        public bool BlockBooterWebsites { get; set; } = true;
        public bool BlockDiscordInvites { get; set; } = false;
        public bool BlockDisturbingWebsites { get; set; } = true;
        public bool BlockIpLoggingWebsites { get; set; } = true;
        public bool BlockUrlShorteners { get; set; } = true;


        public string ToEmbedFieldString(ulong gid, LocalizationService lcs)
        {
            if (!this.Enabled)
                return lcs.GetString(gid, "str-off");

            var sb = new StringBuilder(lcs.GetString(gid, "str-lf"));
            sb.AppendLine();
            if (this.BlockDiscordInvites)
                sb.AppendLine(lcs.GetString(gid, "str-lf-invite"));
            if (this.BlockBooterWebsites)
                sb.AppendLine(lcs.GetString(gid, "str-lf-ddos"));
            if (this.BlockDisturbingWebsites)
                sb.AppendLine(lcs.GetString(gid, "str-lf-gore"));
            if (this.BlockIpLoggingWebsites)
                sb.AppendLine(lcs.GetString(gid, "str-lf-ip"));
            if (this.BlockUrlShorteners)
                sb.AppendLine(lcs.GetString(gid, "str-lf-urlshort"));

            return sb.ToString();
        }
    }
}
