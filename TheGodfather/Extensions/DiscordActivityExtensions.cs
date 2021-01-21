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
            if (activity.CustomStatus is { }) {
                if (activity.CustomStatus.Emoji is { })
                    sb.Append(activity.CustomStatus.Emoji.GetDiscordName()).Append(' ');
                sb.AppendLine(activity.CustomStatus.Name);
            } else {
                sb.Append(activity.ActivityType.Humanize()).Append(' ').AppendLine(activity.Name);
            }
            if (activity.StreamUrl is { })
                sb.AppendLine(activity.StreamUrl);
            if (activity.RichPresence is { })
                sb.AppendLine(activity.RichPresence.Details);
            return sb.ToString();
        }

        public static bool IsDifferentThan(this DiscordActivity? @this, DiscordActivity? other)
        {
            if (@this is null || other is null)
                return !ReferenceEquals(@this, other);

            if (@this.ActivityType != other.ActivityType)
                return true;

            if (@this.CustomStatus?.Emoji?.Id != other.CustomStatus?.Emoji?.Id || @this.CustomStatus?.Name != other?.CustomStatus?.Name)
                return true;

            if (@this.Name != other?.Name)
                return true;

            if (@this.StreamUrl != other?.StreamUrl)
                return true;

            if (@this.RichPresence.IsDifferentThan(other.RichPresence))
                return true;

            return false;
        }
    }
}
