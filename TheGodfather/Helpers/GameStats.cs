#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
#endregion

namespace TheGodfather.Helpers
{
    public class GameStats
    {
        [JsonProperty("duelswon")]
        public uint DuelsWon { get; internal set; }

        [JsonProperty("duelslost")]
        public uint DuelsLost { get; internal set; }
    }
}
