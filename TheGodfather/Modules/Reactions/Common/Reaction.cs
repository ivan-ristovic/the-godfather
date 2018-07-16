#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using TheGodfather.Common.Collections;
#endregion

namespace TheGodfather.Modules.Reactions.Common
{
    public abstract class Reaction : IEquatable<Reaction>
    {
        private static string GetRegexString(string s)
            => $@"\b{s.ToLowerInvariant()}\b";

        public ConcurrentHashSet<Regex> TriggerRegexes { get; protected set; } = new ConcurrentHashSet<Regex>();
        public IEnumerable<string> TriggerStrings => TriggerRegexes.Select(rgx => rgx.ToString().Substring(2, rgx.ToString().Length - 4));
        public IEnumerable<string> OrderedTriggerStrings => TriggerStrings.OrderBy(s => s);
        public int Id { get; }
        public string Response { get; }


        protected Reaction(int id, string trigger, string response, bool is_regex_trigger = false)
        {
            Id = id;
            AddTrigger(trigger, is_regex_trigger);
            Response = response;
        }


        public bool AddTrigger(string trigger, bool regex = false)
        {
            if (regex)
                return TriggerRegexes.Add(new Regex(GetRegexString(trigger.ToLowerInvariant()), RegexOptions.IgnoreCase));
            else
                return TriggerRegexes.Add(new Regex(GetRegexString(Regex.Escape(trigger.ToLowerInvariant())), RegexOptions.IgnoreCase));
        }

        public bool RemoveTrigger(string trigger)
        {
            var rstr = GetRegexString(trigger);
            return TriggerRegexes.RemoveWhere(r => r.ToString() == rstr) > 0;
        }

        public bool Matches(string str)
            => TriggerRegexes.Any(rgx => rgx.IsMatch(str));

        public bool ContainsTriggerPattern(string pattern)
            => TriggerStrings.Any(s => pattern == s);

        public bool HasSameResponseAs<T>(T other) where T : Reaction
            => Response == other.Response;

        public bool Equals(Reaction other)
            => HasSameResponseAs(other);
    }
}
