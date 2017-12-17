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
    public static class GameStats
    {
        public static DiscordEmbedBuilder GetEmbeddedStats(IReadOnlyDictionary<string, string> stats)
        {
            var eb = new DiscordEmbedBuilder() { Color = DiscordColor.Chartreuse };
            eb.AddField("Duel stats", DuelStatsString(stats));
            eb.AddField("Tic-Tac-Toe stats", TTTStatsString(stats));
            eb.AddField("Connect4 stats", Connect4StatsString(stats));
            eb.AddField("Caro stats", CaroStatsString(stats));
            eb.AddField("Nunchi stats", NunchiStatsString(stats), inline: true);
            eb.AddField("Quiz stats", QuizStatsString(stats), inline: true);
            eb.AddField("Race stats", RaceStatsString(stats), inline: true);
            eb.AddField("Hangman stats", HangmanStatsString(stats), inline: true);
            return eb;
        }

        public static string DuelStatsString(IReadOnlyDictionary<string, string> stats)
            => $"W: {stats["duels_won"]} L: {stats["duels_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(stats["duels_won"], stats["duels_lost"])}")}%)";

        public static string TTTStatsString(IReadOnlyDictionary<string, string> stats)
            => $"W: {stats["ttt_won"]} L: {stats["ttt_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(stats["ttt_won"], stats["ttt_lost"])}")}%)";

        public static string Connect4StatsString(IReadOnlyDictionary<string, string> stats)
            => $"W: {stats["chain4_won"]} L: {stats["chain4_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(stats["chain4_won"], stats["chain4_lost"])}")}%)";

        public static string CaroStatsString(IReadOnlyDictionary<string, string> stats)
            => $"W: {stats["caro_won"]} L: {stats["caro_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(stats["caro_won"], stats["caro_lost"])}")}%)";

        public static string NunchiStatsString(IReadOnlyDictionary<string, string> stats)
            => $"W: {stats["nunchis_won"]}";

        public static string QuizStatsString(IReadOnlyDictionary<string, string> stats)
            => $"W: {stats["quizes_won"]}";

        public static string RaceStatsString(IReadOnlyDictionary<string, string> stats)
            => $"W: {stats["races_won"]}";

        public static string HangmanStatsString(IReadOnlyDictionary<string, string> stats)
            => $"W: {stats["hangman_won"]}";

        public static uint CalculateWinPercentage(string won, string lost)
        {
            int w, l;
            int.TryParse(won, out w);
            int.TryParse(lost, out l);

            if (w + l == 0)
                return 0;

            return (uint)Math.Round((double)w / (w + l) * 100);
        }
    }
}
