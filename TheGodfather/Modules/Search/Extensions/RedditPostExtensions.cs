using System;
using DSharpPlus;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Search.Extensions
{
    public static class RedditPostExtensions
    {
        public static LocalizedEmbedBuilder WithRedditPost(this LocalizedEmbedBuilder emb, RedditPost msg)
        {
            if (msg.IsLocked)
                emb.WithTitle($"[LOCKED] {msg.Title}");
            else if (msg.IsArchived)
                emb.WithTitle($"[ARCHIVED] {msg.Title}");
            else if (msg.IsNsfw)
                emb.WithTitle($"[NSFW] {msg.Title}");
            else if (msg.IsPinned)
                emb.WithTitle($"[PINNED] {msg.Title}");
            else if (msg.IsSpoiler)
                emb.WithTitle($"[SPOILER] {msg.Title}");
            else
                emb.WithTitle(msg.Title);

            emb.WithDescription(Formatter.Strip(msg.MarkdownText), unknown: false);

            if (string.Equals(msg.PostType, "image", StringComparison.InvariantCultureIgnoreCase) && Uri.TryCreate(msg.Url, UriKind.Absolute, out Uri? imageUri))
                emb.WithImageUrl(imageUri);
            else if (Uri.TryCreate(msg.ThumbnailUrl, UriKind.Absolute, out Uri? thumbnailUri))
                emb.WithThumbnail(thumbnailUri);
            emb.WithUrl(msg.Url);

            emb.AddLocalizedTitleField("str-type", msg.PostType, inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-author", msg.Author, inline: true);
            emb.AddLocalizedTitleField("str-comments", msg.CommentCount, inline: true);
            emb.AddLocalizedTitleField("str-upvotes", msg.UpvoteCount, inline: true);
            emb.AddLocalizedTitleField("str-upvote-ratio", msg.UpvoteRatio, inline: true);
            if (msg.AwardCount > 0)
                emb.AddLocalizedTitleField("str-awards", msg.AwardCount, inline: true);

            return emb;
        }
    }
}
