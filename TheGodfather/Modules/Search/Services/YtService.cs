using System.Text.RegularExpressions;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json.Linq;
using YoutubeExplode;
using YoutubeExplode.Channels;
using Channel = YoutubeExplode.Channels.Channel;

namespace TheGodfather.Modules.Search.Services;

public sealed class YtService : TheGodfatherHttpService
{
    private const string YtApiUrl = "https://www.googleapis.com/youtube/v3";
    private const string YtUrl = "https://www.youtube.com";
    private static readonly Regex _ytVanityRegex =
        new(@"youtube\..+?/c/(.*?)(?:\?|&|/|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override bool IsDisabled => this.yt is null;

    private readonly YoutubeClient ytExplode;
    private readonly YouTubeService? yt;


    public YtService(BotConfigService cfg)
    {
        this.ytExplode = new YoutubeClient();
        if (!string.IsNullOrWhiteSpace(cfg.CurrentConfiguration.YouTubeKey))
            this.yt = new YouTubeService(new BaseClientService.Initializer {
                ApiKey = cfg.CurrentConfiguration.YouTubeKey,
                ApplicationName = TheGodfather.ApplicationName
            });
    }


    public async Task<string?> GetRssUrlForChannel(string? idOrUrl)
    {
        if (this.IsDisabled)
            return null;

        string? id = await this.GetChannelIdAsync(idOrUrl);
        return id is not null ? $"{YtUrl}/feeds/videos.xml?channel_id={idOrUrl}" : null;
    }

    public async Task<string?> GetChannelIdAsync(string? idOrUrl)
    {
        if (this.IsDisabled || string.IsNullOrWhiteSpace(idOrUrl))
            return null;

        ChannelId? cid = ChannelId.TryParse(idOrUrl);
        if (cid is not null)
            return cid.Value;

        //VideoId? vid = VideoId.TryParse(idOrUrl);
        //if (vid is { }) {
        //    YoutubeExplode.Channels.Channel channel = await this.ytExplode.Channels.GetByVideoAsync(vid.Value);
        //    return channel.Id;
        //}

        UserName? uid = UserName.TryParse(idOrUrl);
        if (uid is not null) {
            Channel channel = await this.ytExplode.Channels.GetByUserAsync(uid.Value);
            return channel.Id;
        }

        string vanityName = _ytVanityRegex.Match(idOrUrl).Groups[1].Value;
        if (string.IsNullOrWhiteSpace(vanityName))
            return null;

        string url = $"{YtApiUrl}/channels?key={this.yt?.ApiKey}&forUsername={vanityName}&part=id";
        try {
            string response = await _http.GetStringAsync(url).ConfigureAwait(false);
            JToken? it = JObject.Parse(response)["items"];
            if (it is null)
                return null;
            List<Dictionary<string, string>>? items = it.ToObject<List<Dictionary<string, string>>>();
            if (items is not null && items.Any())
                return items.First()["id"];
        } catch (Exception e) {
            Log.Error(e, "Failed to get/parse YouTube API response for request URL: {YtRequestUrl}", url);
        }

        return null;
    }

    public string? GetUrlForResourceId(ResourceId id)
    {
        return id.Kind switch {
            "youtube#video" => $"{YtUrl}/watch?v={id.VideoId}",
            "youtube#channel" => $"{YtUrl}/channel/{id.ChannelId}",
            "youtube#playlist" => $"{YtUrl}/playlist?list={id.PlaylistId}",
            _ => null
        };
    }

    public async Task<IReadOnlyList<SearchResult>?> SearchAsync(string query, int amount, string? type = null)
    {
        if (this.IsDisabled)
            return null;

        SearchResource.ListRequest request = this.yt!.Search.List("snippet");
        request.Q = query;
        request.MaxResults = amount is > 1 and < 20 ? amount : 10;
        if (!string.IsNullOrWhiteSpace(type))
            request.Type = type;

        try {
            SearchListResponse response = await request.ExecuteAsync().ConfigureAwait(false);
            return response.Items.ToList().AsReadOnly();
        } catch (Exception e) {
            Log.Error(e, "Failed to get/parse YouTube API response for query: {Query}", query);
        }

        return null;
    }
}