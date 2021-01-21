using System.Linq;
using DSharpPlus.Entities;

namespace TheGodfather.Extensions
{
    internal static class DiscordPresenceExtensions
    {
        public static bool IsDifferentThan(this DiscordRichPresence? @this, DiscordRichPresence? other)
        {
            if (@this is null || other is null)
                return !ReferenceEquals(@this, other);

            if (@this.Application?.Id != other.Application?.Id)
                return true;

            if (@this.CurrentPartySize != other.CurrentPartySize || @this.MaximumPartySize != other.MaximumPartySize)
                return true;

            if (@this.Details != other.Details)
                return true;

            if (@this.StartTimestamp != other.EndTimestamp || @this.EndTimestamp != other.EndTimestamp)
                return true;

            if (@this.Instance != other.Instance)
                return true;

            if (@this.JoinSecret != other.JoinSecret)
                return true;

            if (@this.LargeImage?.Id != other.LargeImage?.Id)
                return true;

            if (@this.MatchSecret != other.MatchSecret)
                return true;

            if (@this.PartyId != other.PartyId)
                return true;

            if (@this.SmallImage?.Id != other.SmallImage?.Id)
                return true;

            if (@this.SpectateSecret != other.SpectateSecret)
                return true;

            if (@this.State != other.State)
                return true;

            return false;
        }

        public static bool IsDifferentThan(this DiscordPresence? @this, DiscordPresence? other)
        {
            if (@this is null || other is null)
                return !ReferenceEquals(@this, other);

            if (@this.Activities is { } && @this.Activities is { }) {
                if (@this.Activities.Count != other.Activities.Count)
                    return true;
                foreach ((DiscordActivity First, DiscordActivity Second) in @this.Activities.Zip(other.Activities)) {
                    if (DiscordActivityExtensions.IsDifferentThan(First, Second))
                        return true;
                }
            } else if ((@this.Activities is null || other.Activities is null) && !ReferenceEquals(@this, other)) {
                return true;
            }

            if (DiscordActivityExtensions.IsDifferentThan(@this.Activity, other.Activity))
                return true;

            if (@this.Status != other.Status)
                return true;

            Serilog.Log.Debug("Client status: {@Before} => {@After}", @this.ClientStatus, other.ClientStatus);
            if (!ReferenceEquals(@this.ClientStatus, other.ClientStatus)
             || @this.ClientStatus?.Desktop != other.ClientStatus?.Desktop
             || @this.ClientStatus?.Mobile != other.ClientStatus?.Mobile
             || @this.ClientStatus?.Web != other.ClientStatus?.Web)
                return true;

            return false;
        }
    }
}
