using System;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;

namespace TheGodfather.Extensions
{
    public static class DiscordAttachmentExtensions
    {
        public static string ToMaskedUrl(this DiscordAttachment a)
            => Formatter.MaskedUrl($"{a.FileName} ({a.FileSize.ToMetric(decimals: 0)}B)", new Uri(a.Url));
    }
}
