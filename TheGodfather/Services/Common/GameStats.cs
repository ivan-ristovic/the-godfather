#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Services.Common
{
    public enum GameStatsType
    {
        AnimalRacesWon,
        CarosWon,
        CarosLost,
        Connect4sWon,
        Connect4sLost,
        DuelsWon,
        DuelsLost,
        HangmansWon,
        NumberRacesWon,
        OthellosWon,
        OthellosLost,
        QuizesWon,
        TicTacToesWon,
        TicTacToesLost
    }

    public class GameStats
    {
        private readonly IReadOnlyDictionary<string, string> stats;

        public ulong UserId 
            => ulong.TryParse(this.stats["uid"], out ulong uid) ? uid : 0;


        public GameStats(IReadOnlyDictionary<string, string> statdict)
        {
            if (statdict == null || !statdict.Any()) {
                this.stats = new Dictionary<string, string>() {
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
                this.stats = statdict;
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

        public DiscordEmbed ToDiscordEmbed(DiscordUser user)
        {
            DiscordEmbedBuilder emb = this.GetEmbedBuilder();
            emb.WithTitle($"Stats for {user.Username}");
            emb.WithThumbnailUrl(user.AvatarUrl);
            return emb.Build();
        }

        public uint CalculateWinPercentage(string won, string lost)
        {
            int.TryParse(won, out int w);
            int.TryParse(lost, out int l);

            if (w + l == 0)
                return 0;

            return (uint)Math.Round((double)w / (w + l) * 100);
        }

        public string DuelStatsString()
            => $"W: {this.stats["duels_won"]} L: {this.stats["duels_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(this.stats["duels_won"], this.stats["duels_lost"])}")}%)";

        public string TTTStatsString()
            => $"W: {this.stats["ttt_won"]} L: {this.stats["ttt_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(this.stats["ttt_won"], this.stats["ttt_lost"])}")}%)";

        public string Chain4StatsString()
            => $"W: {this.stats["chain4_won"]} L: {this.stats["chain4_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(this.stats["chain4_won"], this.stats["chain4_lost"])}")}%)";

        public string CaroStatsString()
            => $"W: {this.stats["caro_won"]} L: {this.stats["caro_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(this.stats["caro_won"], this.stats["caro_lost"])}")}%)";

        public string NunchiStatsString()
            => $"W: {this.stats["numraces_won"]}";

        public string QuizStatsString()
            => $"W: {this.stats["quizes_won"]}";

        public string RaceStatsString()
            => $"W: {this.stats["races_won"]}";

        public string HangmanStatsString()
            => $"W: {this.stats["hangman_won"]}";

        public string OthelloStatsString()
            => $"W: {this.stats["othello_won"]} L: {this.stats["othello_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(this.stats["othello_won"], this.stats["othello_lost"])}")}%)";
    }
}
