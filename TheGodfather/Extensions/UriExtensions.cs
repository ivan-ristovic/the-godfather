using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TheGodfather.Services;

namespace TheGodfather.Extensions
{
    internal static class UriExtensions
    {
        public static async Task<bool> ContentTypeHeaderStartsWith(this Uri uri, string contentType)
        {
            try {
                (_, HttpContentHeaders headers)  = await HttpService.HeadAsync(uri).ConfigureAwait(false);
                return headers.ContentType.MediaType.StartsWith(contentType, StringComparison.InvariantCultureIgnoreCase);
            } catch {

            }
            return false;
        }

        public static Task<bool> ContentTypeHeaderIsImageAsync(this Uri uri)
            => ContentTypeHeaderStartsWith(uri, "image/");
    }
}
