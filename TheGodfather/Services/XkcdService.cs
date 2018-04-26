#region USING_DIRECTIVES
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.Entities;
#endregion;

namespace TheGodfather.Services
{
    public class XkcdService : TheGodfatherHttpService
    {
        public static string XkcdUrl = "https://xkcd.com";


        public static async Task<XkcdComic> GetLatestComicAsync()
        {
            try {
                var res = await _http.GetStringAsync($"{XkcdUrl}/info.0.json")
                    .ConfigureAwait(false);
                var comic = JsonConvert.DeserializeObject<XkcdComic>(res);
                return comic;
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return null;
            }
        }

    }
}
