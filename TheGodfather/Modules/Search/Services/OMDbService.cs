using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Search.Services
{
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
            string response = await _http.GetStringAsync(url).ConfigureAwait(false);
            OMDbResponse data = JsonConvert.DeserializeObject<OMDbResponse>(response);
            IReadOnlyList<MovieInfo>? results = data.Success ? data.Results?.AsReadOnly() : null;
            if (results is null || !results.Any())
                return null;

            return results
                .ToList()
                .AsReadOnly();
        }

        public async Task<MovieInfo?> SearchSingleAsync(OMDbQueryType type, string query)
        {
            if (this.IsDisabled)
                return null;

            string url = $"{Endpoint}?apikey={this.key}&{type.ToApiString()}={WebUtility.UrlEncode(query)}";
            string response = await _http.GetStringAsync(url).ConfigureAwait(false);
            MovieInfo data = JsonConvert.DeserializeObject<MovieInfo>(response);
            return data.Success ? data : null;
        }

        public string GetUrl(string id) => $"{ImdbUrl}title/{id}";
    }
}
