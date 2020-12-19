using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TheGodfather.Services
{
    public static class HttpService
    {
        private static readonly HttpClient _client;
        private static readonly HttpClientHandler _handler;

        static HttpService()
        {
            _handler = new HttpClientHandler { AllowAutoRedirect = false };
            _client = new HttpClient(_handler, disposeHandler: true) {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        public static async Task<(HttpResponseHeaders, HttpContentHeaders)> HeadAsync(Uri requestUri)
        {
            using var m = new HttpRequestMessage(HttpMethod.Head, requestUri);
            HttpResponseMessage resp = await _client.SendAsync(m);
            return (resp.Headers, resp.Content.Headers);
        }

        public static Task<HttpResponseMessage> GetAsync(Uri requestUri)
            => _client.GetAsync(requestUri);

        public static Task<HttpResponseMessage> GetAsync(string requestUri)
            => _client.GetAsync(requestUri);

        public static Task<string> GetStringAsync(Uri requestUri)
            => _client.GetStringAsync(requestUri);

        public static Task<string> GetStringAsync(string requestUri)
            => _client.GetStringAsync(requestUri);

        public static Task<Stream> GetStreamAsync(Uri requestUri)
            => _client.GetStreamAsync(requestUri);

        public static Task<Stream> GetStreamAsync(string requestUri)
            => _client.GetStreamAsync(requestUri);

        public static Task<MemoryStream> GetMemoryStreamAsync(Uri requestUri)
            => GetMemoryStreamAsync(requestUri.ToString());

        public static async Task<MemoryStream> GetMemoryStreamAsync(string requestUri)
        {
            using Stream stream = await GetStreamAsync(requestUri);
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}
