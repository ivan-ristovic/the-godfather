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
    public static readonly LinkfilterMatcher UrlShortenerMatcher = new(UrlShorteners.Keys);


    private static List<string> GetWebsiteUrlsFromJson(string path)
    {
        path = ContentPathResolver.Resolve(path);
        try {
            using FileStream fs = new FileInfo(path).OpenRead();
            using var sr = new StreamReader(fs, new UTF8Encoding(false));
            return JsonConvert.DeserializeObject<List<string>>(sr.ReadToEnd()) ?? throw new JsonSerializationException();
        } catch (Exception e) {
            Log.Error(e, "Failed to load website URLs from: {Path}", path);
            return new List<string> { "invalid.localhost" };
        }
    }

    private static Dictionary<string, string> GetUrlShorteners()
    {
        const string path = "Resources/linkfilter/url_shorteners.json";
        try {
            using FileStream fs = new FileInfo(ContentPathResolver.Resolve(path)).OpenRead();
            using var sr = new StreamReader(fs, new UTF8Encoding(false));
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd()) ?? throw new JsonSerializationException();
        } catch (Exception e) {
            Log.Error(e, "Failed to load URL shorteners");
            return new Dictionary<string, string> { ["invalid.localhost"] = "invalid" };
        }
    }

    [GeneratedRegex(@"discord\.(gg|app|com)/(invite/)?([\w\-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex DiscordInviteRegex();
}
