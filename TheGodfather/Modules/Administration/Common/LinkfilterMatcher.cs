using System.Text.RegularExpressions;

namespace TheGodfather.Modules.Administration.Common;

public sealed class LinkfilterMatcher : Regex
{
    public LinkfilterMatcher(params string[] items)
        : this((IEnumerable<string>)items) { }

    public LinkfilterMatcher(IEnumerable<string> items)
        : base($@"\b({string.Join("|", items.Select(Escape))})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    {
        if (!items.Any())
            throw new ArgumentException("No items provided for matching.", nameof(items));
    }

    public LinkfilterMatch MatchContent(string content)
        => new(this.Match(content));
}

public sealed class LinkfilterMatch
{
    public bool Success { get; }
    public string? Matched { get; }


    public LinkfilterMatch()
    {
        this.Success = false;
    }

    public LinkfilterMatch(Match match)
    {
        this.Success = match.Success;
        this.Matched = match.Success ? match.Groups[0].Value : null;
    }
}
