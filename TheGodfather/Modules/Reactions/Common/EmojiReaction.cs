namespace TheGodfather.Modules.Reactions.Common
{
    public class EmojiReaction : Reaction
    {
        public EmojiReaction(string trigger, string reaction, bool is_regex_trigger = false)
            : base(trigger, reaction, is_regex_trigger) { }
    }
}
