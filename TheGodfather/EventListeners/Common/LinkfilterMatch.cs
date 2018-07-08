#region USING_DIRECTIVES
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#endregion

namespace TheGodfather.EventListeners.Common
{
    public class LinkfilterMatch
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

    public class LinkfilterMatcher : Regex
    {
        public LinkfilterMatcher(params string[] items) 
            : base($@"({string.Join("|", items.Select(Escape))})", RegexOptions.IgnoreCase)
        {
            if (!items.Any())
                throw new ArgumentException("No items provided for matching.");
        }

        public LinkfilterMatcher(IEnumerable<string> items) 
            : base($@"({string.Join("|", items.Select(Escape))})", RegexOptions.IgnoreCase)
        {
            if (!items.Any())
                throw new ArgumentException("No items provided for matching.");
        }

        public LinkfilterMatch Check(DiscordMessage message)
            => new LinkfilterMatch(Match(message.Content));
    }
}
