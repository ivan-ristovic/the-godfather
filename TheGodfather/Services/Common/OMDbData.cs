#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
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

    public static class OMDbQueryTypeExtensions
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


        public DiscordEmbed ToDiscordEmbed()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = this.Title,
                Description = this.Plot,
                Url = $"http://www.imdb.com/title/{ this.IMDbId }",
                Color = DiscordColor.Yellow
            };

            if (!string.IsNullOrWhiteSpace(this.Type))
                emb.AddField("Type", this.Type, inline: true);
            if (!string.IsNullOrWhiteSpace(this.Year))
                emb.AddField("Air time", this.Year, inline: true);
            if (!string.IsNullOrWhiteSpace(this.IMDbId))
                emb.AddField("IMDb ID", this.IMDbId, inline: true);
            if (!string.IsNullOrWhiteSpace(this.Genre))
                emb.AddField("Genre", this.Genre, inline: true);
            if (!string.IsNullOrWhiteSpace(this.ReleaseDate))
                emb.AddField("Release date", this.ReleaseDate, inline: true);
            if (!string.IsNullOrWhiteSpace(this.Rated))
                emb.AddField("Rated", this.Rated, inline: true);
            if (!string.IsNullOrWhiteSpace(this.Duration))
                emb.AddField("Duration", this.Duration, inline: true);
            if (!string.IsNullOrWhiteSpace(this.Actors))
                emb.AddField("Actors", this.Actors, inline: true);
            if (!string.IsNullOrWhiteSpace(this.IMDbRating) && !string.IsNullOrWhiteSpace(this.IMDbVotes))
                emb.AddField("IMDb rating", $"{this.IMDbRating} out of {this.IMDbVotes} votes", inline: true);
            if (!string.IsNullOrWhiteSpace(this.Writer))
                emb.AddField("Writer", this.Writer, inline: true);
            if (!string.IsNullOrWhiteSpace(this.Director))
                emb.AddField("Director", this.Director, inline: true);
            if (!string.IsNullOrWhiteSpace(this.Poster) && this.Poster != "N/A")
                emb.WithThumbnailUrl(this.Poster);

            emb.WithFooter("Powered by OMDb.");

            return emb.Build();
        }
        
        public Page ToDiscordPage()
            => new Page() { Embed = this.ToDiscordEmbed() };
    }
}
