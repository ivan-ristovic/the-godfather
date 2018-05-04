#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Services.Common;

using DSharpPlus;
using System.Net.Http;
using System.Net.Http.Headers;
#endregion;

namespace TheGodfather.Services
{
    public class QuoteService : TheGodfatherHttpService
    {
        public static async Task<Quote> GetQuoteOfTheDayAsync()
        {
            try {
                var request = new HttpRequestMessage();
                request.RequestUri = new Uri("https://quotes.rest/qod");
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _http.SendAsync(request)
                    .ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    return null;
                var json = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                var quoteResponse = JsonConvert.DeserializeObject<QuoteApiResponse>(json);
                return quoteResponse.Contents.Quotes.FirstOrDefault();
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return null;
            }
        }
    }
}
