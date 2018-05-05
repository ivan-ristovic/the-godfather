#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Services.Common;

using DSharpPlus;
#endregion;

namespace TheGodfather.Services
{
    public class QuoteService : TheGodfatherHttpService
    {
        private static readonly string _apiUrl = "https://quotes.rest/qod.json";


        public static async Task<Quote> GetQuoteOfTheDayAsync(string category = null)
        {
            try {
                string response = null;
                if (string.IsNullOrWhiteSpace(category))
                    response = await _http.GetStringAsync(_apiUrl).ConfigureAwait(false);
                else
                    response = await _http.GetStringAsync($"{_apiUrl}?category={WebUtility.UrlEncode(category)}").ConfigureAwait(false);

                var data = JsonConvert.DeserializeObject<QuoteApiResponse>(response);
                return data?.Contents?.Quotes?.FirstOrDefault();
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return null;
            }
        }
    }
}
