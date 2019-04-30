#region USING_DIRECTIVES
using System.Text.RegularExpressions;

using TheGodfather.Extensions;
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
            this.Trigger = trigger.CreateWordBoundaryRegex();
        }


        public string BaseRegexString => this.Trigger.ToString().RemoveWordBoundaryEscapes();
    }
}

