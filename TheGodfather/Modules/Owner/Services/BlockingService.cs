using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common.Collections;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
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
        private readonly DatabaseContextBuilder dbb;


        public BlockingService(DatabaseContextBuilder dbb, bool loadData = true)
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
                using (DatabaseContext db = this.dbb.CreateContext()) {
                    this.bChannels = new ConcurrentHashSet<ulong>(db.BlockedChannels.Select(c => c.ChannelId));
                    this.bUsers = new ConcurrentHashSet<ulong>(db.BlockedUsers.Select(u => u.UserId));
                }
            } catch (Exception e) {
                Log.Error(e, "Loading blocked entities failed");
            }
        }

        public bool IsChannelBlocked(ulong cid)
        {
            return false;
        }

        public bool IsUserBlocked(ulong uid)
        {
            return false;
        }

        public bool IsBlocked(ulong cid, ulong uid)
            => this.IsChannelBlocked(cid) || this.IsUserBlocked(uid);

        public async Task<IReadOnlyList<DatabaseBlockedChannel>> GetBlockedChannels()
        {
            List<DatabaseBlockedChannel> blocked;
            using (DatabaseContext db = this.dbb.CreateContext())
                blocked = await db.BlockedChannels.ToListAsync();
            return blocked.AsReadOnly();
        }

        public async Task<IReadOnlyList<DatabaseBlockedUser>> GetBlockedUsers()
        {
            List<DatabaseBlockedUser> blocked;
            using (DatabaseContext db = this.dbb.CreateContext())
                blocked = await db.BlockedUsers.ToListAsync();
            return blocked.AsReadOnly();
        }

        public async Task<bool> BlockChannelAsync(ulong cid, string reason = null)
        {
            using (DatabaseContext db = this.dbb.CreateContext()) {
                if (this.bChannels.Contains(cid))
                    return false;
                if (!this.bChannels.Add(cid))
                    return false;
                db.BlockedChannels.Add(new DatabaseBlockedChannel {
                    ChannelId = cid,
                    Reason = reason
                });
                await db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> BlockChannelsAsync(IEnumerable<ulong> cids, string reason = null)
        {
            if (!cids.Any())
                return 0;

            int blocked = 0;

            using (DatabaseContext db = this.dbb.CreateContext()) {
                foreach (ulong cid in cids) {
                    if (this.bChannels.Contains(cid))
                        continue;
                    if (!this.bChannels.Add(cid))
                        continue;
                    db.BlockedChannels.Add(new DatabaseBlockedChannel {
                        ChannelId = cid,
                        Reason = reason
                    });
                    blocked++;
                }
                await db.SaveChangesAsync();
            }

            return blocked;
        }

        public async Task<bool> BlockUserAsync(ulong uid, string reason = null)
        {
            if (!this.bUsers.Add(uid))
                return false;
            using (DatabaseContext db = this.dbb.CreateContext()) {
                db.BlockedUsers.Add(new DatabaseBlockedUser {
                    UserId = uid,
                    Reason = reason
                });
                await db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> BlockUsersAsync(IEnumerable<ulong> uids, string reason = null)
        {
            if (!uids.Any())
                return 0;

            int blocked = 0;

            using (DatabaseContext db = this.dbb.CreateContext()) {
                foreach (ulong uid in uids) {
                    if (!this.bUsers.Add(uid))
                        continue;
                    db.BlockedUsers.Add(new DatabaseBlockedUser {
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
            using (DatabaseContext db = this.dbb.CreateContext()) {
                db.BlockedChannels.Remove(new DatabaseBlockedChannel { ChannelId = cid });
                await db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> UnblockChannelsAsync(IEnumerable<ulong> cids)
        {
            if (!cids.Any())
                return 0;

            int unblocked = 0;

            using (DatabaseContext db = this.dbb.CreateContext()) {
                foreach (ulong cid in cids) {
                    if (!this.bChannels.TryRemove(cid))
                        continue;
                    db.BlockedChannels.Remove(new DatabaseBlockedChannel { ChannelId = cid });
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
            using (DatabaseContext db = this.dbb.CreateContext()) {
                db.BlockedUsers.Remove(new DatabaseBlockedUser { UserId = uid });
                await db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> UnblockUsersAsync(IEnumerable<ulong> uids)
        {
            if (!uids.Any())
                return 0;

            int unblocked = 0;

            using (DatabaseContext db = this.dbb.CreateContext()) {
                foreach (ulong uid in uids) {
                    if (!this.bUsers.TryRemove(uid))
                        continue;
                    db.BlockedUsers.Remove(new DatabaseBlockedUser { UserId = uid });
                    unblocked++;
                }
                await db.SaveChangesAsync();
            }

            return unblocked;
        }
    }
}
