using System;

namespace TheGodfather.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToDurationString(this TimeSpan ts) 
            => ts.Days > 0 ? $@"{ts:%d} d, {ts:hh\:mm\:ss}" : ts.ToString(@"hh\:mm\:ss");
    }
}
