using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Misc.Services
{
    public sealed class UserRanksService : DbAbstractionServiceBase<XpCount, ulong, ulong>
    {
        public override bool IsDisabled => false;

        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, int>> xps;
        private readonly ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>> modified;


        public UserRanksService(DbContextBuilder dbb, bool loadData = true)
            : base(dbb)
        {
            this.xps = new();
            this.modified = new();
            if (loadData)
                this.LoadData();
        }


        public void LoadData()
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            this.xps = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, int>>(
                db.XpCounts
                    .AsEnumerable()
                    .GroupBy(xpc => xpc.GuildId)
                    .ToDictionary(g => g.Key, g => new ConcurrentDictionary<ulong, int>(g.ToDictionary(g1 => g1.UserId, g1 => g1.Xp)))
            );
        }

        public static short CalculateRankForXp(int msgcount)
            => (short)Math.Floor(Math.Sqrt(msgcount / 10));

        public static int CalculateXpNeededForRank(short index)
            => index * index * 10;

        public short CalculateRankForUser(ulong gid, ulong uid)
            => this.xps.GetOrAdd(gid, new ConcurrentDictionary<ulong, int>()).TryGetValue(uid, out int count) ? CalculateRankForXp(count) : 0;

        public int GetUserXp(ulong? gid, ulong uid)
        {
            if (gid is { })
                return this.xps.GetOrAdd(gid.Value, new ConcurrentDictionary<ulong, int>()).TryGetValue(uid, out int count) ? count : 0;
            else
                return this.xps.Sum(kvp => kvp.Value.GetValueOrDefault(uid));

        }

        public short ChangeXp(ulong gid, ulong uid, int change = 1)
        {
            this.xps.GetOrAdd(gid, new ConcurrentDictionary<ulong, int>()).AddOrUpdate(uid, 1, (k, xp) => xp + change);
            this.modified.GetOrAdd(gid, new ConcurrentHashSet<ulong>()).Add(uid);

            short prev = CalculateRankForXp(this.xps[gid][uid] - change);
            short curr = CalculateRankForXp(this.xps[gid][uid]);

            return curr != prev ? curr : 0;
        }

        public async Task Sync()
        {
            if (this.modified.Any()) {
                using TheGodfatherDbContext db = this.dbb.CreateContext();
                foreach ((ulong gid, ConcurrentHashSet<ulong> uids) in this.modified) {
                    foreach (ulong uid in uids) {
                        XpCount? xpc = await db.XpCounts.FindAsync((long)gid, (long)uid);
                        if (xpc is null) {
                            db.XpCounts.Add(new XpCount { GuildId = gid, UserId = uid, Xp = this.xps[gid].GetValueOrDefault(uid) });
                        } else {
                            xpc.Xp = this.xps[gid].GetValueOrDefault(uid);
                            db.XpCounts.Update(xpc);
                        }
                    }
                    uids.Clear();
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task<IReadOnlyList<XpCount>> GetTopRankedUsersAsync(ulong? gid = null, int count = 10)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            return gid is null
                ? await this.DbSetSelector(db).AsQueryable().OrderByDescending(r => r.Xp).Take(count).ToListAsync()
                : await this.GroupSelector(this.DbSetSelector(db), gid.Value).OrderByDescending(r => r.Xp).Take(count).ToListAsync();
        }

        public async Task RemoveDeletedUsers(IEnumerable<ulong> uids)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            foreach (ulong uid in uids)
                this.DbSetSelector(db).RemoveRange(this.DbSetSelector(db).AsQueryable().Where(xpc => xpc.UserIdDb == (long)uid));
            await db.SaveChangesAsync();
        }

        public override DbSet<XpCount> DbSetSelector(TheGodfatherDbContext db) => db.XpCounts;
        public override XpCount EntityFactory(ulong gid, ulong uid) => new XpCount { GuildId = gid, UserId = uid };
        public override ulong EntityIdSelector(XpCount entity) => entity.UserId;
        public override object[] EntityPrimaryKeySelector(ulong gid, ulong uid) => new object[] { (long)gid, (long)uid };
        public override IQueryable<XpCount> GroupSelector(IQueryable<XpCount> entities, ulong gid) => entities.Where(xpc => xpc.GuildIdDb == (long)gid);
        public override ulong EntityGroupSelector(XpCount entity) => entity.GuildId;
    }
}
