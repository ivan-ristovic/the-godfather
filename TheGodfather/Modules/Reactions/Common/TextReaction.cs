namespace TheGodfather.Modules.Reactions.Common
{
    public class TextReaction : Reaction
    {
        public TextReaction(string trigger, string response, bool is_regex_trigger = false)
            : base(trigger, response, is_regex_trigger) { }
    }
}
