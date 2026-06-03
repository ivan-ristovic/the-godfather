using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using TheGodfather.Modules.Search.Common;

namespace TheGodfather.Modules.Search.Services;

public sealed class ImgurService : TheGodfatherHttpService
{
    public const string ServiceUrl = "api.imgur.com";
    private const string SubredditGalleryEndpoint = $"https://{ServiceUrl}/3/gallery/r";

    public override bool IsDisabled => string.IsNullOrWhiteSpace(this.clientId);

    private readonly string? clientId;


    public ImgurService(BotConfigService cfg)
    {
        this.clientId = cfg.CurrentConfiguration.ImgurKey;
    }


    public async Task<IEnumerable<ImgurGalleryItem>?> GetItemsFromSubAsync(string sub, int amount, SubredditGallerySortOrder order, TimeWindow tw)
    {
        if (this.IsDisabled)
            return null;
            
        if (amount is < 1 or > 10)
            amount = 10;

        string url = $"{SubredditGalleryEndpoint}/{WebUtility.UrlEncode(sub)}/{ToSegment(order)}/{ToSegment(tw)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Client-ID", this.clientId);

        try {
            using HttpResponseMessage response = await _http.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return [];

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            ImgurGalleryResponse? data = JsonConvert.DeserializeObject<ImgurGalleryResponse>(json);
            return data?.Data?
                .Select(ToGalleryItem)
                .Where(item => item.Link is not null)
                .Take(amount)
                .ToArray();
        } catch (Exception e) when (e is HttpRequestException or JsonSerializationException) {
            Log.Error(e, "Failed to fetch/deserialize Imgur gallery response");
            return null;
        }
    }

    private static string ToSegment(SubredditGallerySortOrder sort)
        => sort switch {
            SubredditGallerySortOrder.Time => "time",
            SubredditGallerySortOrder.Top => "top",
            _ => "top"
        };

    private static string ToSegment(TimeWindow window)
        => window switch {
            TimeWindow.Day => "day",
            TimeWindow.Week => "week",
            TimeWindow.Month => "month",
            TimeWindow.Year => "year",
            TimeWindow.All => "all",
            _ => "day"
        };

    private static ImgurGalleryItem ToGalleryItem(ImgurGalleryItemDto dto)
    {
        DateTimeOffset? timestamp = dto.DateTime is null ? null : DateTimeOffset.FromUnixTimeSeconds(dto.DateTime.Value);
        return dto.IsAlbum
            ? new ImgurGalleryAlbum {
                Title = dto.Title,
                Description = dto.Description,
                Link = dto.Link,
                DateTime = timestamp,
                Nsfw = dto.Nsfw,
                Views = dto.Views,
                Score = dto.Score,
                Points = dto.Points,
                Ups = dto.Ups,
                Downs = dto.Downs,
                CommentCount = dto.CommentCount
            }
            : new ImgurGalleryImage {
                Title = dto.Title,
                Description = dto.Description,
                Link = dto.Link,
                DateTime = timestamp,
                Nsfw = dto.Nsfw,
                Views = dto.Views,
                Score = dto.Score,
                Points = dto.Points,
                Ups = dto.Ups,
                Downs = dto.Downs,
                CommentCount = dto.CommentCount
            };
    }

    private sealed class ImgurGalleryResponse
    {
        [JsonProperty("data")]
        public IReadOnlyList<ImgurGalleryItemDto>? Data { get; init; }
    }

    private sealed class ImgurGalleryItemDto
    {
        [JsonProperty("is_album")]
        public bool IsAlbum { get; init; }

        [JsonProperty("title")]
        public string? Title { get; init; }

        [JsonProperty("description")]
        public string? Description { get; init; }

        [JsonProperty("link")]
        public string? Link { get; init; }

        [JsonProperty("datetime")]
        public long? DateTime { get; init; }

        [JsonProperty("nsfw")]
        public bool? Nsfw { get; init; }

        [JsonProperty("views")]
        public int? Views { get; init; }

        [JsonProperty("score")]
        public int? Score { get; init; }

        [JsonProperty("points")]
        public int? Points { get; init; }

        [JsonProperty("ups")]
        public int? Ups { get; init; }

        [JsonProperty("downs")]
        public int? Downs { get; init; }

        [JsonProperty("comment_count")]
        public int? CommentCount { get; init; }
    }
}
