#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Services
{
    public class MovieInfoService : HttpService, IGodfatherService
    {
        private string _requestUrl;


        public MovieInfoService(string key)
        {
            _requestUrl = $"http://www.omdbapi.com/?apikey={ key }";
        }


        public async Task<IReadOnlyList<Page>> GetPaginatedResultsAsync(string query)
        {
            var ids = await SearchAndRetrieveResultIdsAsync(query)
                .ConfigureAwait(false);

            if (ids == null || !ids.Any())
                return null;

            List<MovieInfo> detailedResults = new List<MovieInfo>();
            foreach (var id in ids) {
                var detailed = await GetMovieInfoAsync(id)
                    .ConfigureAwait(false);
                if (detailed == null || !detailed.Success)
                    continue;
                detailedResults.Add(detailed);
            }

            return detailedResults.Select(info => {
                var emb = new DiscordEmbedBuilder() {
                    Title = info.Title,
                    Description = info.Plot,
                    Url = $"http://www.imdb.com/title/{ info.IMDbId }",
                    Color = DiscordColor.Yellow
                };
                emb.AddField("Type", info.Type, inline: true)
                   .AddField("Genre", info.Genre, inline: true)
                   .AddField("Release date", info.ReleaseDate, inline: true)
                   .AddField("Rated", info.Rated, inline: true)
                   .AddField("Duration", info.Duration, inline: true)
                   .AddField("Actors", info.Actors, inline: true)
                   .AddField("IMDb rating", $"{info.IMDbRating} out of {info.IMDbVotes} votes", inline: true)
                   .AddField("Writer", info.Writer, inline: true)
                   .AddField("Director", info.Director, inline: true);

                if (info.Poster != "N/A")
                    emb.WithThumbnailUrl(info.Poster);

                return new Page() { Embed = emb.Build() };
            }).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<Page>> GetPaginatedResultsByIdAsync(string id)
        {
            return null;
        }


        private async Task<IReadOnlyList<string>> SearchAndRetrieveResultIdsAsync(string query)
        {
            try {
                var response = await _http.GetStringAsync($"{_requestUrl}&s={query}")
                    .ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<OMDbResponse>(response);
                if (data.Success)
                    return data.Results.Select(r => r.IMDbId).ToList().AsReadOnly();
                else
                    return null;
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return null;
            }
        }

        private async Task<MovieInfo> GetMovieInfoAsync(string id)
        {
            try {
                var response = await _http.GetStringAsync($"{_requestUrl}&i={ id }")
                    .ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<MovieInfo>(response);
                if (data.Success)
                    return data;
                else
                    return null;
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return null;
            }
        }
    }
}
