using System;
using System.Text.RegularExpressions;

namespace TheGodfather.Extensions
{
    internal static class StringExtensions
    {
        public static bool TryParseRegex(this string pattern, out Regex? regex, RegexOptions options = RegexOptions.IgnoreCase, bool escape = false)
        {
            regex = null;

            if (string.IsNullOrEmpty(pattern))
                return false;
            pattern = pattern.ToLowerInvariant();

            try {
                regex = new Regex(escape ? Regex.Escape(pattern) : pattern, options);
            } catch (ArgumentException) {
                return false;
            }

            return true;
        }

        // http://www.dotnetperls.com/levenshtein
        public static int LevenshteinDistanceTo(this string s, string t)
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

        public static Regex ToRegex(this string pattern, RegexOptions options = RegexOptions.IgnoreCase, bool escape = false)
        {
            return TryParseRegex(pattern, out Regex? regex, options, escape) && regex is { } 
                ? regex 
                : throw new ArgumentException($"Invalid regex string: {pattern}", nameof(pattern));
        }
    }
}
