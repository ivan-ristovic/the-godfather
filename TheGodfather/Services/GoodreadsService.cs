#region USING_DIRECTIVES
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Services
{
    public class GoodreadsService : TheGodfatherHttpService
    {
        private static readonly string _url = "https://www.goodreads.com/search/index.xml";
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(GoodreadsResponse));
        private static readonly SemaphoreSlim _requestSemaphore = new SemaphoreSlim(1, 1);

        private readonly string key;


        public GoodreadsService(string key)
        {
            this.key = key;
        }


        public async Task<GoodreadsSearchInfo> SearchBooksAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query missing.", nameof(query));

            await _requestSemaphore.WaitAsync();
            try {
                using (Stream stream = await _http.GetStreamAsync($"{_url}?key={this.key}&q={WebUtility.UrlEncode(query)}").ConfigureAwait(false)) {
                    var response = (GoodreadsResponse)_serializer.Deserialize(stream);
                    return response.SearchInfo;
                }
            } finally {
                await Task.Delay(TimeSpan.FromSeconds(1));
                _requestSemaphore.Release();
            }
        }
    }
}
