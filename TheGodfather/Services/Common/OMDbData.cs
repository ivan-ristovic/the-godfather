#region USING_DIRECTIVES
using System.Collections.Generic;
using Newtonsoft.Json;
#endregion

namespace TheGodfather.Services.Common
{
    public enum OMDbQueryType
    {
        Id,
        Title
    }

    public static class SearchTypeExtensions
    {
        public static string ToApiString(this OMDbQueryType type)
        {
            switch (type) {
                case OMDbQueryType.Id: return "i";
                case OMDbQueryType.Title: return "t";
                default: return "";
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

        [JsonProperty("Rated")]
        public string Rated { get; set; }

        [JsonProperty("Released")]
        public string ReleaseDate { get; set; }

        [JsonProperty("Runtime")]
        public string Duration { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Genre")]
        public string Genre { get; set; }

        [JsonProperty("Director")]
        public string Director { get; set; }

        [JsonProperty("Writer")]
        public string Writer { get; set; }

        [JsonProperty("Actors")]
        public string Actors { get; set; }

        [JsonProperty("Plot")]
        public string Plot { get; set; }

        [JsonProperty("Poster")]
        public string Poster { get; set; }

        [JsonProperty("imdbID")]
        public string IMDbId { get; set; }

        [JsonProperty("imdbRating")]
        public string IMDbRating { get; set; }

        [JsonProperty("imdbVotes")]
        public string IMDbVotes { get; set; }

        [JsonProperty("Response")]
        public bool Success { get; set; }
    }
}
