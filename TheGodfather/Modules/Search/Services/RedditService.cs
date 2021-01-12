using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using Humanizer;
using TheGodfather.Modules.Search.Common;

namespace TheGodfather.Modules.Search.Services
{
    public static class RedditService
    {
        private static readonly Regex _subPrefixRegex = new Regex("^/?r?/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _sanitizeRegex = new Regex("[^a-z0-9/]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _urlRegex = new Regex(
            "<span> *<a +href *= *\"([^\"]+)\"> *\\[link\\] *</a> *</span>", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );


        public static string? GetFeedURLForSubreddit(string sub, RedditCategory category, out string? rsub)
        {
            rsub = null;

            if (string.IsNullOrWhiteSpace(sub))
                return null;

            if (_sanitizeRegex.IsMatch(sub))
                return null;

            sub = _subPrefixRegex.Replace(sub, string.Empty);
            rsub = "/r/" + sub.ToLowerInvariant();

            string url = $"https://www.reddit.com{rsub}/{category.Humanize(LetterCasing.LowerCase)}.rss";
            return RssFeedsService.IsValidFeedURL(url) ? url : null;
        }

        public static string? GetImageUrl(TextSyndicationContent content)
        {
            Match m = _urlRegex.Match(content.Text);
            return m.Success ? m.Groups[1].Value : null;
        }
    }
}
