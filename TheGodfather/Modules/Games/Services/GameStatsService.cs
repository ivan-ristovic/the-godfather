using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games.Services
{
    public sealed class GameStatsService : DbAbstractionServiceBase<GameStats, ulong>
    {
        public override bool IsDisabled => false;


        public GameStatsService(DbContextBuilder dbb)
            : base(dbb) { }


        public override DbSet<GameStats> DbSetSelector(TheGodfatherDbContext db) => db.GameStats;
        public override GameStats EntityFactory(ulong id) => new GameStats { UserId = id };
        public override ulong EntityIdSelector(GameStats entity) => entity.UserId;
        public override object[] EntityPrimaryKeySelector(ulong id) => new object[] { (long)id };

        public async Task UpdateStatsAsync(ulong uid, Action<GameStats> action)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
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

        public Task<IReadOnlyList<GameStats>> GetTopAnimalRaceStatsAsync(int amount = 10)
            => this.GetOrderedInternalAsync(amount, s => s.AnimalRacesWon);

        public Task<IReadOnlyList<GameStats>> GetTopCaroStatsAsync(int amount = 10)
            => this.GetOrderedInternalAsync(amount, s => GameStats.WinPercentage(s.CaroWon, s.CaroLost), s => s.CaroWon);

        public Task<IReadOnlyList<GameStats>> GetTopConnect4StatsAsync(int amount = 10)
            => this.GetOrderedInternalAsync(amount, s => GameStats.WinPercentage(s.Connect4Won, s.Connect4Lost), s => s.Connect4Won);

        public Task<IReadOnlyList<GameStats>> GetTopDuelStatsAsync(int amount = 10)
            => this.GetOrderedInternalAsync(amount, s => GameStats.WinPercentage(s.DuelsWon, s.DuelsLost), s => s.DuelsWon);

        public Task<IReadOnlyList<GameStats>> GetTopHangmanStatsAsync(int amount = 10)
            => this.GetOrderedInternalAsync(amount, s => s.HangmanWon);

        public Task<IReadOnlyList<GameStats>> GetTopNumberRaceStatsAsync(int amount = 10)
            => this.GetOrderedInternalAsync(amount, s => s.NumberRacesWon);

        public Task<IReadOnlyList<GameStats>> GetTopOthelloStatsAsync(int amount = 10)
            => this.GetOrderedInternalAsync(amount, s => GameStats.WinPercentage(s.OthelloWon, s.OthelloLost), s => s.OthelloWon);

        public Task<IReadOnlyList<GameStats>> GetTopQuizStatsAsync(int amount = 10)
            => this.GetOrderedInternalAsync(amount, s => s.QuizWon);

        public Task<IReadOnlyList<GameStats>> GetTopRussianRouletteStatsAsync(int amount = 10)
            => this.GetOrderedInternalAsync(amount, s => s.RussianRoulettesWon);

        public Task<IReadOnlyList<GameStats>> GetTopTicTacToeStatsAsync(int amount = 10)
            => this.GetOrderedInternalAsync(amount, s => GameStats.WinPercentage(s.TicTacToeWon, s.TicTacToeLost), s => s.TicTacToeWon);

        public Task<IReadOnlyList<GameStats>> GetTopTypingRaceStatsAsync(int amount = 10)
            => this.GetOrderedInternalAsync(amount, s => s.TypingRacesWon);


        private async Task<IReadOnlyList<GameStats>> GetOrderedInternalAsync<T>(int amount, Func<GameStats, T> orderBy, Func<GameStats, T>? thenBy = null)
        {
            // FIXME inefficient - try to use IQueryable with LINQ Expressions
            List<GameStats> top;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
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
