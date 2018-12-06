#region USING_DIRECTIVES
using DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#endregion

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class LinkfilterMatch
    {
        public bool Success { get; }
        public string Matched { get; }


        public LinkfilterMatch()
        {
            this.Success = false;
        }

        public LinkfilterMatch(Match match)
        {
            this.Success = match.Success;
            this.Matched = match.Groups[0].Value;
        }
    }

    public sealed class LinkfilterMatcher : Regex
    {
        public LinkfilterMatcher(params string[] items)
            : base($@"\b({string.Join("|", items.Select(Escape))})\b", RegexOptions.IgnoreCase)
        {
            if (items is null || !items.Any())
                throw new ArgumentException("No items provided for matching.");
        }

        public LinkfilterMatcher(IEnumerable<string> items)
            : base($@"\b({string.Join("|", items.Select(Escape))})\b", RegexOptions.IgnoreCase)
        {
            if (items is null || !items.Any())
                throw new ArgumentException("No items provided for matching.");
        }

        public LinkfilterMatch Check(DiscordMessage message)
            => new LinkfilterMatch(this.Match(message.Content));
    }
}
