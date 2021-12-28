using System.Net;
using Newtonsoft.Json;
using TheGodfather.Modules.Search.Common;

namespace TheGodfather.Modules.Search.Services;

public class OMDbService : TheGodfatherHttpService
{
    private const string Endpoint = "http://www.omdbapi.com/";
    private const string ImdbUrl = "http://www.imdb.com/";

    public override bool IsDisabled => string.IsNullOrWhiteSpace(this.key);

    private readonly string? key;


    public OMDbService(BotConfigService cfg)
    {
        this.key = cfg.CurrentConfiguration.OMDbKey;
    }


    public async Task<IReadOnlyList<MovieInfo>?> SearchAsync(string query)
    {
        if (this.IsDisabled)
            return null;

        string url = $"{Endpoint}?apikey={this.key}&s={WebUtility.UrlEncode(query)}";

        try {
            string response = await _http.GetStringAsync(url).ConfigureAwait(false);
            OMDbResponse data = JsonConvert.DeserializeObject<OMDbResponse>(response) ?? throw new JsonSerializationException();
            IReadOnlyList<MovieInfo>? results = data.Success ? data.Results?.AsReadOnly() : null;
            if (results is null || !results.Any())
                return null;

            return results
                .ToList()
                .AsReadOnly();
        } catch (Exception e) {
            Log.Error(e, "Failed to fetch OMDb data");
            return null;
        }
    }

    public async Task<MovieInfo?> SearchSingleAsync(OMDbQueryType type, string query)
    {
        if (this.IsDisabled)
            return null;

        try {
            string url = $"{Endpoint}?apikey={this.key}&{type.ToApiString()}={WebUtility.UrlEncode(query)}";
            string response = await _http.GetStringAsync(url).ConfigureAwait(false);
            MovieInfo data = JsonConvert.DeserializeObject<MovieInfo>(response) ?? throw new JsonSerializationException();
            return data.Success ? data : null;
        } catch (Exception e) {
            Log.Error(e, "Failed to fetch OMDb data");
            return null;
        }
    }

    public string GetUrl(string id) => $"{ImdbUrl}title/{id}";
}