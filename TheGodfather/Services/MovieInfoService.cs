#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using DSharpPlus;
#endregion

namespace TheGodfather.Services
{
    public class MovieInfoService : HttpService, IGodfatherService
    {
        private string _key;


        public MovieInfoService(string key)
        {
            _key = key;
        }


        public async Task<IReadOnlyList<MovieInfo>> SearchAsync(string query)
        {
            try {
                var response = await _http.GetStringAsync($"http://www.omdbapi.com/?apikey={ _key }&s={ query }")
                    .ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<OMDbResponse>(response);
                if (data.Success)
                    return data.Results.AsReadOnly();
                else
                    return null;
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return null;
            }
        }
    }

    public class OMDbResponse
    {
        [JsonProperty("Search")]
        public List<MovieInfo> Results { get; set; }
        
        [JsonProperty("totalResults")]
        public int NumberOfResults { get; set; }

        [JsonProperty("Response")]
        public bool Success { get; set; }
    }

    public class MovieInfo
    {
        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("Year")]
        public string Year { get; set; }

        [JsonProperty("imdbID")]
        public string IMDbId { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Poster")]
        public string Poster { get; set; }
    }
}
