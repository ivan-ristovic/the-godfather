namespace TheGodfather.Modules.Reactions.Common
{
    public class EmojiReaction : Reaction
    {
        public EmojiReaction(int id, string trigger, string reaction, bool isRegex = false)
            : base(id, trigger, reaction, isRegex)
        {

        }
    }
}
