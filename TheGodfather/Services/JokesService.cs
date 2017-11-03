#region USING_DIRECTIVES
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace TheGodfather.Services
{
    public class JokesService
    {
        public JokesService()
        {

        }


        public async Task<string> GetRandomJokeAsync()
        {
            var res = await GetStringResponseAsync("https://icanhazdadjoke.com/")
                .ConfigureAwait(false);
            return res;
        }

        public async Task<string> SearchForJokesAsync(string query)
        {
            var res = await GetStringResponseAsync("https://icanhazdadjoke.com/search?term=" + query.Replace(' ', '+'))
                .ConfigureAwait(false);
            return res;
        }

        public async Task<string> GetYoMommaJokeAsync()
        {
            string data = null;
            using (WebClient wc = new WebClient()) {
                data = await wc.DownloadStringTaskAsync("http://api.yomomma.info/")
                    .ConfigureAwait(false);
            }

            try {
                return JObject.Parse(data)["joke"].ToString();
            } catch (JsonException) {
                throw new WebException();
            }
        }

        private async Task<string> GetStringResponseAsync(string url)
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
