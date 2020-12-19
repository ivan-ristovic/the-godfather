using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Misc.Services
{
    public sealed class UserRanksService : DbAbstractionServiceBase<XpCount, ulong>
    {
        public override bool IsDisabled => throw new NotImplementedException();

        private readonly ConcurrentDictionary<ulong, uint> xps;


        public UserRanksService(DbContextBuilder dbb)
            : base(dbb)
        {
            this.xps = new ConcurrentDictionary<ulong, uint>();
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
            this.xps.AddOrUpdate(uid, 1, (k, v) => v + change);

            short prev = CalculateRankForXp(this.xps[uid] - change);
            short curr = CalculateRankForXp(this.xps[uid]);

            return curr != prev ? curr : 0;
        }

        public bool Sync()
        {
            bool failed = true;
            try {
                using TheGodfatherDbContext db = this.dbb.CreateContext();
                foreach ((ulong uid, uint count) in this.xps) {
                    XpCount uxp = db.XpCounts.Find((long)uid);
                    if (uxp is null) {
                        db.XpCounts.Add(new XpCount {
                            Xp = count,
                            UserId = uid
                        });
                    } else {
                        uxp.Xp += count;
                        db.XpCounts.Update(uxp);
                    }
                }
                db.SaveChanges();
                failed = false;
            } finally {
                if (!failed)
                    this.xps.Clear();
            }
            return failed;
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
