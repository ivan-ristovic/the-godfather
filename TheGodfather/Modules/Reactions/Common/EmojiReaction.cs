#region USING_DIRECTIVES
using System.Collections.Generic;
#endregion

namespace TheGodfather.Modules.Reactions.Common
{
    public class EmojiReaction : Reaction
    {
        public EmojiReaction(int id, string trigger, string reaction, bool isRegex = false)
            : base(id, trigger, reaction, isRegex)
        {

        }

        public EmojiReaction(int id, IEnumerable<string> triggers, string reaction, bool isRegex = false)
            : base(id, triggers, reaction, isRegex)
        {

        }
    }
}
