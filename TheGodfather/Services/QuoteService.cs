#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using TheGodfather.Services.Common;

using DSharpPlus;
#endregion;

namespace TheGodfather.Services
{
    public class QuoteService : TheGodfatherHttpService
    {
        private static readonly Regex _tagMatcher = new Regex("<.*?>", RegexOptions.Compiled);

        public static async Task<Quote> GetQuoteOfTheDayAsync(string category = null)
        {
            try {
                string response = null;
                if (string.IsNullOrWhiteSpace(category))
                    response = await _http.GetStringAsync("https://quotes.rest/qod.json").ConfigureAwait(false);
                else
                    response = await _http.GetStringAsync($"{"https://quotes.rest/qod.json"}?category={WebUtility.UrlEncode(category)}").ConfigureAwait(false);

                var data = JsonConvert.DeserializeObject<QuoteApiResponse>(response);
                return data?.Contents?.Quotes?.FirstOrDefault();
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return null;
            }
        }

        public static async Task<string> GetRandomQuoteAsync()
        {
            try {
                string response = await _http.GetStringAsync("http://quotesondesign.com/wp-json/posts?filter[orderby]=rand&filter[posts_per_page]=1")
                    .ConfigureAwait(false);
                var data = JArray.Parse(response).First["content"].ToString();
                data = _tagMatcher.Replace(data, String.Empty);
                return WebUtility.HtmlDecode(data).Trim();
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return null;
            }
        }
    }
}
