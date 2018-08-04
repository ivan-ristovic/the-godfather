#region USING_DIRECTIVES
using DSharpPlus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Services
{
    public class MemeGenService : TheGodfatherHttpService
    {
        private static readonly string _url = "http://memegen.link";
        private static readonly string _urlHttps = "https://memegen.link";
        private static readonly Regex _whitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly ImmutableDictionary<char, string> _replacements = new Dictionary<char, string>() {
            {'?', "~q"},
            {'%', "~p"},
            {'#', "~h"},
            {'/', "~s"},
            {' ', "-"},
            {'-', "--"},
            {'_', "__"},
            {'"', "''"}
        }.ToImmutableDictionary();


        public override bool IsDisabled() 
            => false;


        public static string GenerateMeme(string template, string topText, string bottomText)
            => $"{_url}/{Sanitize(template)}/{Sanitize(topText)}/{Sanitize(bottomText)}.jpg?font=impact";

        public static async Task<IReadOnlyList<string>> GetMemeTemplatesAsync()
        {
            string json = await _http.GetStringAsync($"{_urlHttps}/api/templates/").ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return data
                .OrderBy(kvp => kvp.Key)
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
                .Select(kvp => $"{Formatter.Bold(kvp.Key)} ({Formatter.MaskedUrl(Path.GetFileName(kvp.Value), new Uri(kvp.Value))})")
                .ToList()
                .AsReadOnly();
        }

        private static string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "_";

            input = _whitespaceRegex.Replace(input, " ");

            var sb = new StringBuilder();
            foreach (char c in input) {
                if (_replacements.TryGetValue(c, out string tmp))
                    sb.Append(tmp);
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
