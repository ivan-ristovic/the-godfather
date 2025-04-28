using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TheGodfather.Services;

public static class HttpService
{
    private static readonly HttpClient _client;

    static HttpService()
    {
        var handler = new HttpClientHandler { AllowAutoRedirect = false };
        _client = new HttpClient(handler, true) {
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
        await using Stream stream = await GetStreamAsync(requestUri);
        var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public static Task<HttpResponseMessage> PostJsonAsync<TValue>(string requestUri, TValue obj, string? bearer)
    {
        var http = new HttpClient();
        if (bearer is not null)
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        return http.PostAsJsonAsync(new Uri(requestUri).ToString(), obj);
    }

}