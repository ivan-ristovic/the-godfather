using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TheGodfather.Modules.Reactions.Common
{
    public class TextReaction : Reaction
    {
        public string Response { get; protected set; }


        public TextReaction(string trigger, string response, bool is_regex_trigger = false)
            : base(trigger, is_regex_trigger)
        {
            Response = response;
        }
    }
}
