using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TheGodfather.Modules.Search.Common;

namespace TheGodfather.Modules.Search.Services;

public class QuoteService : TheGodfatherHttpService
{
    private const string QuoteUrl = "https://quotes.rest/qod.json";
    private const string RandomQuoteUrl = "https://quotesondesign.com/wp-json/wp/v2/posts/?orderby=rand&per_page=1";

    private static readonly Regex _tagMatcher = new("<.*?>", RegexOptions.Compiled);
    private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromHours(6)});

    public override bool IsDisabled => this._key == null;

    private readonly string? _key;

    public QuoteService(BotConfigService bcs)
    {
        this._key = bcs.CurrentConfiguration.QuoteKey;
    }

    public async Task<Quote?> GetQuoteOfTheDayAsync()
    {
        if (_cache.TryGetValue("QOTD", out Quote? quote))
            return quote;

        try {
            string? response = await _http.GetStringAsync($"{QuoteUrl}?api_key={this._key}").ConfigureAwait(false);
            QuoteApiResponse data = JsonConvert.DeserializeObject<QuoteApiResponse>(response) ?? throw new JsonSerializationException();
            Quote? q = data?.Contents?.Quotes?.FirstOrDefault();
            if (q is not null)
                _cache.Set("QOTD", q,  DateTime.Today.AddDays(1).Subtract(DateTime.Now));
            return q;
        } catch (Exception e) {
            Log.Error(e, "Failed to retrieve quote of the day");
            return null;
        }
    }

    public static async Task<string?> GetRandomQuoteAsync()
    {
        try {
            string response = await _http.GetStringAsync(RandomQuoteUrl).ConfigureAwait(false);
            string? data = JArray.Parse(response).FirstOrDefault()?["content"]?["rendered"]?.ToString();
            if (data is null)
                throw new Exception("Failed to parse JSON");
            data = _tagMatcher.Replace(data, string.Empty);
            return WebUtility.HtmlDecode(data).Trim();
        } catch (Exception e) {
            Log.Error(e, "Failed to retrieve random quote");
            return null;
        }
    }
}