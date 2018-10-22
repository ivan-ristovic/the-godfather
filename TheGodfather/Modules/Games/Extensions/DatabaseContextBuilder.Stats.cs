#region USING_DIRECTIVES
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Database;
using TheGodfather.Database.Entities;
#endregion

namespace TheGodfather.Modules.Games.Extensions
{
    public static class DatabaseContextBuilderStatsExtensions
    {
        public static async Task UpdateStatsAsync(this DatabaseContextBuilder dbb, ulong uid, Action<DatabaseGameStats> action)
        {
            using (DatabaseContext db = dbb.CreateContext()) {
                DatabaseGameStats stats = await db.GameStats.FindAsync((long)uid);
                if (stats is null) {
                    stats = new DatabaseGameStats(uid);
                    action(stats);
                    db.GameStats.Add(stats);
                } else {
                    action(stats);
                    db.GameStats.Update(stats);
                }
                await db.SaveChangesAsync();
            }
        }

        public static Task<IReadOnlyList<DatabaseGameStats>> GetTopAnimalRaceStatsAsync(this DatabaseContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.AnimalRacesWon);

        public static Task<IReadOnlyList<DatabaseGameStats>> GetTopCaroStatsAsync(this DatabaseContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.CalculateWinPercentage(s.CaroWon, s.CaroLost), s => s.CaroWon);

        public static Task<IReadOnlyList<DatabaseGameStats>> GetTopChain4StatsAsync(this DatabaseContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.CalculateWinPercentage(s.Chain4Won, s.Chain4Lost), s => s.Chain4Won);

        public static Task<IReadOnlyList<DatabaseGameStats>> GetTopDuelStatsAsync(this DatabaseContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.CalculateWinPercentage(s.DuelsWon, s.DuelsLost), s => s.DuelsWon);

        public static Task<IReadOnlyList<DatabaseGameStats>> GetTopHangmanStatsAsync(this DatabaseContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.HangmanWon);

        public static Task<IReadOnlyList<DatabaseGameStats>> GetTopNumberRaceStatsAsync(this DatabaseContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.NumberRacesWon);

        public static Task<IReadOnlyList<DatabaseGameStats>> GetTopOthelloStatsAsync(this DatabaseContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.CalculateWinPercentage(s.OthelloWon, s.OthelloLost), s => s.OthelloWon);

        public static Task<IReadOnlyList<DatabaseGameStats>> GetTopQuizStatsAsync(this DatabaseContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.QuizesWon);

        public static Task<IReadOnlyList<DatabaseGameStats>> GetTopTicTacToeStatsAsync(this DatabaseContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.CalculateWinPercentage(s.TicTacToeWon, s.TicTacToeLost), s => s.TicTacToeWon);


        private static async Task<IReadOnlyList<DatabaseGameStats>> GetTopStatsCollectionInternalAsync(this DatabaseContextBuilder dbb, int amount,
            Func<DatabaseGameStats, int> orderBy, Func<DatabaseGameStats, int> thenBy = null)
        {
            List<DatabaseGameStats> top;
            using (DatabaseContext db = dbb.CreateContext()) {
                IOrderedEnumerable<DatabaseGameStats> topOrderedStats;
                if (thenBy is null) {
                    topOrderedStats = db.GameStats
                        .OrderByDescending(orderBy);
                } else {
                    topOrderedStats = db.GameStats
                        .OrderByDescending(orderBy)
                        .ThenByDescending(thenBy);
                }
                top = await topOrderedStats
                    .AsQueryable()
                    .Take(amount)
                    .ToListAsync();
            }

            return top.AsReadOnly();
        }
    }
}
