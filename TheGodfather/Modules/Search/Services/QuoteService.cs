using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Search.Services
{
    public class QuoteService : TheGodfatherHttpService
    {
        private const string QuoteUrl = "https://quotes.rest/qod.json";
        private const string RandomQuoteUrl = "https://quotesondesign.com/wp-json/wp/v2/posts/?orderby=rand&per_page=1";

        private static readonly Regex _tagMatcher = new Regex("<.*?>", RegexOptions.Compiled);
        private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());


        public override bool IsDisabled => false;


        public static async Task<Quote?> GetQuoteOfTheDayAsync(string? category = null)
        {
            if (_cache.TryGetValue("qotd", out Quote quote))
                return quote;

            try {
                string? response = string.IsNullOrWhiteSpace(category)
                    ? await _http.GetStringAsync(QuoteUrl).ConfigureAwait(false)
                    : await _http.GetStringAsync($"{QuoteUrl}?category={WebUtility.UrlEncode(category)}").ConfigureAwait(false);
                QuoteApiResponse data = JsonConvert.DeserializeObject<QuoteApiResponse>(response);
                Quote? q = data?.Contents?.Quotes?.FirstOrDefault();
                if (q is { })
                    _cache.Set("qotd", q, TimeSpan.FromMinutes(10));
                return q;
            } catch (Exception e) {
                Log.Error(e, "Failed to retrieve quote of the day in category {Category}", category ?? "(not set)");
                return null;
            }
        }

        public static async Task<string?> GetRandomQuoteAsync()
        {
            try {
                string response = await _http.GetStringAsync(RandomQuoteUrl).ConfigureAwait(false);
                string? data = JArray.Parse(response)?.FirstOrDefault()?["content"]?["rendered"]?.ToString();
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
}
