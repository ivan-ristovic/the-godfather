using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Owner.Services
{
    public sealed class BlockingService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        public IReadOnlyList<ulong> BlockedChannels => this.bChannels.ToList().AsReadOnly();
        public IReadOnlyList<ulong> BlockedUsers => this.bUsers.ToList().AsReadOnly();

        private ConcurrentHashSet<ulong> bChannels;
        private ConcurrentHashSet<ulong> bUsers;
        private readonly DbContextBuilder dbb;


        public BlockingService(DbContextBuilder dbb, bool loadData = true)
        {
            this.dbb = dbb;
            this.bChannels = new ConcurrentHashSet<ulong>();
            this.bUsers = new ConcurrentHashSet<ulong>();
            if (loadData)
                this.LoadData();
        }


        public void LoadData()
        {
            Log.Debug("Loading blocked entities");
            try {
                using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                    this.bChannels = new ConcurrentHashSet<ulong>(db.BlockedChannels.Select(c => c.ChannelId));
                    this.bUsers = new ConcurrentHashSet<ulong>(db.BlockedUsers.Select(u => u.UserId));
                }
            } catch (Exception e) {
                Log.Error(e, "Loading blocked entities failed");
            }
        }

        public void Sync()
        {
            this.bUsers.Clear();
            this.bChannels.Clear();
            this.LoadData();
        }

        public bool IsChannelBlocked(ulong cid)

            => this.bChannels.Contains(cid);

        public bool IsUserBlocked(ulong uid)
            => this.bUsers.Contains(uid);

        public bool IsBlocked(ulong cid, ulong uid)
            => this.IsChannelBlocked(cid) || this.IsUserBlocked(uid);

        public async Task<IReadOnlyList<BlockedChannel>> GetBlockedChannelsAsync()
        {
            List<BlockedChannel> blocked;
            using (TheGodfatherDbContext db = this.dbb.CreateContext())
                blocked = await db.BlockedChannels.ToListAsync();
            return blocked.AsReadOnly();
        }

        public async Task<IReadOnlyList<BlockedUser>> GetBlockedUsersAsync()
        {
            List<BlockedUser> blocked;
            using (TheGodfatherDbContext db = this.dbb.CreateContext())
                blocked = await db.BlockedUsers.ToListAsync();
            return blocked.AsReadOnly();
        }

        public async Task<bool> BlockChannelAsync(ulong cid, string? reason = null)
        {
            if (!this.bChannels.Add(cid))
                return false;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.BlockedChannels.Add(new BlockedChannel {
                    ChannelId = cid,
                    Reason = reason
                });
                await db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> BlockChannelsAsync(IEnumerable<ulong> cids, string? reason = null)
        {
            if (!cids.Any())
                return 0;

            int blocked = 0;

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                foreach (ulong cid in cids) {
                    if (!this.bChannels.Add(cid))
                        continue;
                    db.BlockedChannels.Add(new BlockedChannel {
                        ChannelId = cid,
                        Reason = reason
                    });
                    blocked++;
                }
                await db.SaveChangesAsync();
            }

            return blocked;
        }

        public async Task<bool> BlockUserAsync(ulong uid, string? reason = null)
        {
            if (!this.bUsers.Add(uid))
                return false;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.BlockedUsers.Add(new BlockedUser {
                    UserId = uid,
                    Reason = reason
                });
                await db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> BlockUsersAsync(IEnumerable<ulong> uids, string? reason = null)
        {
            if (!uids.Any())
                return 0;

            int blocked = 0;

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                foreach (ulong uid in uids) {
                    if (!this.bUsers.Add(uid))
                        continue;
                    db.BlockedUsers.Add(new BlockedUser {
                        UserId = uid,
                        Reason = reason
                    });
                    blocked++;
                }
                await db.SaveChangesAsync();
            }

            return blocked;
        }

        public async Task<bool> UnblockChannelAsync(ulong cid)
        {
            if (!this.bChannels.TryRemove(cid))
                return false;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.BlockedChannels.Remove(new BlockedChannel { ChannelId = cid });
                await db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> UnblockChannelsAsync(IEnumerable<ulong> cids)
        {
            if (!cids.Any())
                return 0;

            int unblocked = 0;

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                foreach (ulong cid in cids) {
                    if (!this.bChannels.TryRemove(cid))
                        continue;
                    db.BlockedChannels.Remove(new BlockedChannel { ChannelId = cid });
                    unblocked++;
                }
                await db.SaveChangesAsync();
            }

            return unblocked;
        }

        public async Task<bool> UnblockUserAsync(ulong uid)
        {
            if (!this.bUsers.TryRemove(uid))
                return false;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.BlockedUsers.Remove(new BlockedUser { UserId = uid });
                await db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> UnblockUsersAsync(IEnumerable<ulong> uids)
        {
            if (!uids.Any())
                return 0;

            int unblocked = 0;

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                foreach (ulong uid in uids) {
                    if (!this.bUsers.TryRemove(uid))
                        continue;
                    db.BlockedUsers.Remove(new BlockedUser { UserId = uid });
                    unblocked++;
                }
                await db.SaveChangesAsync();
            }

            return unblocked;
        }
    }
}
