using System.Text;

namespace TheGodfather.Modules.Administration.Common;

public static class LinkfilterSettingsExtensions
{
    public static string ToEmbedFieldString(this LinkfilterSettings settings, ulong gid, LocalizationService lcs)
    {
        if (!settings.Enabled)
            return lcs.GetString(gid, TranslationKey.str_off);

        var sb = new StringBuilder(lcs.GetString(gid, TranslationKey.str_lf));
        sb.AppendLine();
        if (settings.BlockDiscordInvites)
            sb.AppendLine(lcs.GetString(gid, TranslationKey.str_lf_invite));
        if (settings.BlockBooterWebsites)
            sb.AppendLine(lcs.GetString(gid, TranslationKey.str_lf_ddos));
        if (settings.BlockDisturbingWebsites)
            sb.AppendLine(lcs.GetString(gid, TranslationKey.str_lf_gore));
        if (settings.BlockIpLoggingWebsites)
            sb.AppendLine(lcs.GetString(gid, TranslationKey.str_lf_ip));
        if (settings.BlockUrlShorteners)
            sb.AppendLine(lcs.GetString(gid, TranslationKey.str_lf_urlshort));

        return sb.ToString();
    }
}