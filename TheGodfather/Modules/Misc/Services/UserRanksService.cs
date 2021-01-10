using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Misc.Services
{
    public sealed class UserRanksService : DbAbstractionServiceBase<XpCount, ulong>
    {
        public override bool IsDisabled => false;

        private ConcurrentDictionary<ulong, uint> xps;
        private readonly ConcurrentHashSet<ulong> modified;


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
            this.xps = new(db.XpCounts.ToDictionary(xp => xp.UserId, xp => xp.Xp));
        }

        public static short CalculateRankForXp(uint msgcount)
            => (short)Math.Floor(Math.Sqrt(msgcount / 10));

        public static uint CalculateXpNeededForRank(short index)
            => (uint)(index * index * 10);

        public short CalculateRankForUser(ulong uid)
            => this.xps.TryGetValue(uid, out uint count) ? CalculateRankForXp(count) : 0;

        public uint GetUserXp(ulong uid)
            => this.xps.TryGetValue(uid, out uint count) ? count : 0;

        public short ChangeXp(ulong uid, uint change = 1)
        {
            this.xps.AddOrUpdate(uid, 1, (k, xp) => xp + change);
            this.modified.Add(uid);

            short prev = CalculateRankForXp(this.xps[uid] - change);
            short curr = CalculateRankForXp(this.xps[uid]);

            return curr != prev ? curr : 0;
        }

        public async Task Sync()
        {
            if (this.modified.Any()) {
                using TheGodfatherDbContext db = this.dbb.CreateContext();
                foreach (ulong uid in this.modified) {
                    XpCount? xpc = await db.XpCounts.FindAsync((long)uid);
                    if (xpc is null) {
                        db.XpCounts.Add(new XpCount { UserId = uid, Xp = this.xps.GetValueOrDefault(uid) });
                    } else {
                        xpc.Xp = this.xps.GetValueOrDefault(uid);
                        db.XpCounts.Update(xpc);
                    }
                }
                await db.SaveChangesAsync();
                this.modified.Clear();
            }
        }

        public async Task<XpRank?> FindRankAsync(ulong gid)
        {
            XpRank? rank = null;
            using (TheGodfatherDbContext db = this.dbb.CreateContext())
                rank = await db.XpRanks.FindAsync((long)gid, rank);
            return rank;
        }

        public async Task<IReadOnlyList<XpCount>> GetTopRankedUsersAsync(int count = 10)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            return await this.DbSetSelector(db).OrderByDescending(r => r.Xp).Take(count).ToListAsync();
        }

        public override DbSet<XpCount> DbSetSelector(TheGodfatherDbContext db) => db.XpCounts;
        public override XpCount EntityFactory(ulong id) => new XpCount { UserId = id };
        public override ulong EntityIdSelector(XpCount entity) => entity.UserId;
        public override object[] EntityPrimaryKeySelector(ulong id) => new object[] { id };
    }
}
