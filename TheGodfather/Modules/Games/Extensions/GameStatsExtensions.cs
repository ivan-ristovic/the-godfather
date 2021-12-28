using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace TheGodfather.Modules.Games.Extensions;

public static class GameStatsExtensions
{
    public static async Task<string> BuildStatsStringAsync(DiscordClient client, IReadOnlyList<GameStats> top, Func<GameStats, string> selector)
    {
        var sb = new StringBuilder();

        foreach (GameStats userStats in top) {
            try {
                DiscordUser u = await client.GetUserAsync(userStats.UserId);
                sb.Append(u.Mention);
                sb.Append(": ");
            } catch (NotFoundException) {
                sb.Append("<?>: ");
            }
            sb.Append(selector(userStats));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string BuildDuelStatsString(this GameStats s)
        => $"W: {s.DuelsWon} L: {s.DuelsLost} ({Formatter.Bold($"{GameStats.WinPercentage(s.DuelsWon, s.DuelsLost)}")}%)";

    public static string BuildTicTacToeStatsString(this GameStats s)
        => $"W: {s.TicTacToeWon} L: {s.TicTacToeLost} ({Formatter.Bold($"{GameStats.WinPercentage(s.TicTacToeWon, s.TicTacToeLost)}")}%)";

    public static string BuildConnect4StatsString(this GameStats s)
        => $"W: {s.Connect4Won} L: {s.Connect4Lost} ({Formatter.Bold($"{GameStats.WinPercentage(s.Connect4Won, s.Connect4Lost)}")}%)";

    public static string BuildCaroStatsString(this GameStats s)
        => $"W: {s.CaroWon} L: {s.CaroLost} ({Formatter.Bold($"{GameStats.WinPercentage(s.CaroWon, s.CaroLost)}")}%)";

    public static string BuildNumberRaceStatsString(this GameStats s)
        => $"W: {s.NumberRacesWon}";

    public static string BuildTypingRaceStatsString(this GameStats s)
        => $"W: {s.TypingRacesWon}";

    public static string BuildQuizStatsString(this GameStats s)
        => $"W: {s.QuizWon}";

    public static string BuildAnimalRaceStatsString(this GameStats s)
        => $"W: {s.AnimalRacesWon}";

    public static string BuildRussianRouletteStatsString(this GameStats s)
        => $"W: {s.RussianRoulettesWon}";

    public static string BuildHangmanStatsString(this GameStats s)
        => $"W: {s.HangmanWon}";

    public static string BuildOthelloStatsString(this GameStats s)
        => $"W: {s.OthelloWon} L: {s.OthelloLost} ({Formatter.Bold($"{GameStats.WinPercentage(s.OthelloWon, s.OthelloLost)}")}%)";
}