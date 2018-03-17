using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;

namespace TheGodfather.Modules.Reactions.Common
{
    public class EmojiReaction : Reaction
    {
        public DiscordEmoji Reaction { get; protected set; }


        public EmojiReaction(string trigger, DiscordEmoji reaction, bool is_regex_trigger = false)
        {
            AddTrigger(trigger, is_regex_trigger);
            Reaction = reaction;
        }


        public override bool HasSameResponseAs(Reaction other)
        {
            if (!(other is EmojiReaction er))
                return false;
            return Reaction == er.Reaction;
        }
    }
}
