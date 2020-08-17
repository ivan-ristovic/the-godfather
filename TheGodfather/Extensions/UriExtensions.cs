using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TheGodfather.Services;

namespace TheGodfather.Extensions
{
    internal static class UriExtensions
    {
        public static bool ContentTypeHeaderStartsWith(this HttpContentHeaders headers, string contentType)
            => headers.ContentType.MediaType.StartsWith(contentType, StringComparison.InvariantCultureIgnoreCase);

        public static bool ContentTypeHeaderIsImage(this HttpContentHeaders headers)
            => headers.ContentTypeHeaderStartsWith("image/");

        public static async Task<bool> ContentTypeHeaderStartsWith(this Uri uri, string contentType)
        {
            try {
                (_, HttpContentHeaders headers)  = await HttpService.HeadAsync(uri).ConfigureAwait(false);
                return headers.ContentTypeHeaderStartsWith(contentType);
            } catch {

            }
            return false;
        }

        public static Task<bool> ContentTypeHeaderIsImageAsync(this Uri uri)
            => uri.ContentTypeHeaderStartsWith("image/");
    }
}
