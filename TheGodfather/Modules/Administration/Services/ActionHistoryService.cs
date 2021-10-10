using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class ActionHistoryService : DbAbstractionServiceBase<ActionHistoryEntry, (ulong gid, ulong uid), DateTimeOffset>
    {
        private const int MaxEntriesPerUser = DiscordLimits.EmbedFieldLimit;

        public override bool IsDisabled => false;


        public ActionHistoryService(DbContextBuilder dbb)
            : base(dbb) { }

        
        public async Task LimitedAddAsync(ActionHistoryEntry entry)
        {
            await this.AddAsync(entry);

            IReadOnlyList<ActionHistoryEntry> entries = await this.GetAllAsync((entry.GuildId, entry.UserId));
            if (entries.Count > MaxEntriesPerUser) {
                var toRemove = entries
                    .OrderByDescending(e => e.Type)
                    .ThenByDescending(e => e.Time)
                    .Skip(MaxEntriesPerUser)
                    .ToList()
                    ;
                Log.Debug("Removing {Count} action history entries in guild {GuildId} for user {UserId}", entries.Count, entry.GuildId, entry.UserId);
                await this.RemoveAsync(toRemove);
            }
        }

        public async Task<IReadOnlyList<ActionHistoryEntry>> GetAllAsync(ulong gid)
        {
            List<ActionHistoryEntry> res;
            using (TheGodfatherDbContext db = this.dbb.CreateContext())
                res = await this.InternalGetEntriesForGuild(db, gid).ToListAsync();
            return res.AsReadOnly();
        }

        public Task<int> RemoveBeforeAsync(ulong gid, DateTimeOffset when)
            => this.InternalRemoveByPredicateAsync(gid, e => e.Time < when);

        public Task<int> RemoveAfterAsync(ulong gid, DateTimeOffset when)
            => this.InternalRemoveByPredicateAsync(gid, e => e.Time > when);

        public async Task ClearAsync(ulong gid)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            this.DbSetSelector(db).RemoveRange(this.DbSetSelector(db).AsQueryable().Where(e => e.GuildIdDb == (long)gid));
            await db.SaveChangesAsync();
        }

        public override DbSet<ActionHistoryEntry> DbSetSelector(TheGodfatherDbContext db)
            => db.ActionHistory;

        public override IQueryable<ActionHistoryEntry> GroupSelector(IQueryable<ActionHistoryEntry> ah, (ulong gid, ulong uid) grid)
            => ah.Where(e => e.GuildIdDb == (long)grid.gid && e.UserIdDb == (long)grid.uid);

        public override (ulong, ulong) EntityGroupSelector(ActionHistoryEntry entity)
            => (entity.GuildId, entity.UserId);

        public override ActionHistoryEntry EntityFactory((ulong gid, ulong uid) grid, DateTimeOffset id)
            => new ActionHistoryEntry { GuildId = grid.gid, UserId = grid.uid, Time = id };

        public override DateTimeOffset EntityIdSelector(ActionHistoryEntry entity)
            => entity.Time;

        public override object[] EntityPrimaryKeySelector((ulong gid, ulong uid) grid, DateTimeOffset id)
            => new object[] { (long)grid.gid, (long)grid.uid, id };


        private IQueryable<ActionHistoryEntry> InternalGetEntriesForGuild(TheGodfatherDbContext db, ulong gid)
            => this.DbSetSelector(db).AsQueryable().Where(n => n.GuildIdDb == (long)gid);

        private async Task<int> InternalRemoveByPredicateAsync(ulong gid, Func<ActionHistoryEntry, bool> predicate)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            var entries = this.InternalGetEntriesForGuild(db, gid)
                .AsEnumerable()
                .Where(predicate)
                .ToList();
            await this.RemoveAsync(entries);
            await db.SaveChangesAsync();
            return entries.Count;
        }
    }
}
