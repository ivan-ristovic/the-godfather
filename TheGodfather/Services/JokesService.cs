#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using TheGodfather.Common;

using DSharpPlus;
#endregion

namespace TheGodfather.Services
{
    public class JokesService : TheGodfatherHttpService
    {
        public static async Task<string> GetRandomJokeAsync()
        {
            var res = await GetStringResponseAsync("https://icanhazdadjoke.com/")
                .ConfigureAwait(false);
            return res;
        }

        public static async Task<IReadOnlyList<string>> SearchForJokesAsync(string query)
        {
            try {
                var res = await GetStringResponseAsync($"https://icanhazdadjoke.com/search?term={ WebUtility.UrlEncode(query) }")
                    .ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(res))
                    return Enumerable.Empty<string>().ToList().AsReadOnly();
                return res.Split('\n').ToList().AsReadOnly();
            } catch (Exception e) {
                // LogProvider.LogProvider.LogException(LogLevel.Debug, e);
                return null;
            }
        }

        public static async Task<string> GetYoMommaJokeAsync()
        {
            try {
                string data = await _http.GetStringAsync("http://api.yomomma.info/")
                    .ConfigureAwait(false);
                return JObject.Parse(data)["joke"].ToString();
            } catch (Exception e) {
                // LogProvider.LogProvider.LogException(LogLevel.Debug, e);
                return null;
            }
        }

        private static async Task<string> GetStringResponseAsync(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Accept = "text/plain";

            string data = null;
            using (var response = await request.GetResponseAsync().ConfigureAwait(false))
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream)) {
                data = await reader.ReadToEndAsync()
                    .ConfigureAwait(false);
            }

            return data;
        }
    }
}
