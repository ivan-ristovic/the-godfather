using System.Net.Http.Headers;

namespace TheGodfather.Extensions;

internal static class UriExtensions
{
    public static bool ContentTypeHeaderStartsWith(this HttpContentHeaders headers, string contentType)
        => headers.ContentType?.MediaType?.StartsWith(contentType, StringComparison.InvariantCultureIgnoreCase) ?? false;

    public static bool ContentTypeHeaderIsImage(this HttpContentHeaders headers)
        => headers.ContentTypeHeaderStartsWith("image/");

    public static async Task<bool> TestHeadersAsync(this Uri uri, string contentType, long? contentLengthLimit)
    {
        try {
            (_, HttpContentHeaders headers) = await HttpService.HeadAsync(uri).ConfigureAwait(false);
            return headers.ContentTypeHeaderStartsWith(contentType) && headers.ContentLength <= contentLengthLimit;
        } catch {

        }
        return false;
    }

    public static Task<bool> ContentTypeHeaderIsImageAsync(this Uri uri, long? contentLengthLimit)
        => uri.TestHeadersAsync("image/", contentLengthLimit);
}