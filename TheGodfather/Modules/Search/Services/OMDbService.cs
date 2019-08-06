#region USING_DIRECTIVES
using DSharpPlus.Interactivity;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Search.Services
{
    public class OMDbService : TheGodfatherHttpService
    {
        private static readonly string _url = "http://www.omdbapi.com/";

        public override bool IsDisabled => string.IsNullOrWhiteSpace(this.key);

        private readonly string key;


        public OMDbService(BotConfigService cfg)
        {
            this.key = cfg.CurrentConfiguration.OMDbKey;
        }


        public async Task<IReadOnlyList<Page>> GetPaginatedResultsAsync(string query)
        {
            if (this.IsDisabled)
                return null;

            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query missing!", nameof(query));
            
            string response = await _http.GetStringAsync($"{_url}?apikey={this.key}&s={query}").ConfigureAwait(false);
            OMDbResponse data = JsonConvert.DeserializeObject<OMDbResponse>(response);
            IReadOnlyList<MovieInfo> results = data.Success ? data.Results?.AsReadOnly() : null;
            if (results is null || !results.Any())
                return null;

            return results
                .Select(info => info.ToDiscordPage())
                .ToList()
                .AsReadOnly();
        }

        public async Task<MovieInfo> GetSingleResultAsync(OMDbQueryType type, string query)
        {
            if (this.IsDisabled)
                return null;

            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query missing!", nameof(query));

            string response = await _http.GetStringAsync($"{_url}?apikey={this.key}&{type.ToApiString()}={query}").ConfigureAwait(false);
            MovieInfo data = JsonConvert.DeserializeObject<MovieInfo>(response);
            return data.Success ? data : null;
        }
    }
}
