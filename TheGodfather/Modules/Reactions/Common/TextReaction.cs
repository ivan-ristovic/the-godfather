namespace TheGodfather.Modules.Reactions.Common
{
    public class TextReaction : Reaction
    {
        public string Response { get; protected set; }


        public TextReaction(string trigger, string response, bool is_regex_trigger = false)
        {
            AddTrigger(trigger, is_regex_trigger);
            Response = response;
        }


        public override bool HasSameResponseAs(Reaction other)
        {
            if (!(other is TextReaction tr))
                return false;
            return Response == tr.Response;
        }
    }
}
