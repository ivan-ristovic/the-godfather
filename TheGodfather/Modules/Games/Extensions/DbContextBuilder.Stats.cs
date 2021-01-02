#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
#endregion

namespace TheGodfather.Modules.Games.Extensions
{
    public static class DbContextBuilderStatsExtensions
    {
        [Obsolete]
        public static async Task UpdateStatsAsync(this DbContextBuilder dbb, ulong uid, Action<GameStats> action)
        {
            using TheGodfatherDbContext db = dbb.CreateContext();
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

        [Obsolete]
        public static Task<IReadOnlyList<GameStats>> GetTopAnimalRaceStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.AnimalRacesWon);

        [Obsolete]
        public static Task<IReadOnlyList<GameStats>> GetTopCaroStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => GameStats.WinPercentage(s.CaroWon, s.CaroLost), s => s.CaroWon);

        [Obsolete]
        public static Task<IReadOnlyList<GameStats>> GetTopChain4StatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => GameStats.WinPercentage(s.Chain4Won, s.Chain4Lost), s => s.Chain4Won);

        [Obsolete]
        public static Task<IReadOnlyList<GameStats>> GetTopDuelStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => GameStats.WinPercentage(s.DuelWon, s.DuelLost), s => s.DuelWon);

        [Obsolete]
        public static Task<IReadOnlyList<GameStats>> GetTopHangmanStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.HangmanWon);

        [Obsolete]
        public static Task<IReadOnlyList<GameStats>> GetTopNumberRaceStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.NumberRacesWon);

        [Obsolete]
        public static Task<IReadOnlyList<GameStats>> GetTopOthelloStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => GameStats.WinPercentage(s.OthelloWon, s.OthelloLost), s => s.OthelloWon);

        [Obsolete]
        public static Task<IReadOnlyList<GameStats>> GetTopQuizStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => s.QuizWon);

        [Obsolete]
        public static Task<IReadOnlyList<GameStats>> GetTopTicTacToeStatsAsync(this DbContextBuilder dbb, int amount = 10)
            => dbb.GetTopStatsCollectionInternalAsync(amount, s => GameStats.WinPercentage(s.TicTacToeWon, s.TicTacToeLost), s => s.TicTacToeWon);


        [Obsolete]
        private static async Task<IReadOnlyList<GameStats>> GetTopStatsCollectionInternalAsync(this DbContextBuilder dbb, int amount,
            Func<GameStats, int> orderBy, Func<GameStats, int> thenBy = null)
        {
            List<GameStats> top;
            using (TheGodfatherDbContext db = dbb.CreateContext()) {
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
