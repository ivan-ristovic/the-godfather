#region USING_DIRECTIVES
using System;
using System.Net;
using Newtonsoft.Json;

using TheGodfather.Entities;

using DSharpPlus;
#endregion;

namespace TheGodfather.Services
{
    public static class PetImagesService
    {
        public static string RandomCatImage()
        {
            try {
                using (var wc = new WebClient()) {
                    var data = wc.DownloadString("http://random.cat/meow");
                    var jsondata = JsonConvert.DeserializeObject<DeserializedData>(data);
                    return jsondata.URL;
                }
            } catch (Exception e) {
                Logger.LogException(LogLevel.Debug, e);
                return null;
            }
        }

        public static string RandomDogImage()
        {
            try {
                using (var wc = new WebClient()) {
                    var data = wc.DownloadString("https://random.dog/woof");
                    return "https://random.dog/" + data;
                }
            } catch (Exception e) {
                Logger.LogException(LogLevel.Debug, e);
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
