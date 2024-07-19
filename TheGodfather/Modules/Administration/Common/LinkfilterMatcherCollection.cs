using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TheGodfather.Modules.Administration.Common;

public static partial class LinkfilterMatcherCollection
{
    public static readonly Regex InviteRegex = DiscordInviteRegex();

    public static readonly LinkfilterMatcher IpLoggerMatcher = new(GetWebsiteUrlsFromJson("Resources/linkfilter/ip_loggers.json"));

    public static readonly LinkfilterMatcher BooterMatcher = new(GetWebsiteUrlsFromJson("Resources/linkfilter/booters.json"));

    public static readonly LinkfilterMatcher DisturbingWebsiteMatcher = new(GetWebsiteUrlsFromJson("Resources/linkfilter/disturbing_sites.json"));

    public static readonly ImmutableDictionary<string, string> UrlShorteners = GetUrlShorteners().ToImmutableDictionary();

    public static readonly LinkfilterMatcher UrlShortenerRegex = new(UrlShorteners.Keys);

    private static List<string> GetWebsiteUrlsFromJson(string path)
    {
        try {
            string json = "{}";
            var utf8 = new UTF8Encoding(false);
            var fi = new FileInfo(path);

            using (FileStream fs = fi.OpenRead())
            using (var sr = new StreamReader(fs, utf8)) {
                json = sr.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<List<string>>(json) ?? throw new JsonSerializationException();
        } catch (Exception e) {
            Log.Error(e, "Failed to load website URLs from: {Path}", path);
            return new List<string>();
        }
    }

    private static Dictionary<string, string> GetUrlShorteners()
    {
        try {
            string json = "{}";
            var utf8 = new UTF8Encoding(false);
            var fi = new FileInfo("Resources/linkfilter/url_shorteners.json");

            using (FileStream fs = fi.OpenRead())
            using (var sr = new StreamReader(fs, utf8)) {
                json = sr.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? throw new JsonSerializationException();
        } catch (Exception e) {
            Log.Error(e, "Failed to load URL shorteners");
            return new Dictionary<string, string>();
        }
    }

    [GeneratedRegex(@"discord\.(gg|app|com)\/invite\/([\w\-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex DiscordInviteRegex();
}