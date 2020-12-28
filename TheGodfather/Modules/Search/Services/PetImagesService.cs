using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;
using TheGodfather.Services;

namespace TheGodfather.Modules.Search.Services
{
    public class PetImagesService : TheGodfatherHttpService
    {
        private const string CatUrl = "http://aws.random.cat/meow";
        private const string DogUrl = "https://random.dog/";

        public override bool IsDisabled => false;


        public static async Task<string?> GetRandomCatImageAsync()
        {
            try {
                string data = await _http.GetStringAsync(CatUrl).ConfigureAwait(false);
                return JObject.Parse(data)["file"]?.ToString();
            } catch (Exception e) {
                Log.Error(e, "Failed to retrieve random cat image");
                return null;
            }
        }

        public static async Task<string?> GetRandomDogImageAsync()
        {
            try {
                string data = await _http.GetStringAsync($"{DogUrl}/woof").ConfigureAwait(false);
                return DogUrl + data;
            } catch (Exception e) {
                Log.Error(e, "Failed to retrieve random dog image");
                return null;
            }
        }
    }
}
