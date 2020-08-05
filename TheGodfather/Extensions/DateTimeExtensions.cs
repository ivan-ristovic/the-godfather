using System;

namespace TheGodfather.Extensions
{
    internal static class DateTimeExtensions
    {
        // TODO remove
        [Obsolete]
        public static string ToUtcTimestamp(this DateTime datetime)
            => $"At {datetime.ToUniversalTime().ToString()} UTC";

        // TODO remove
        [Obsolete]
        public static string ToUtcTimestamp(this DateTimeOffset datetime)
            => $"At {datetime.ToUniversalTime().ToString()} UTC";
    }
}
