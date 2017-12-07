#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers
{
    public sealed class GameStats
    {
        [JsonProperty("duelswon")]
        public uint DuelsWon { get; internal set; }

        [JsonProperty("duelslost")]
        public uint DuelsLost { get; internal set; }

        [JsonIgnore]
        public uint DuelWinPercentage
        {
            get {
                if (DuelsWon + DuelsLost == 0)
                    return 0;
                return (uint)Math.Round((double)DuelsWon / (DuelsWon + DuelsLost) * 100);
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
        public uint TTTWinPercentage
        {
            get {
                if (TTTWon + TTTLost == 0)
                    return 0;
                return (uint)Math.Round((double)TTTWon / (TTTWon + TTTLost) * 100);
            }
            internal set { }
        }
        
        [JsonProperty("c4won")]
        public uint Connect4Won { get; internal set; }

        [JsonProperty("c4lost")]
        public uint Connect4Lost { get; internal set; }

        [JsonIgnore]
        public uint Connect4WinPercentage
        {
            get {
                if (Connect4Won + Connect4Lost == 0)
                    return 0;
                return (uint)Math.Round((double)Connect4Won / (Connect4Won + Connect4Lost) * 100);
            }
            internal set { }
        }

        [JsonProperty("carowon")]
        public uint CaroWon { get; internal set; }

        [JsonProperty("carolost")]
        public uint CaroLost { get; internal set; }

        [JsonIgnore]
        public uint CaroWinPercentage
        {
            get {
                if (CaroWon + CaroLost == 0)
                    return 0;
                return (uint)Math.Round((double)CaroWon / (CaroWon + CaroLost) * 100);
            }
            internal set { }
        }


        public string DuelStatsString()
            => $"W: {DuelsWon} L: {DuelsLost} ({Formatter.Bold($"{DuelWinPercentage}")}%)";

        public string TTTStatsString()
            => $"W: {TTTWon} L: {TTTLost} ({Formatter.Bold($"{TTTWinPercentage}")}%)";

        public string Connect4StatsString()
            => $"W: {Connect4Won} L: {Connect4Lost} ({Formatter.Bold($"{Connect4WinPercentage}")}%)";

        public string CaroStatsString()
            => $"W: {CaroWon} L: {CaroLost} ({Formatter.Bold($"{CaroWinPercentage}")}%)";

        public string NunchiStatsString() 
            => $"W: {NunchiGamesWon}";

        public string QuizStatsString() 
            => $"W: {QuizesWon}";

        public string RaceStatsString() 
            => $"W: {RacesWon}";

        public string HangmanStatsString() 
            => $"W: {HangmanWon}";


        public DiscordEmbedBuilder GetEmbeddedStats()
        {
            var eb = new DiscordEmbedBuilder() { Color = DiscordColor.Chartreuse };
            eb.AddField("Duel stats", DuelStatsString());
            eb.AddField("Tic-Tac-Toe stats", TTTStatsString());
            eb.AddField("Connect4 stats", Connect4StatsString());
            eb.AddField("Caro stats", CaroStatsString());
            eb.AddField("Nunchi stats", NunchiStatsString(), inline: true);
            eb.AddField("Quiz stats", QuizStatsString(), inline: true);
            eb.AddField("Race stats", RaceStatsString(), inline: true);
            eb.AddField("Hangman stats", HangmanStatsString(), inline: true);
            return eb;
        }

    }
}
