#region USING_DIRECTIVES
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Services
{
    public class OMDbService : TheGodfatherHttpService
    {
        private readonly string url;


        public OMDbService(string key)
        {
            this.url = $"http://www.omdbapi.com/?apikey={ key }";
        }


        public async Task<IReadOnlyList<Page>> GetPaginatedResultsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query missing!", "query");
            
            string response = await _http.GetStringAsync($"{this.url}&s={query}").ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<OMDbResponse>(response);
            IReadOnlyList<MovieInfo> results = data.Success ? data.Results?.AsReadOnly() : null;
            if (results == null || !results.Any())
                return null;

            return results
                .Select(info => info.ToDiscordPage())
                .ToList()
                .AsReadOnly();
        }

        public async Task<MovieInfo> GetSingleResultAsync(OMDbQueryType type, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query missing!", "query");

            string response = await _http.GetStringAsync($"{this.url}&{type.ToApiString()}={query}").ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<MovieInfo>(response);
            return data.Success ? data : null;
        }
    }
}
