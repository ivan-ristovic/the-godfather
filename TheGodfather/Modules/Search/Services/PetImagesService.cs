using Newtonsoft.Json.Linq;

namespace TheGodfather.Modules.Search.Services;

public class PetImagesService : TheGodfatherHttpService
{
    private const string CatUrl = "https://api.thecatapi.com/v1/images/search";
    private const string DogUrl = "https://dog.ceo/api/breeds/image/random";

    public override bool IsDisabled => false;


    public static async Task<string?> GetRandomCatImageAsync()
    {
        try {
            string data = await _http.GetStringAsync(CatUrl).ConfigureAwait(false);
            return JArray.Parse(data)[0]["url"]?.ToString();
        } catch (Exception e) {
            Log.Error(e, "Failed to retrieve random cat image");
            return null;
        }
    }

    public static async Task<string?> GetRandomDogImageAsync()
    {
        try {
            string data = await _http.GetStringAsync($"{DogUrl}/woof").ConfigureAwait(false);
            var json = JObject.Parse(data);
            if (json["status"]?.ToString() == "success" && json["message"] is not null) {
                return json["message"][0].ToString();
            } else {
                return null;
            }
        } catch (Exception e) {
            Log.Error(e, "Failed to retrieve random dog image");
            return null;
        }
    }
}