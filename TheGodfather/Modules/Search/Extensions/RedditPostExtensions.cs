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
            emb.WithTitle(msg.Title);
            emb.WithDescription(Formatter.Strip(msg.MarkdownText), unknown: false);
            if (Uri.TryCreate(msg.ThumbnailUrl, UriKind.Absolute, out Uri? uri))
                emb.WithThumbnail(uri);
            emb.WithUrl(msg.Url);

            emb.AddLocalizedTitleField("str-type", msg.PostType, inline: true);
            emb.AddLocalizedTitleField("str-author", msg.Author, inline: true);
            emb.AddLocalizedTitleField("str-comments", msg.CommentCount, inline: true);
            emb.AddLocalizedTitleField("str-upvotes", msg.UpvoteCount, inline: true);
            emb.AddLocalizedTitleField("str-upvote-ratio", msg.UpvoteRatio, inline: true);
            emb.AddLocalizedTitleField("str-archived", msg.IsArchived, inline: true);
            emb.AddLocalizedTitleField("str-locked", msg.IsLocked, inline: true);
            emb.AddLocalizedTitleField("str-nsfw", msg.IsNsfw, inline: true);
            emb.AddLocalizedTitleField("str-pinned", msg.IsPinned, inline: true);
            emb.AddLocalizedTitleField("str-spoiler", msg.IsSpoiler, inline: true);
            if (msg.AwardCount > 0)
                emb.AddLocalizedTitleField("str-awards", msg.AwardCount, inline: true);

            return emb;
        }
    }
}
