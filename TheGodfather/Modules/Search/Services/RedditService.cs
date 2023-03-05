using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Exceptions;

namespace TheGodfather.Modules.Search.Services;

public sealed class RedditService : TheGodfatherHttpService
{
    private static readonly Regex _subPrefixRegex = new("^/?r?/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _sanitizeRegex = new("[^_a-z0-9/]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override bool IsDisabled => false;


    public async Task<IEnumerable<RedditPost>?> GetPostsAsync(string sub, RedditCategory category, int limit = 10)
    {
        if (limit is < 1 or > DiscordLimits.EmbedFieldLimit)
            limit = 10;

        if (string.IsNullOrWhiteSpace(sub))
            return null;

        if (_sanitizeRegex.IsMatch(sub))
            return null;

        sub = $"/r/{_subPrefixRegex.Replace(sub, string.Empty).ToLowerInvariant()}";
        string url = $"https://www.reddit.com{sub}/{category.Humanize(LetterCasing.LowerCase)}/.json?limit={limit}";

        try {
            string json = await _http.GetStringAsync(url);
            RedditListing res = JsonConvert.DeserializeObject<RedditListing>(json) ?? throw new JsonSerializationException();
            if (res.ErrorMessage is { })
                throw new SearchServiceException<RedditError>(res.ErrorMessage, res.Error);

            return res.Data.Posts
                .Select(pw => pw.Data)
                .ToList()
                .AsReadOnly();
        } catch (HttpRequestException e) {
            Log.Error(e, "Failed to fetch reddit API JSON");
            throw new SearchServiceException<RedditError>(e.Message, new RedditError {
                ErrorCode = e.StatusCode is { } ? (int)e.StatusCode : 400,
                ErrorMessage = e.Message
            });
        } catch (JsonSerializationException e) {
            Log.Error(e, "Failed to deserialize reddit API JSON");
        } catch (SearchServiceException<RedditError> e) {
            Log.Error(e, "Failed to retrieve reddit API data");
            throw;
        }

        return null;
    }

    public string? GetFeedUrlForSubreddit(string sub, RedditCategory category, out string? rsub)
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
}