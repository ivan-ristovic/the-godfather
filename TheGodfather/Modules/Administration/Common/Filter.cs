#region USING_DIRECTIVES
using System.Text.RegularExpressions;
#endregion

namespace TheGodfather.Modules.Administration.Common
{
    public class Filter
    {
        public int Id { get; }
        public Regex Trigger { get; }


        public Filter(int id, string trigger)
        {
            this.Id = id;
            this.Trigger = new Regex($@"\b{trigger}\b");
        }

        public Filter(int id, Regex trigger)
        {
            this.Id = id;
            this.Trigger = trigger;
        }
    }
}

