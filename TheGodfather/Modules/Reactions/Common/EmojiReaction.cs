namespace TheGodfather.Modules.Reactions.Common
{
    public class EmojiReaction : Reaction
    {
        public EmojiReaction(int id, string trigger, string reaction, bool is_regex_trigger = false)
            : base(id, trigger, reaction, is_regex_trigger) { }
    }
}
