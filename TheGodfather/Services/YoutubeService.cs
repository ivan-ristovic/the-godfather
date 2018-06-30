#region USING_DIRECTIVES
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Modules.Music.Common;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

using YoutubeExplode;
#endregion

namespace TheGodfather.Services
{
    public class YoutubeService : TheGodfatherHttpService
    {
        private YouTubeService _yt { get; set; }
        private string _key { get; set; }


        public YoutubeService(string key)
        {
            _key = key;
            _yt = new YouTubeService(new BaseClientService.Initializer() {
                ApiKey = key,
                ApplicationName = "TheGodfather"
            });
        }


        public static string GetYoutubeRSSFeedLinkForChannelId(string id) =>
            $"https://www.youtube.com/feeds/videos.xml?channel_id={ id }";


        public async Task<string> GetFirstVideoResultAsync(string query)
        {
            var res = await GetResultsAsync(query, 1, "video")
                .ConfigureAwait(false);
            if (!res.Any())
                return null;
            return $"https://www.youtube.com/watch?v={ res.FirstOrDefault()?.Id.VideoId }";
        }

        public async Task<IReadOnlyList<Page>> GetPaginatedResults(string query, int amount = 1, string type = null)
        {
            var res = await GetResultsAsync(query, amount, type)
                .ConfigureAwait(false);
            return PaginateSearchResult(res);
        }

        public async Task<SongInfo> GetSongInfoAsync(string url)
        {
            SongInfo si = await GetSongInfoViaYtExplode(url)
                .ConfigureAwait(false);
            if (si != null)
                return si;

            return await GetSongInfoViaYtDl(url)
                .ConfigureAwait(false);
        }

        public async Task<string> GetYoutubeIdAsync(string url)
        {
            if (YoutubeClient.TryParseChannelId(url, out string id))
                return id;

            id = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
            try {
                var u = $"https://www.googleapis.com/youtube/v3/channels?key={ _key }&forUsername={ id }&part=id";
                var jsondata = await _http.GetStringAsync(u)
                    .ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<YoutubeResponse>(jsondata);
                if (data.Items != null && data.Items.Any())
                    return data.Items.First()["id"];
            } catch (Exception e) {
                TheGodfather.LogProvider.LogException(LogLevel.Debug, e);
            }

            return null;
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

        private IReadOnlyList<Page> PaginateSearchResult(IEnumerable<SearchResult> results)
        {
            if (results == null || !results.Any())
                return null;

            List<Page> pages = new List<Page>();
            foreach (var res in results.Take(10)) {
                var emb = new DiscordEmbedBuilder() {
                    Title = res.Snippet.Title,
                    Description = Formatter.Italic(string.IsNullOrWhiteSpace(res.Snippet.Description) ? "No description provided" : res.Snippet.Description),
                    Color = DiscordColor.Red,
                    ThumbnailUrl = res.Snippet.Thumbnails.Default__.Url
                };
                emb.AddField("Channel", res.Snippet.ChannelTitle, inline: true);
                emb.AddField("Published at", $"{res.Snippet.PublishedAt ?? DateTime.Now}", inline: true);
                switch (res.Id.Kind) {
                    case "youtube#video":
                        emb.WithUrl("https://www.youtube.com/watch?v=" + res.Id.VideoId);
                        break;
                    case "youtube#channel":
                        emb.WithDescription("https://www.youtube.com/channel/" + res.Id.ChannelId);
                        break;
                    case "youtube#playlist":
                        emb.WithDescription("https://www.youtube.com/playlist?list=" + res.Id.PlaylistId);
                        break;
                }
                pages.Add(new Page() { Embed = emb.Build() });
            }

            return pages.AsReadOnly();
        }

        private async Task<SongInfo> GetSongInfoViaYtExplode(string url)
        {
            try {
                if (!YoutubeClient.TryParseVideoId(url, out var id))
                    return null;

                var client = new YoutubeClient();
                var video = await client.GetVideoAsync(id);
                if (video == null)
                    return null;

                var streamInfo = await client.GetVideoMediaStreamInfosAsync(video.Id);
                var stream = streamInfo.Audio.OrderByDescending(x => x.Bitrate).FirstOrDefault();
                if (stream == null)
                    return null;

                return new SongInfo {
                    Provider = "YouTube",
                    Query = "https://youtube.com/watch?v=" + video.Id,
                    Thumbnail = video.Thumbnails.MediumResUrl,
                    TotalTime = video.Duration,
                    Uri = stream.Url,
                    VideoId = video.Id,
                    Title = video.Title,
                };
            } catch (Exception e) {
                TheGodfather.LogProvider.LogException(LogLevel.Debug, e);
                return null;
            }
        }

        private async Task<SongInfo> GetSongInfoViaYtDl(string url)
        {
            string[] data = null;
            try {
                var ytdlinfo = new ProcessStartInfo() {
                    FileName = "Resources/youtube-dl",
                    Arguments = $"-4 --geo-bypass -f bestaudio -e --get-url --get-id --get-thumbnail --get-duration --no-check-certificate --default-search \"ytsearch:\" \"{url}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };
                using (var process = new Process() { StartInfo = ytdlinfo }) {
                    process.Start();
                    var str = await process.StandardOutput.ReadToEndAsync();
                    var err = await process.StandardError.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(err))
                        TheGodfather.LogProvider.LogMessage(LogLevel.Warning, err);
                    if (!string.IsNullOrWhiteSpace(str))
                        data = str.Split('\n');
                }

                if (data == null || data.Length < 6)
                    return null;

                if (!TimeSpan.TryParseExact(data[4], new[] { "ss", "m\\:ss", "mm\\:ss", "h\\:mm\\:ss", "hh\\:mm\\:ss", "hhh\\:mm\\:ss" }, CultureInfo.InvariantCulture, out var time))
                    time = TimeSpan.FromHours(24);

                return new SongInfo() {
                    Title = data[0],
                    VideoId = data[1],
                    Uri = data[2],
                    Thumbnail = data[3],
                    TotalTime = time,
                    Provider = "YouTube",
                    Query = "https://youtube.com/watch?v=" + data[1],
                };
            } catch (Exception e) {
                TheGodfather.LogProvider.LogException(LogLevel.Debug, e);
                return null;
            }
        }
    }
}
