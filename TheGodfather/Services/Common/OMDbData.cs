#region USING_DIRECTIVES
using System.Collections.Generic;
using Newtonsoft.Json;
#endregion

namespace TheGodfather.Services.Common
{
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
