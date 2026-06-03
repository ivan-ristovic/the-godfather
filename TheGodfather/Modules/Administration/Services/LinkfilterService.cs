using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Modules.Administration.Services;

public sealed class LinkfilterService : ITheGodfatherService
{
    public bool IsDisabled => false;


    public LinkfilterResult? Match(string content, LinkfilterSettings settings, bool isAdministrator = false)
    {
        if (!settings.Enabled || isAdministrator || string.IsNullOrWhiteSpace(content))
            return null;

        if (settings.BlockDiscordInvites && LinkfilterMatcherCollection.InviteRegex.Match(content) is { Success: true } invite)
            return new LinkfilterResult("Discord invite", invite.Groups[0].Value);

        if (settings.BlockBooterWebsites && LinkfilterMatcherCollection.BooterMatcher.MatchContent(content) is { Success: true } booter)
            return new LinkfilterResult("DDoS/Booter website", booter.Matched);

        if (settings.BlockIpLoggingWebsites && LinkfilterMatcherCollection.IpLoggerMatcher.MatchContent(content) is { Success: true } logger)
            return new LinkfilterResult("IP logging website", logger.Matched);

        if (settings.BlockDisturbingWebsites && LinkfilterMatcherCollection.DisturbingWebsiteMatcher.MatchContent(content) is { Success: true } disturbing)
            return new LinkfilterResult("Disturbing content website", disturbing.Matched);

        if (settings.BlockUrlShorteners && LinkfilterMatcherCollection.UrlShortenerMatcher.MatchContent(content) is { Success: true } shortener)
            return new LinkfilterResult("URL shortener website", shortener.Matched);

        return null;
    }
}

public sealed record LinkfilterResult(string Cause, string? Matched);
