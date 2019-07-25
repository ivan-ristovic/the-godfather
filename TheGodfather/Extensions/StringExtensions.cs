#region USING_DIRECTIVES
using System;
using System.Text.RegularExpressions;
#endregion

namespace TheGodfather.Extensions
{
    internal static class StringExtensions
    {
        public static bool IsValidRegexString(this string pattern)
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
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
                return m;

            if (m == 0)
                return n;

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
                ;

            for (int j = 0; j <= m; d[0, j] = j++)
                ;

            // Step 3
            for (int i = 1; i <= n; i++) {
                //Step 4
                for (int j = 1; j <= m; j++) {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

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
            if (string.IsNullOrWhiteSpace(pattern) || !IsValidRegexString(pattern))
                return false;

            result = ToRegex(pattern, escape: false);
            return true;
        }

        public static Regex ToRegex(this string pattern, bool escape = false)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException("Provided string was empty", nameof(pattern));

            string rstr = pattern.ToLowerInvariant();
            return new Regex(escape ? Regex.Escape(pattern) : rstr, RegexOptions.IgnoreCase);
        }
    }
}
