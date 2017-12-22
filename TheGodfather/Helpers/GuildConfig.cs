#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
#endregion

namespace TheGodfather.Helpers
{
    public sealed class GuildConfig
    {

        [JsonProperty("Reactions")]
        public ConcurrentDictionary<string, string> Reactions { get; internal set; }
    }
}
