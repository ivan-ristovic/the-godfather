using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TheGodfather.Modules.Search.Services;

public sealed class CatFactsService : TheGodfatherHttpService
{
    public override bool IsDisabled => false;

    private const string Endpoint = "https://catfact.ninja/fact";


    public async Task<string?> GetFactAsync()
    {
        try {
            string json = await _http.GetStringAsync(Endpoint);
            string fact = JObject.Parse(json)["fact"]?.ToString() ?? throw new JsonSerializationException();
            return fact;
        } catch (JsonSerializationException e) {
            Log.Error(e, "Failed to deserialize cat fact JSON");
        } catch (Exception e) {
            Log.Warning(e, "Failed to retrieve cat fact");
        }
         
        return null;
    }
}