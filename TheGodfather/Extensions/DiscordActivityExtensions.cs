using System.Text;
using DSharpPlus.Entities;

namespace TheGodfather.Extensions;

internal static class DiscordActivityExtensions
{
    public static string ToDetailedString(this DiscordActivity activity)
    {
        var sb = new StringBuilder();
        if (activity.CustomStatus is not null) {
            if (activity.CustomStatus.Emoji is not null)
                sb.Append(activity.CustomStatus.Emoji.GetDiscordName()).Append(' ');
            sb.AppendLine(activity.CustomStatus.Name);
        } else {
            sb.Append(activity.ActivityType.Humanize()).Append(' ').AppendLine(activity.Name);
        }
        if (activity.StreamUrl is not null)
            sb.AppendLine(activity.StreamUrl);
        if (activity.RichPresence is not null)
            sb.AppendLine(activity.RichPresence.Details);
        return sb.ToString();
    }
}