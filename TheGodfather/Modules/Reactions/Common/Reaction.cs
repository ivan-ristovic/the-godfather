#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using TheGodfather.Common.Collections;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Reactions.Common
{
    public abstract class Reaction : IEquatable<Reaction>
    {
        private static readonly Regex _wordBoundaryRegex = new Regex(@"\\b|(\(\^\|\\s\))|(\(\$\|\\s\))", RegexOptions.Compiled);

        public int Id { get; }
        public string Response { get; }
        public int RegexCount => this.triggerRegexes.Count;
        public IEnumerable<string> TriggerStrings => this.triggerRegexes.Select(rgx => _wordBoundaryRegex.Replace(rgx.ToString(), ""));
        public IEnumerable<string> OrderedTriggerStrings => this.TriggerStrings.OrderBy(s => s);

        private readonly ConcurrentHashSet<Regex> triggerRegexes;


        protected Reaction(int id, string trigger, string response, bool isRegex = false)
        {
            this.Id = id;
            this.Response = response;
            this.triggerRegexes = new ConcurrentHashSet<Regex>();
            this.AddTrigger(trigger, isRegex);
        }

        protected Reaction(int id, IEnumerable<string> triggers, string response, bool isRegex = false)
        {
            this.Id = id;
            this.Response = response;
            this.triggerRegexes = new ConcurrentHashSet<Regex>();
            foreach (string trigger in triggers)
                this.AddTrigger(trigger, isRegex);
        }


        public bool AddTrigger(string trigger, bool isRegex = false)
        {
            Regex regex;

            if (isRegex)
                trigger.TryParseRegex(out regex);
            else
                Regex.Escape(trigger).TryParseRegex(out regex);

            return this.triggerRegexes.Add(regex);
        }

        public bool RemoveTrigger(string trigger)
        {
            trigger.TryParseRegex(out Regex regex);
            return this.triggerRegexes.RemoveWhere(r => r.ToString() == regex.ToString()) > 0;
        }

        public bool IsMatch(string str)
            => !string.IsNullOrWhiteSpace(str) && this.triggerRegexes.Any(rgx => rgx.IsMatch(str));

        public bool ContainsTriggerPattern(string pattern)
            => !string.IsNullOrWhiteSpace(pattern) && this.TriggerStrings.Any(s => pattern == s);

        public bool HasSameResponseAs<T>(T other) where T : Reaction
            => this.Response == other.Response;

        public bool Equals(Reaction other)
            => this.HasSameResponseAs(other);
    }
}
