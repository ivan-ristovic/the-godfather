using System.Net;
using Newtonsoft.Json;

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
            } catch {
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
            } catch {
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
