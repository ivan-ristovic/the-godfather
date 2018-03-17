using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TheGodfather.Extensions.Collections;

namespace TheGodfather.Modules.Reactions.Common
{
    public abstract class Reaction
    {
        public ConcurrentHashSet<Regex> TriggerRegexes { get; protected set; } = new ConcurrentHashSet<Regex>();
        public IEnumerable<string> TriggerStrings => TriggerRegexes.Select(rgx => rgx.ToString().Substring(2, rgx.ToString().Length - 4));
        public IEnumerable<string> OrderedTriggerStrings => TriggerStrings.OrderBy(s => s);


        public bool AddTrigger(string trigger, bool is_regex_trigger = false)
        {
            if (is_regex_trigger)
                return TriggerRegexes.Add(new Regex($@"\b{trigger.ToLowerInvariant()}\b", RegexOptions.IgnoreCase));
            else
                return TriggerRegexes.Add(new Regex($@"\b{Regex.Escape(trigger.ToLowerInvariant())}\b", RegexOptions.IgnoreCase));
        }


        public bool ContainsPattern(string pattern)
            => TriggerStrings.Any(s => pattern == s);

        public abstract bool HasSameResponseAs(Reaction other);
    }
}
