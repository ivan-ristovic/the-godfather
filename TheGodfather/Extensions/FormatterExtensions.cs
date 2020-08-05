using System;

namespace TheGodfather.Extensions
{
    public static class FormatterExt
    {
        // TODO remove when implemented into D#+
        [Obsolete]
        public static string Spoiler(string str)
            => $"||{str}||";
    }
}
