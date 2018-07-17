namespace TheGodfather.Modules.Reactions.Common
{
    public class EmojiReaction : Reaction
    {
        public EmojiReaction(int id, string trigger, string reaction, bool regex = false)
            : base(id, trigger, reaction, regex)
        {

        }
    }
}
