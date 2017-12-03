#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers
{
    public class GameStats
    {
        [JsonProperty("duelswon")]
        public uint DuelsWon { get; internal set; }

        [JsonProperty("duelslost")]
        public uint DuelsLost { get; internal set; }

        [JsonIgnore]
        public int DuelWinPercentage {
            get {
                if (DuelsWon + DuelsLost == 0)
                    return 0;
                return (int)Math.Round((double)DuelsWon / (DuelsWon + DuelsLost) * 100);
            }
            internal set { }
        }

        [JsonProperty("hangmanwon")]
        public uint HangmanWon { get; internal set; }

        [JsonProperty("nunchiswon")]
        public uint NunchiGamesWon { get; internal set; }

        [JsonProperty("quizeswon")]
        public uint QuizesWon { get; internal set; }

        [JsonProperty("raceswon")]
        public uint RacesWon { get; internal set; }

        [JsonProperty("tttwon")]
        public uint TTTWon { get; internal set; }

        [JsonProperty("tttlost")]
        public uint TTTLost { get; internal set; }

        [JsonIgnore]
        public int TTTWinPercentage
        {
            get {
                if (TTTWon + TTTLost == 0)
                    return 0;
                return (int)Math.Round((double)TTTWon / (TTTWon + TTTLost) * 100);
            }
            internal set { }
        }
    }
}
