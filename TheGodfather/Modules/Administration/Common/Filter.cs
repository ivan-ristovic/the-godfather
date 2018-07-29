#region USING_DIRECTIVES
using System.Text.RegularExpressions;
#endregion

namespace TheGodfather.Modules.Administration.Common
{
    public class Filter
    {
        public int Id { get; }
        public Regex Trigger { get; }

        public static string Wrap(string str)
            => $@"\b{str}\b";

        public static string Unwrap(string str)
            => str.Replace(@"\b", "");


        public Filter(int id, string trigger)
        {
            this.Id = id;
            this.Trigger = new Regex(Wrap(trigger));
        }

        public Filter(int id, Regex trigger)
        {
            this.Id = id;
            this.Trigger = trigger;
        }

        public string GetBaseRegexString() 
            => Unwrap(this.Trigger.ToString());
    }
}

