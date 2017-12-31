#region USING_DIRECTIVES
using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

using DSharpPlus.Entities;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
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
        private string _key { get; set; }


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

            _key = key;
            _yt = new YouTubeService(new BaseClientService.Initializer() {
                ApiKey = key,
                ApplicationName = "TheGodfather"
                // HttpClientInitializer = credential
            });
        }


        public static string GetYoutubeRSSFeedLinkForChannelId(string id) =>
            "https://www.youtube.com/feeds/videos.xml?channel_id=" + id;


        public async Task<string> GetFirstVideoResultAsync(string query)
        {
            var res = await GetResultsAsync(query, 1, "video").ConfigureAwait(false);
            return "https://www.youtube.com/watch?v=" + res.First().Id.VideoId;
        }

        public async Task<DiscordEmbed> GetEmbeddedResults(string query, int amount, string type = null)
        {
            var res = await GetResultsAsync(query, amount, type)
                .ConfigureAwait(false);
            return EmbedYouTubeResults(res);
        }

        public async Task<string> TryDownloadYoutubeAudioAsync(string url)
        {
            if (!YoutubeClient.TryParseVideoId(url, out string id))
                return null;

            var yt = new YoutubeClient();
            var infos = await yt.GetVideoMediaStreamInfosAsync(id);
            var info = infos.Audio.First();
            string filename = "test.mp3";
            await yt.DownloadMediaStreamAsync(info, filename);
            return filename;
        }

        private async Task<List<SearchResult>> GetResultsAsync(string query, int amount, string type = null)
        {
            var searchListRequest = _yt.Search.List("snippet");
            searchListRequest.Q = query;
            searchListRequest.MaxResults = amount;
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
        
        public async Task<string> GetYoutubeIdAsync(string url)
        {
            string id;

            if (!YoutubeClient.TryParseChannelId(url, out id))
                return id;

            id = url.Split('/').Last();

            var results = FeedService.GetFeedResults(GetYoutubeRSSFeedLinkForChannelId(id));
            if (results == null) {
                try {
                    var wc = new WebClient();
                    var jsondata = await wc.DownloadStringTaskAsync("https://www.googleapis.com/youtube/v3/channels?key=" + _key + "&forUsername=" + id + "&part=id")
                        .ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<DeserializedData>(jsondata);
                    if (data.Items != null)
                        return data.Items[0]["id"];
                } catch {

                }
            } else {
                return id;
            }

            return null;
        }
        
        private sealed class DeserializedData
        {
            [JsonProperty("items")]
            public List<Dictionary<string, string>> Items { get; set; }
        }
    }
}
