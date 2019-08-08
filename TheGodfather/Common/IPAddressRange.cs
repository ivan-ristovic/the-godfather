using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TheGodfather.Common
{
    public sealed class IPAddressRange
    {
        private static readonly Regex _formatRegex = new Regex(@"^(?<range>(\d{1,3}\.){1,3}\d{1,3})(:(?<port>\d{4,5}))?$", RegexOptions.Compiled);

        public static bool TryParse(string str, out IPAddressRange res)
        {
            res = null;

            Match m = _formatRegex.Match(str);
            if (!m.Success)
                return false;

            string[] quartets = m.Groups["range"].Value.Split(".", StringSplitOptions.RemoveEmptyEntries);
            if (quartets.First().All(c => c == '0'))
                return false;
            foreach (string quartet in quartets) {
                if (!byte.TryParse(quartet, out byte v))
                    return false;
            }
            if (m.Groups["port"].Success && (!ushort.TryParse(m.Groups["port"].Value, out ushort port) || port < 1000))
                return false;

            res = new IPAddressRange {
                Content = str
            };

            return true;
        }


        public string Content { get; private set; }
    }
}
