using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Misc.Services
{
    public sealed class UserRanksService : ITheGodfatherService
    {
        public bool IsDisabled => false;
        public ReadOnlyDictionary<ulong, uint> UserXP => new ReadOnlyDictionary<ulong, uint>(this.xps);


        private readonly ConcurrentDictionary<ulong, uint> xps;


        public UserRanksService()
        {
            this.xps = new ConcurrentDictionary<ulong, uint>();
        }


        public short CalculateRankForMessageCount(uint msgcount)
            => (short)Math.Floor(Math.Sqrt(msgcount / 10));

        public short CalculateRankForUser(ulong uid)
            => this.xps.TryGetValue(uid, out uint count) ? this.CalculateRankForMessageCount(count) : (short)0;

        public uint CalculateXpNeededForRank(short index)
            => (uint)(index * index * 10);

        public uint GetMessageCountForUser(ulong uid)
            => this.xps.TryGetValue(uid, out uint count) ? count : 0;

        public short IncrementMessageCountForUser(ulong uid)
        {
            this.xps.AddOrUpdate(uid, 1, (k, v) => v + 1);

            short prev = this.CalculateRankForMessageCount(this.xps[uid] - 1);
            short curr = this.CalculateRankForMessageCount(this.xps[uid]);

            return curr != prev ? curr : (short)0;
        }

        public void Sync(TheGodfatherDbContext db)
        {
            bool failed = true;
            try {
                foreach ((ulong uid, uint count) in this.UserXP) {
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
            } catch (Exception e) {
                throw e;
            } finally {
                if (!failed)
                    this.xps.Clear();
            }
        }
    }
}
