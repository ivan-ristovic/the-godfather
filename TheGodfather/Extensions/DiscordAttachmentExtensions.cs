using DSharpPlus;
using DSharpPlus.Entities;

namespace TheGodfather.Extensions;

public static class DiscordAttachmentExtensions
{
    public static string ToMaskedUrl(this DiscordAttachment a)
        => Formatter.MaskedUrl($"{a.FileName} ({a.FileSize.ToMetric(decimals: 0)}B)", new Uri(a.Url));
}