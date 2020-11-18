#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TheGodfather.Database.Entities;
#endregion

namespace TheGodfather.Modules.Games.Extensions
{
    public static class DatabaseGameStatsExtensions
    {
        public static async Task<string> BuildStatsStringAsync(DiscordClient client, 
            IReadOnlyList<DatabaseGameStats> top, Func<DatabaseGameStats, string> selector)
        {
            var sb = new StringBuilder();

            foreach (DatabaseGameStats userStats in top) {
                try {
                    DiscordUser u = await client.GetUserAsync(userStats.UserId);
                    sb.Append(u.Mention);
                    sb.Append(": ");
                } catch (NotFoundException) {
                    sb.Append("<unknown name>: ");
                }
                sb.Append(selector(userStats));
                sb.AppendLine();
            }

            return sb.ToString();
        }


        public static DiscordEmbedBuilder ToPartialDiscordEmbed(this DatabaseGameStats stats)
        {
            var emb = new DiscordEmbedBuilder {
                Color = DiscordColor.Chartreuse
            };
            emb.AddField("Duel stats", stats.BuildDuelStatsString())
               .AddField("Tic-Tac-Toe stats", stats.BuildTicTacToeStatsString())
               .AddField("Connect4 stats", stats.BuildChain4StatsString())
               .AddField("Caro stats", stats.BuildCaroStatsString())
               .AddField("Othello stats", stats.BuildOthelloStatsString())
               .AddField("Nunchi stats", stats.BuildNumberRaceStatsString(), inline: true)
               .AddField("Quiz stats", stats.BuildQuizStatsString(), inline: true)
               .AddField("Race stats", stats.BuildAnimalRaceStatsString(), inline: true)
               .AddField("Hangman stats", stats.BuildHangmanStatsString(), inline: true);
            return emb;
        }

        public static DiscordEmbed ToDiscordEmbed(this DatabaseGameStats stats, DiscordUser user)
        {
            DiscordEmbedBuilder emb = stats.ToPartialDiscordEmbed();
            emb.WithTitle($"Stats for {user.Username}");
            emb.WithThumbnail(user.AvatarUrl);
            return emb.Build();
        }

        public static string BuildDuelStatsString(this DatabaseGameStats stats)
            => $"W: {stats.DuelsWon} L: {stats.DuelsLost} ({Formatter.Bold($"{stats.CalculateWinPercentage(stats.DuelsWon, stats.DuelsLost)}")}%)";

        public static string BuildTicTacToeStatsString(this DatabaseGameStats stats)
            => $"W: {stats.TicTacToeWon} L: {stats.TicTacToeLost} ({Formatter.Bold($"{stats.CalculateWinPercentage(stats.TicTacToeWon, stats.TicTacToeLost)}")}%)";

        public static string BuildChain4StatsString(this DatabaseGameStats stats)
            => $"W: {stats.Chain4Won} L: {stats.Chain4Lost} ({Formatter.Bold($"{stats.CalculateWinPercentage(stats.Chain4Won, stats.Chain4Lost)}")}%)";

        public static string BuildCaroStatsString(this DatabaseGameStats stats)
            => $"W: {stats.CaroWon} L: {stats.CaroLost} ({Formatter.Bold($"{stats.CalculateWinPercentage(stats.CaroWon, stats.CaroLost)}")}%)";

        public static string BuildNumberRaceStatsString(this DatabaseGameStats stats)
            => $"W: {stats.NumberRacesWon}";

        public static string BuildQuizStatsString(this DatabaseGameStats stats)
            => $"W: {stats.QuizesWon}";

        public static string BuildAnimalRaceStatsString(this DatabaseGameStats stats)
            => $"W: {stats.AnimalRacesWon}";

        public static string BuildHangmanStatsString(this DatabaseGameStats stats)
            => $"W: {stats.HangmanWon}";

        public static string BuildOthelloStatsString(this DatabaseGameStats stats)
            => $"W: {stats.OthelloWon} L: {stats.OthelloLost} ({Formatter.Bold($"{stats.CalculateWinPercentage(stats.OthelloWon, stats.OthelloLost)}")}%)";
    }
}
