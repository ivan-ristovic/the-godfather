#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using TheGodfather.Helpers.DataManagers;
/*
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
*/
#endregion

namespace TheGodfather.Services
{
    public class YoutubeService
    {
        private YouTubeService _yt { get; set; }


        public YoutubeService(string key)
        {
            /*
            UserCredential credential;
            using (var stream = new FileStream("Resources/yt_secret.json", FileMode.Open, FileAccess.Read)) {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.YoutubeReadonly },
                    "user", CancellationToken.None, new FileDataStore("Books.ListMyLibrary")
                );
            }
            */

            _yt = new YouTubeService(new BaseClientService.Initializer() {
                ApiKey = key,
                ApplicationName = "TheGodfather"
                // HttpClientInitializer = credential
            });
        }


        public async Task<DiscordEmbed> GetEmbeddedResults(string query, int ammount, string type = null)
        {
            var res = await GetResultsAsync(query, ammount, type)
                .ConfigureAwait(false);
            return EmbedYouTubeResults(res);
        }

        private async Task<List<SearchResult>> GetResultsAsync(string query, int ammount, string type = null)
        {
            var searchListRequest = _yt.Search.List("snippet");
            searchListRequest.Q = query;
            searchListRequest.MaxResults = ammount;
            if (type != null)
                searchListRequest.Type = type;

            var searchListResponse = await searchListRequest.ExecuteAsync()
                .ConfigureAwait(false);

            List<SearchResult> videos = new List<SearchResult>();
            videos.AddRange(searchListResponse.Items);

            return videos;
        }

        private DiscordEmbed EmbedYouTubeResults(List<SearchResult> results)
        {
            if (results == null || results.Count == 0)
                return new DiscordEmbedBuilder() { Description = "No results...", Color = DiscordColor.Red };

            if (results.Count > 25)
                results = results.Take(25).ToList();

            var em = new DiscordEmbedBuilder() {
                Color = DiscordColor.Red
            };
            foreach (var r in results) {
                switch (r.Id.Kind) {
                    case "youtube#video":
                        em.AddField(r.Snippet.Title, "https://www.youtube.com/watch?v=" + r.Id.VideoId);
                        break;

                    case "youtube#channel":
                        em.AddField(r.Snippet.Title, "https://www.youtube.com/channel/" + r.Id.ChannelId);
                        break;

                    case "youtube#playlist":
                        em.AddField(r.Snippet.Title, "https://www.youtube.com/playlist?list=" + r.Id.PlaylistId);
                        break;
                }
            }
            return em.Build();
        }
    }
}
