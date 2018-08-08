#region USING_DIRECTIVES
using Newtonsoft.Json;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
#endregion

namespace TheGodfather.EventListeners.Common
{
    public static class LinkfilterMatcherCollection
    {
        public static readonly Regex InviteRegex = new Regex(@"discord(?:\.gg|app\.com\/invite)\/([\w\-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly LinkfilterMatcher IpLoggerMatcher = new LinkfilterMatcher(GetWebsiteUrlsFromJson("Resources/linkfilter/ip_loggers.json"));

        public static readonly LinkfilterMatcher BooterMatcher = new LinkfilterMatcher(GetWebsiteUrlsFromJson("Resources/linkfilter/booters.json"));

        public static readonly LinkfilterMatcher DisturbingWebsiteMatcher = new LinkfilterMatcher(GetWebsiteUrlsFromJson("Resources/linkfilter/disturbing_sites.json"));

        public static readonly ImmutableDictionary<string, string> UrlShorteners = GetUrlShorteners().ToImmutableDictionary();

        public static readonly LinkfilterMatcher UrlShortenerRegex = new LinkfilterMatcher(UrlShorteners.Keys);

        private static List<string> GetWebsiteUrlsFromJson(string path)
        {
            try {
                string json = "{}";
                var utf8 = new UTF8Encoding(false);
                var fi = new FileInfo(path);
                if (!fi.Exists)
                    throw new IOException($"File not found: {path}!");

                using (FileStream fs = fi.OpenRead())
                using (var sr = new StreamReader(fs, utf8))
                    json = sr.ReadToEnd();

                return JsonConvert.DeserializeObject<List<string>>(json);
            } catch {
                return new List<string>();
            }
        }

        private static Dictionary<string, string> GetUrlShorteners()
        {
            try {
                string json = "{}";
                var utf8 = new UTF8Encoding(false);
                var fi = new FileInfo("Resources/linkfilter/url_shorteners.json");
                if (!fi.Exists)
                    throw new IOException($"File not found: `Resources/linkfilter/url_shorteners.json`!");

                using (FileStream fs = fi.OpenRead())
                using (var sr = new StreamReader(fs, utf8))
                    json = sr.ReadToEnd();

                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            } catch {
                return new Dictionary<string, string>();
            }
        }
    }
}

