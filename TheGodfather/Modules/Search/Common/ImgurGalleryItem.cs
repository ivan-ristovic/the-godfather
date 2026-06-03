namespace TheGodfather.Modules.Search.Common;

public enum SubredditGallerySortOrder
{
    Time,
    Top
}

public enum TimeWindow
{
    Day,
    Week,
    Month,
    Year,
    All
}

public abstract class ImgurGalleryItem
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Link { get; init; }
    public DateTimeOffset? DateTime { get; init; }
    public bool? Nsfw { get; init; }
    public int? Views { get; init; }
    public int? Score { get; init; }
    public int? Points { get; init; }
    public int? Ups { get; init; }
    public int? Downs { get; init; }
    public int? CommentCount { get; init; }
}

public sealed class ImgurGalleryImage : ImgurGalleryItem;

public sealed class ImgurGalleryAlbum : ImgurGalleryItem;
