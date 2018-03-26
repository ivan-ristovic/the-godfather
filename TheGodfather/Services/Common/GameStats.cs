#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Services.Common
{
    public class GameStats
    {
        private IReadOnlyDictionary<string, string> Stats { get; }
        public ulong UserId => ulong.TryParse(Stats["uid"], out ulong uid) ? uid : 0;


        public GameStats(IReadOnlyDictionary<string, string> statdict)
        {
            if (statdict == null || !statdict.Any()) {
                Stats = new Dictionary<string, string>() {
                    { "duels_won" , "0" },
                    { "duels_lost" , "0" },
                    { "ttt_won" , "0" },
                    { "ttt_lost" , "0" },
                    { "chain4_won" , "0" },
                    { "chain4_lost" , "0" },
                    { "caro_won" , "0" },
                    { "caro_lost" , "0" },
                    { "numraces_won" , "0" },
                    { "quizes_won" , "0" },
                    { "races_won" , "0" },
                    { "hangman_won" , "0" }
                };
            } else {
                Stats = statdict;
            }
        }


        public DiscordEmbedBuilder GetEmbedBuilder()
        {
            var emb = new DiscordEmbedBuilder() {
                Color = DiscordColor.Chartreuse
            };
            emb.AddField("Duel stats", DuelStatsString())
               .AddField("Tic-Tac-Toe stats", TTTStatsString())
               .AddField("Connect4 stats", Chain4StatsString())
               .AddField("Caro stats", CaroStatsString())
               .AddField("Othello stats", OthelloStatsString())
               .AddField("Nunchi stats", NunchiStatsString(), inline: true)
               .AddField("Quiz stats", QuizStatsString(), inline: true)
               .AddField("Race stats", RaceStatsString(), inline: true)
               .AddField("Hangman stats", HangmanStatsString(), inline: true);
            return emb;
        }

        public string DuelStatsString()
            => $"W: {Stats["duels_won"]} L: {Stats["duels_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(Stats["duels_won"], Stats["duels_lost"])}")}%)";

        public string TTTStatsString()
            => $"W: {Stats["ttt_won"]} L: {Stats["ttt_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(Stats["ttt_won"], Stats["ttt_lost"])}")}%)";

        public string Chain4StatsString()
            => $"W: {Stats["chain4_won"]} L: {Stats["chain4_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(Stats["chain4_won"], Stats["chain4_lost"])}")}%)";

        public string CaroStatsString()
            => $"W: {Stats["caro_won"]} L: {Stats["caro_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(Stats["caro_won"], Stats["caro_lost"])}")}%)";

        public string NunchiStatsString()
            => $"W: {Stats["numraces_won"]}";

        public string QuizStatsString()
            => $"W: {Stats["quizes_won"]}";

        public string RaceStatsString()
            => $"W: {Stats["races_won"]}";

        public string HangmanStatsString()
            => $"W: {Stats["hangman_won"]}";

        public string OthelloStatsString()
            => $"W: {Stats["othello_won"]} L: {Stats["othello_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(Stats["othello_won"], Stats["othello_lost"])}")}%)";

        public uint CalculateWinPercentage(string won, string lost)
        {
            int.TryParse(won, out int w);
            int.TryParse(lost, out int l);

            if (w + l == 0)
                return 0;

            return (uint)Math.Round((double)w / (w + l) * 100);
        }
    }
}
