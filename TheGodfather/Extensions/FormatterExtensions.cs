using System.Text.RegularExpressions;

namespace TheGodfather.Extensions
{
    public static class FormatterExt
    {
        private static readonly Regex MdStripRegex = new Regex(@"([`\*_~\[\]\(\)""])", RegexOptions.ECMAScript);


        public static string Spoiler(string str)
            => $"||{str}||";

        public static string StripMarkdown(string str)
            => MdStripRegex.Replace(str, m => string.Empty);
    }
}
