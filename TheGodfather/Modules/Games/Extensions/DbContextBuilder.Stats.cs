#region USING_DIRECTIVES
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Database.Models;
#endregion

namespace TheGodfather.Modules.Games.Extensions
{
    public static class DbContextBuilderStatsExtensions
    {
        public static async Task UpdateStatsAsync(this DbContextBuilder dbb, ulong uid, Action<GameStats> action)
        {
            using (TheGodfatherDbContext db = dbb.CreateDbContext()) {
                GameStats stats = await db.GameStats.FindAsync((long)uid);
                if (stats is null) {
                    stats = new GameStats { UserId = uid };
                    action(stats);
                    db.GameStats.Add(stats);
                } else {
                    action(stats);
                    db.GameStats.Update(stats);
                }
                await db.SaveChangesAsync();
            }
        }

        public static Task<IReadOnlyList<GameStats>> GetTopAnimalRaceStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.AnimalRacesWon);

        public static Task<IReadOnlyList<GameStats>> GetTopCaroStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.CalculateWinPercentage(s.CaroWon, s.CaroLost), s => s.CaroWon);

        public static Task<IReadOnlyList<GameStats>> GetTopChain4StatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.CalculateWinPercentage(s.Chain4Won, s.Chain4Lost), s => s.Chain4Won);

        public static Task<IReadOnlyList<GameStats>> GetTopDuelStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.CalculateWinPercentage(s.DuelWon, s.DuelLost), s => s.DuelWon);

        public static Task<IReadOnlyList<GameStats>> GetTopHangmanStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.HangmanWon);

        public static Task<IReadOnlyList<GameStats>> GetTopNumberRaceStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.NumberRacesWon);

        public static Task<IReadOnlyList<GameStats>> GetTopOthelloStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.CalculateWinPercentage(s.OthelloWon, s.OthelloLost), s => s.OthelloWon);

        public static Task<IReadOnlyList<GameStats>> GetTopQuizStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.QuizWon);

        public static Task<IReadOnlyList<GameStats>> GetTopTicTacToeStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.CalculateWinPercentage(s.TicTacToeWon, s.TicTacToeLost), s => s.TicTacToeWon);


        private static async Task<IReadOnlyList<GameStats>> GetTopStatsCollectionInternalAsync(this DbContextBuilder dbb, int amount,
            Func<GameStats, int> orderBy, Func<GameStats, int> thenBy = null)
        {
            List<GameStats> top;
            using (TheGodfatherDbContext db = dbb.CreateDbContext()) {
                IOrderedEnumerable<GameStats> topOrderedStats = thenBy is null
                    ? db.GameStats
                        .OrderByDescending(orderBy)
                    : db.GameStats
                        .OrderByDescending(orderBy)
                        .ThenByDescending(thenBy);
                top = await topOrderedStats
                    .AsQueryable()
                    .Take(amount)
                    .ToListAsync();
            }

            return top.AsReadOnly();
        }
    }
}
