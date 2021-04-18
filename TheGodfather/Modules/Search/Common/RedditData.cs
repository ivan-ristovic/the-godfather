#nullable disable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TheGodfather.Modules.Search.Common
{
    public sealed class RedditError
    {
        [JsonProperty("error")]
        public int ErrorCode { get; set; }

        [JsonProperty("message")]
        public string ErrorMessage { get; set; }
    }

    public sealed class RedditListing
    {
        [JsonProperty("data")]
        public RedditData Data { get; set; }

        [JsonProperty("error")]
        public int ErrorCode { get; set; }

        [JsonProperty("message")]
        public string ErrorMessage { get; set; }


        public RedditError Error => new RedditError { 
            ErrorCode = this.ErrorCode, 
            ErrorMessage = this.ErrorMessage, 
        };
    }

    public sealed class RedditData
    {
        [JsonProperty("modhash")]
        public string ModHash { get; set; }

        [JsonProperty("dist")]
        public int Dist { get; set; }

        [JsonProperty("children")]
        public List<RedditPostWrapper> Posts { get; set; }
    }

    public sealed class RedditPostWrapper
    {
        [JsonProperty("data")]
        public RedditPost Data { get; set; }
    }

    public sealed class RedditPost
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("selftext")]
        public string MarkdownText { get; set; }

        [JsonProperty("post_hint")]
        public string PostType { get; set; }

        [JsonProperty("subreddit_name_prefixed")]
        public string Subreddit { get; set; }

        [JsonProperty("upvote_ratio")]
        public float UpvoteRatio { get; set; }

        [JsonProperty("ups")]
        public int UpvoteCount { get; set; }

        [JsonProperty("total_awards_received")]
        public int AwardCount { get; set; }

        [JsonProperty("thumbnail")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("archived")]
        public bool IsArchived { get; set; }

        [JsonProperty("locked")]
        public bool IsLocked { get; set; }

        [JsonProperty("pinned")]
        public bool IsPinned { get; set; }

        [JsonProperty("over_18")]
        public bool IsNsfw { get; set; }

        [JsonProperty("spoiler")]
        public bool IsSpoiler { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("num_comments")]
        public int CommentCount { get; set; }

        [JsonProperty("permalink")]
        public string Permalink { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
