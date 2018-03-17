using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TheGodfather.Modules.Reactions.Common
{
    public abstract class Reaction
    {
        public Regex TriggerRegex { get; protected set; }
        public string TriggerString { get; protected set; }


        protected Reaction(string trigger, bool is_regex_trigger = false)
        {
            TriggerString = trigger.ToLowerInvariant();
            if (is_regex_trigger)
                TriggerRegex = new Regex($@"\b({TriggerString})\b", RegexOptions.IgnoreCase);
            else
                TriggerRegex = new Regex(Regex.Escape(TriggerString));
        }


        public bool EqualsToString(string pattern)
        {
            return TriggerString.Equals($@"\b{pattern.ToLowerInvariant()}\b");
        }
    }
}
