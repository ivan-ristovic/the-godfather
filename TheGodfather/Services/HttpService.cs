using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TheGodfather.Services
{
    public static class HttpService
    {
        private static readonly HttpClient _http;
        private static readonly HttpClientHandler _handler;

        static HttpService()
        {
            _handler = new HttpClientHandler { AllowAutoRedirect = false };
            _http = new HttpClient(_handler, disposeHandler: true);
        }

        public static Task<HttpResponseMessage> GetAsync(Uri requestUri)
            => _http.GetAsync(requestUri);

        public static Task<HttpResponseMessage> GetAsync(string requestUri)
            => _http.GetAsync(requestUri);
        
        public static Task<string> GetStringAsync(Uri requestUri)
            => _http.GetStringAsync(requestUri);
        
        public static Task<string> GetStringAsync(string requestUri)
            => _http.GetStringAsync(requestUri);

        public static Task<Stream> GetStreamAsync(Uri requestUri)
            => _http.GetStreamAsync(requestUri);

        public static Task<Stream> GetStreamAsync(string requestUri)
            => _http.GetStreamAsync(requestUri);

        public static Task<MemoryStream> GetMemoryStreamAsync(Uri requestUri)
            => GetMemoryStreamAsync(requestUri.ToString());
     
        public static async Task<MemoryStream> GetMemoryStreamAsync(string requestUri)
        {
            using (Stream stream = await GetStreamAsync(requestUri)) {
                var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }
    }
}
