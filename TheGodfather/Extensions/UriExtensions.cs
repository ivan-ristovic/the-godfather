using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TheGodfather.Extensions
{
    public static class UriExtensions
    {
        private static readonly HttpClient _http;
        private static readonly HttpClientHandler _handler;


        static UriExtensions()
        {
            _handler = new HttpClientHandler {
                AllowAutoRedirect = false
            };
            _http = new HttpClient(_handler, true);
        }


        public static async Task<bool> IsValidImageUriAsync(this Uri uri)
        {
            try {
                using (HttpResponseMessage response = await _http.GetAsync(uri).ConfigureAwait(false)) {
                    if (response.Content.Headers.ContentType.MediaType.StartsWith("image/"))
                        return true;
                }
            } catch {

            }

            return false;
        }
    }
}
