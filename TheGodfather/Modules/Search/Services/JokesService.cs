using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace TheGodfather.Modules.Search.Services;

public sealed class JokesService : TheGodfatherHttpService
{
    private const string JokesApi = "https://icanhazdadjoke.com";
    private const string YoMamaApi = "https://api.yomomma.info/";

    public override bool IsDisabled => false;


    public static Task<string?> GetRandomJokeAsync()
        => ReadResponseAsync(JokesApi);

    public static async Task<string?> GetRandomYoMommaJokeAsync()
    {
        string data = await _http.GetStringAsync(YoMamaApi).ConfigureAwait(false);
        return JObject.Parse(data)["joke"]?.ToString();
    }

    public static async Task<IReadOnlyList<string>?> SearchForJokesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException(@"Query missing", nameof(query));

        string? res = await ReadResponseAsync($"{JokesApi}/search?term={WebUtility.UrlEncode(query)}").ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(res) ? null : res.Split('\n').ToList().AsReadOnly();
    }


    private static async Task<string?> ReadResponseAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Accept", "text/plain");
        HttpResponseMessage response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}