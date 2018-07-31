#region USING_DIRECTIVES
using System;
#endregion

namespace TheGodfather.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToUtcTimestamp(this DateTime datetime)
            => $"At {datetime.ToUniversalTime().ToString()} UTC";

        public static string ToUtcTimestamp(this DateTimeOffset datetime)
            => $"At {datetime.ToUniversalTime().ToString()} UTC";
    }
}
