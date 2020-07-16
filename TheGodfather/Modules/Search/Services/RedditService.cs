#region USING_DIRECTIVES
using System;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
#endregion

namespace TheGodfather.Modules.Search.Services
{
    public enum RedditCategory
    {
        Hot,
        New,
        Rising,
        Controversial,
        Top,
        Gilded
    }


    public static class RedditCategoryExtensions
    {
        public static string ToUrlPartString(this RedditCategory category)
        {
            switch (category) {
                case RedditCategory.Controversial: return "controversial";
                case RedditCategory.Gilded: return "gilded";
                case RedditCategory.Hot: return "hot";
                case RedditCategory.New: return "new";
                case RedditCategory.Rising: return "rising";
                case RedditCategory.Top: return "top";
            }
            throw new ArgumentException("Unknown reddit category", nameof(category));
        }
    }


    public static class RedditService
    {
        private static readonly Regex _subPrefixRegex = new Regex("^/?r?/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _sanitizeRegex = new Regex("[^a-z0-9/]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _urlRegex = new Regex("<span> *<a +href *= *\"([^\"]+)\"> *\\[link\\] *</a> *</span>", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        public static string GetFeedURLForSubreddit(string sub, RedditCategory category, out string rsub)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new ArgumentException("Subreddit missing", nameof(sub));

            if (_sanitizeRegex.IsMatch(sub))
                throw new ArgumentException("Subreddit is in invalid format (needs to be lowercase and without spaces, for example `/r/rule34`)", nameof(sub));

            sub = _subPrefixRegex.Replace(sub, string.Empty);
            rsub = "/r/" + sub.ToLowerInvariant();

            string url = $"https://www.reddit.com{rsub}/{category.ToUrlPartString()}.rss";
            if (!RssService.IsValidFeedURL(url))
                return null;

            return url;
        }

        public static string GetImageUrl(TextSyndicationContent content)
        {
            Match m = _urlRegex.Match(content.Text);
            if (m.Success)
                return m.Groups[1].Value;
            else
                return null;
        }
    }
}
