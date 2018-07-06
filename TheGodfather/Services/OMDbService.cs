#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Common;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Services
{
    public class OMDbService : TheGodfatherHttpService
    {
        private readonly string _requestUrl;


        public OMDbService(string key)
        {
            _requestUrl = $"http://www.omdbapi.com/?apikey={ key }";
        }


        public async Task<IReadOnlyList<Page>> GetPaginatedResultsAsync(string q)
        {
            var results = await SearchQueryAsync(q)
                .ConfigureAwait(false);

            if (results == null || !results.Any())
                return null;

            return results.Select(info => EmbedInfoInPage(info)).ToList().AsReadOnly();
        }

        public async Task<Page> GetSingleResultAsync(OMDbQueryType type, string q)
        {
            var result = await SearchAsync(type, q)
                .ConfigureAwait(false);

            if (result == null)
                return null;

            return EmbedInfoInPage(result);
        }

        public DiscordEmbed EmbedInfo(MovieInfo info)
        {
            var emb = new DiscordEmbedBuilder() {
                Title = info.Title,
                Description = info.Plot,
                Url = $"http://www.imdb.com/title/{ info.IMDbId }",
                Color = DiscordColor.Yellow
            };

            if (!string.IsNullOrWhiteSpace(info.Type))
                emb.AddField("Type", info.Type, inline: true);
            if (!string.IsNullOrWhiteSpace(info.AirYears))
                emb.AddField("Air time", info.AirYears, inline: true);
            if (!string.IsNullOrWhiteSpace(info.IMDbId))
                emb.AddField("IMDb ID", info.IMDbId, inline: true);
            if (!string.IsNullOrWhiteSpace(info.Genre))
                emb.AddField("Genre", info.Genre, inline: true);
            if (!string.IsNullOrWhiteSpace(info.ReleaseDate))
                emb.AddField("Release date", info.ReleaseDate, inline: true);
            if (!string.IsNullOrWhiteSpace(info.Rated))
                emb.AddField("Rated", info.Rated, inline: true);
            if (!string.IsNullOrWhiteSpace(info.Duration))
                emb.AddField("Duration", info.Duration, inline: true);
            if (!string.IsNullOrWhiteSpace(info.Actors))
                emb.AddField("Actors", info.Actors, inline: true);
            if (!string.IsNullOrWhiteSpace(info.IMDbRating) && !string.IsNullOrWhiteSpace(info.IMDbVotes))
                emb.AddField("IMDb rating", $"{info.IMDbRating} out of {info.IMDbVotes} votes", inline: true);
            if (!string.IsNullOrWhiteSpace(info.Writer))
                emb.AddField("Writer", info.Writer, inline: true);
            if (!string.IsNullOrWhiteSpace(info.Director))
                emb.AddField("Director", info.Director, inline: true);
            if (!string.IsNullOrWhiteSpace(info.Poster) && info.Poster != "N/A")
                emb.WithThumbnailUrl(info.Poster);

            emb.WithFooter("Powered by OMDb.");

            return emb.Build();
        }


        private async Task<IReadOnlyList<MovieInfo>> SearchQueryAsync(string query)
        {
            try {
                var response = await _http.GetStringAsync($"{_requestUrl}&s={query}")
                    .ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<OMDbResponse>(response);
                return data.Success ? data.Results?.AsReadOnly() : null;
            } catch (Exception e) {
                // LogProvider.LogProvider.LogException(LogLevel.Debug, e);
                return null;
            }
        }

        private async Task<MovieInfo> SearchAsync(OMDbQueryType type, string q)
        {
            try {
                var response = await _http.GetStringAsync($"{_requestUrl}&{type.ToApiString()}={q}")
                    .ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<MovieInfo>(response);
                return data.Success ? data : null;
            } catch (Exception e) {
                // LogProvider.LogProvider.LogException(LogLevel.Debug, e);
                return null;
            }
        }

        private async Task<MovieInfo> GetMovieInfoAsync(string id)
        {
            try {
                var response = await _http.GetStringAsync($"{_requestUrl}&i={ id }")
                    .ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<MovieInfo>(response);
                return data.Success ? data : null;
            } catch (Exception e) {
                // LogProvider.LogProvider.LogException(LogLevel.Debug, e);
                return null;
            }
        }

        private Page EmbedInfoInPage(MovieInfo info)
            => new Page() { Embed = EmbedInfo(info) };
    }
}
