#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text.RegularExpressions;
#endregion

namespace TheGodfather.Extensions
{
    internal static class StringExtensions
    {
        public static bool IsValidRegex(this string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return false;

            try {
                Regex.Match("", pattern);
            } catch (ArgumentException) {
                return false;
            }

            return true;
        }

        // http://www.dotnetperls.com/levenshtein
        public static int LevenshteinDistance(this string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
                return m;

            if (m == 0)
                return n;

            // Step 2
            for (var i = 0; i <= n; d[i, 0] = i++)
                ;

            for (var j = 0; j <= m; d[0, j] = j++)
                ;

            // Step 3
            for (var i = 1; i <= n; i++) {
                //Step 4
                for (var j = 1; j <= m; j++) {
                    // Step 5
                    var cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            // Step 7
            return d[n, m];
        }

        public static bool TryParseRegex(this string pattern, out Regex result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(pattern) || !IsValidRegex(pattern))
                return false;

            string rstr = pattern.ToLowerInvariant();
            if (Char.IsLetterOrDigit(pattern.First()))
                rstr = $@"\b{rstr}";
            else
                rstr = $@"(^|\s){rstr}";
            if (Char.IsLetterOrDigit(pattern.Last()))
                rstr = $@"{rstr}\b";
            else
                rstr = $@"{rstr}($|\s)";

            result = new Regex(rstr, RegexOptions.IgnoreCase);

            return true;
        }
    }
}
