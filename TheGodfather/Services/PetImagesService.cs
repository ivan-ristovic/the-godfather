#region USING_DIRECTIVES
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Common;

using DSharpPlus;
#endregion;

namespace TheGodfather.Services
{
    public class PetImagesService : HttpService
    {
        public static async Task<string> GetRandomCatImageAsync()
        {
            try {
                var data = await _http.GetStringAsync("http://aws.random.cat/meow")
                    .ConfigureAwait(false);
                var jsondata = JsonConvert.DeserializeObject<DeserializedData>(data);
                return jsondata.URL;
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Warning, e);
                return null;
            }
        }

        public static async Task<string> GetRandomDogImageAsync()
        {
            try {
                var data = await _http.GetStringAsync("https://random.dog/woof")
                    .ConfigureAwait(false);
                return "https://random.dog/" + data;
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Warning, e);
                return null;
            }
        }


        private sealed class DeserializedData
        {
            [JsonProperty("file")]
            public string URL { get; set; }
        }
    }
}
