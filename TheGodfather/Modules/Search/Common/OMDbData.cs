#nullable disable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TheGodfather.Modules.Search.Common
{
    public enum OMDbQueryType
    {
        Id,
        Title
    }

    public static class OMDbQueryTypeExtensions
    {
        public static string ToApiString(this OMDbQueryType type)
        {
            return type switch {
                OMDbQueryType.Id => "i",
                OMDbQueryType.Title => "t",
                _ => "",
            };
        }
    }

    public class OMDbResponse
    {
        [JsonProperty("totalResults")]
        public int NumberOfResults { get; set; }

        [JsonProperty("Search")]
        public List<MovieInfo> Results { get; set; }

        [JsonProperty("Response")]
        public bool Success { get; set; }
    }

    public class MovieInfo
    {
        [JsonProperty("Actors")]
        public string Actors { get; set; }

        [JsonProperty("Director")]
        public string Director { get; set; }

        [JsonProperty("Runtime")]
        public string Duration { get; set; }

        [JsonProperty("Genre")]
        public string Genre { get; set; }

        [JsonProperty("imdbID")]
        public string IMDbId { get; set; }

        [JsonProperty("imdbRating")]
        public string IMDbRating { get; set; }

        [JsonProperty("imdbVotes")]
        public string IMDbVotes { get; set; }

        [JsonProperty("Plot")]
        public string Plot { get; set; }

        [JsonProperty("Poster")]
        public string Poster { get; set; }

        [JsonProperty("Rated")]
        public string Rated { get; set; }

        [JsonProperty("Released")]
        public string ReleaseDate { get; set; }

        [JsonProperty("Response")]
        public bool Success { get; set; }

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Writer")]
        public string Writer { get; set; }

        [JsonProperty("Year")]
        public string Year { get; set; }
    }
}
