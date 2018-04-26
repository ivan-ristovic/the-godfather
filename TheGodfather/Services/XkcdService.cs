#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Services.Common;

using DSharpPlus;
#endregion;

namespace TheGodfather.Services
{
    public class XkcdService : TheGodfatherHttpService
    {
        public static string XkcdUrl { get; } = "https://xkcd.com";
        public static int ComicNum { get; } = 1900;


        public static async Task<XkcdComic> GetComicAsync(int? id = null)
        {
            if (id <= 0 || id > ComicNum)
                return null;

            try {
                string response;
                if (id.HasValue)
                    response = await _http.GetStringAsync($"{XkcdUrl}/{id}/info.0.json").ConfigureAwait(false);
                else
                    response = await _http.GetStringAsync($"{XkcdUrl}/info.0.json").ConfigureAwait(false);
                var comic = JsonConvert.DeserializeObject<XkcdComic>(response);
                return comic;
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return null;
            }
        }
    }
}
