#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;
#endregion

namespace TheGodfather.EventListeners.Common
{
    public class LinkfilterMatch
    {
        public bool Success { get; }
        public string Matched { get; }


        public LinkfilterMatch(Match match)
        {
            Success = match.Success;
            Matched = match.Groups[0].Value;
        }
    }

    public class LinkfilterMatcher : Regex
    {
        public LinkfilterMatcher(params string[] items) 
            : base($@"({string.Join("|", items.Select(Escape))})", RegexOptions.IgnoreCase)
        {

        }

        public LinkfilterMatcher(IEnumerable<string> items) 
            : base($@"({string.Join("|", items.Select(Escape))})", RegexOptions.IgnoreCase)
        {

        }

        public LinkfilterMatch Check(DiscordMessage message)
            => new LinkfilterMatch(Match(message.Content));
    }
}
