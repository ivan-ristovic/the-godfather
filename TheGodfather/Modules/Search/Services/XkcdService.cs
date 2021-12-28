using Newtonsoft.Json;
using TheGodfather.Modules.Search.Common;

namespace TheGodfather.Modules.Search.Services;

public sealed class XkcdService : TheGodfatherHttpService
{
    public const string XkcdUrl = "https://xkcd.com";
    public override bool IsDisabled => false;
    public static int TotalComics { get; private set; } = 2400;


    public static string? CreateUrlForComic(int id) 
        => id < 1 || id > TotalComics ? null : $"{XkcdUrl}/{id}";

    public static Task<XkcdComic?> GetComicAsync(int? id = null)
    {
        if (id < 1 || id > TotalComics)
            return Task.FromResult<XkcdComic?>(null);

        try {
            return id.HasValue ? GetComicByIdAsync(id.Value) : GetLatestComicAsync();
        } catch {
            return Task.FromResult<XkcdComic?>(null);
        }
    }

    public static Task<XkcdComic?> GetRandomComicAsync()
        => GetComicByIdAsync(new SecureRandom().Next(TotalComics));


    private static Task<XkcdComic?> GetComicByIdAsync(int id)
        => GetComicAsync($"{XkcdUrl}/{id}/info.0.json");

    private static async Task<XkcdComic?> GetLatestComicAsync()
    {
        XkcdComic? comic = await GetComicAsync($"{XkcdUrl}/info.0.json");
        if (comic is { })
            TotalComics = comic.Id;
        return comic;
    }

    private static async Task<XkcdComic?> GetComicAsync(string requestUrl)
    {
        try {
            string response = await _http.GetStringAsync(requestUrl).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<XkcdComic>(response);
        } catch {
            return null;
        }
    }
}