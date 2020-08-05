using System.Text;
using DSharpPlus.Entities;
using Humanizer;

namespace TheGodfather.Extensions
{
    internal static class DiscordActivityExtensions
    {
        public static string ToDetailedString(this DiscordActivity activity)
        {
            var sb = new StringBuilder();
            if (activity.CustomStatus is { })
                sb.Append(activity.CustomStatus.Emoji.GetDiscordName()).Append(' ').AppendLine(activity.CustomStatus.Name);
            else
                sb.Append(activity.ActivityType.Humanize()).Append(' ').AppendLine(activity.Name);
            if (activity.StreamUrl is { })
                sb.AppendLine(activity.StreamUrl);
            if (activity.RichPresence is { })
                sb.AppendLine(activity.RichPresence.Details);
            return sb.ToString();
        }
    }
}
