#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace TheGodfather.Modules.Games.Common
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
            if (statdict is null || !statdict.Any()) {
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


        public DiscordEmbedBuilder ToPartialDiscordEmbed()
        {
            var emb = new DiscordEmbedBuilder() {
                Color = DiscordColor.Chartreuse
            };
            emb.AddField("Duel stats", this.DuelStatsString())
               .AddField("Tic-Tac-Toe stats", this.TTTStatsString())
               .AddField("Connect4 stats", this.Chain4StatsString())
               .AddField("Caro stats", this.CaroStatsString())
               .AddField("Othello stats", this.OthelloStatsString())
               .AddField("Nunchi stats", this.NunchiStatsString(), inline: true)
               .AddField("Quiz stats", this.QuizStatsString(), inline: true)
               .AddField("Race stats", this.RaceStatsString(), inline: true)
               .AddField("Hangman stats", this.HangmanStatsString(), inline: true);
            return emb;
        }

        public DiscordEmbed ToDiscordEmbed(DiscordUser user)
        {
            DiscordEmbedBuilder emb = this.ToPartialDiscordEmbed();
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
            => $"W: {this.stats["duels_won"]} L: {this.stats["duels_lost"]} ({Formatter.Bold($"{this.CalculateWinPercentage(this.stats["duels_won"], this.stats["duels_lost"])}")}%)";

        public string TTTStatsString()
            => $"W: {this.stats["ttt_won"]} L: {this.stats["ttt_lost"]} ({Formatter.Bold($"{this.CalculateWinPercentage(this.stats["ttt_won"], this.stats["ttt_lost"])}")}%)";

        public string Chain4StatsString()
            => $"W: {this.stats["chain4_won"]} L: {this.stats["chain4_lost"]} ({Formatter.Bold($"{this.CalculateWinPercentage(this.stats["chain4_won"], this.stats["chain4_lost"])}")}%)";

        public string CaroStatsString()
            => $"W: {this.stats["caro_won"]} L: {this.stats["caro_lost"]} ({Formatter.Bold($"{this.CalculateWinPercentage(this.stats["caro_won"], this.stats["caro_lost"])}")}%)";

        public string NunchiStatsString()
            => $"W: {this.stats["numraces_won"]}";

        public string QuizStatsString()
            => $"W: {this.stats["quizes_won"]}";

        public string RaceStatsString()
            => $"W: {this.stats["races_won"]}";

        public string HangmanStatsString()
            => $"W: {this.stats["hangman_won"]}";

        public string OthelloStatsString()
            => $"W: {this.stats["othello_won"]} L: {this.stats["othello_lost"]} ({Formatter.Bold($"{this.CalculateWinPercentage(this.stats["othello_won"], this.stats["othello_lost"])}")}%)";
    }
}
