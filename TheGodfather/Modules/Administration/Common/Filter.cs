#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using TheGodfather.Common.Collections;
#endregion

namespace TheGodfather.Modules.Administration.Common
{
    public class Filter
    {
        public int Id { get; }
        public Regex Trigger { get; }


        public Filter(int id, string trigger)
        {
            Id = id;
            Trigger = new Regex($@"\b{trigger}\b");
        }

        public Filter(int id, Regex trigger)
        {
            Id = id;
            Trigger = trigger;
        }
    }
}

