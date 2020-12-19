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
        private ConcurrentHashSet<ulong> bGuilds;
        private readonly DbContextBuilder dbb;


        public BlockingService(DbContextBuilder dbb, bool loadData = true)
        {
            this.dbb = dbb;
            this.bChannels = new ConcurrentHashSet<ulong>();
            this.bUsers = new ConcurrentHashSet<ulong>();
            this.bGuilds = new ConcurrentHashSet<ulong>();
            if (loadData)
                this.LoadData();
        }


        public void LoadData()
        {
            Log.Debug("Loading blocked entities");
            try {
                using TheGodfatherDbContext db = this.dbb.CreateContext();
                this.bChannels = new ConcurrentHashSet<ulong>(db.BlockedChannels.Select(c => c.Id));
                this.bUsers = new ConcurrentHashSet<ulong>(db.BlockedUsers.Select(u => u.Id));
                this.bGuilds = new ConcurrentHashSet<ulong>(db.BlockedGuilds.Select(g => g.Id));
            } catch (Exception e) {
                Log.Error(e, "Loading blocked entities failed");
            }
        }

        public void Sync()
        {
            this.bUsers.Clear();
            this.bChannels.Clear();
            this.bGuilds.Clear();
            this.LoadData();
        }

        public bool IsChannelBlocked(ulong cid)

            => this.bChannels.Contains(cid);

        public bool IsUserBlocked(ulong uid)
            => this.bUsers.Contains(uid);

        public bool IsGuildBlocked(ulong gid)
            => this.bGuilds.Contains(gid);

        public bool IsBlocked(ulong gid, ulong cid, ulong uid)
            => this.IsGuildBlocked(gid) || this.IsChannelBlocked(cid) || this.IsUserBlocked(uid);

        public async Task<IReadOnlyList<BlockedChannel>> GetBlockedChannelsAsync()
        {
            List<BlockedChannel> blocked;
            using (TheGodfatherDbContext db = this.dbb.CreateContext())
                blocked = await db.BlockedChannels.ToListAsync();
            return blocked.AsReadOnly();
        }

        public async Task<IReadOnlyList<BlockedGuild>> GetBlockedGuildsAsync()
        {
            List<BlockedGuild> blocked;
            using (TheGodfatherDbContext db = this.dbb.CreateContext())
                blocked = await db.BlockedGuilds.ToListAsync();
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
                    Id = cid,
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
                foreach (ulong cid in cids.Distinct()) {
                    if (!this.bChannels.Add(cid))
                        continue;
                    db.BlockedChannels.Add(new BlockedChannel {
                        Id = cid,
                        Reason = reason
                    });
                    blocked++;
                }
                await db.SaveChangesAsync();
            }

            return blocked;
        }

        public async Task<bool> BlockGuildAsync(ulong gid, string? reason = null)
        {
            if (!this.bGuilds.Add(gid))
                return false;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.BlockedGuilds.Add(new BlockedGuild {
                    Id = gid,
                    Reason = reason
                });
                await db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> BlockGuildsAsync(IEnumerable<ulong> gids, string? reason = null)
        {
            if (!gids.Any())
                return 0;

            int blocked = 0;

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                foreach (ulong gid in gids.Distinct()) {
                    if (!this.bGuilds.Add(gid))
                        continue;
                    db.BlockedGuilds.Add(new BlockedGuild {
                        Id = gid,
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
                    Id = uid,
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
                foreach (ulong uid in uids.Distinct()) {
                    if (!this.bUsers.Add(uid))
                        continue;
                    db.BlockedUsers.Add(new BlockedUser {
                        Id = uid,
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
                db.BlockedChannels.Remove(new BlockedChannel { Id = cid });
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
                foreach (ulong cid in cids.Distinct()) {
                    if (!this.bChannels.TryRemove(cid))
                        continue;
                    db.BlockedChannels.Remove(new BlockedChannel { Id = cid });
                    unblocked++;
                }
                await db.SaveChangesAsync();
            }

            return unblocked;
        }

        public async Task<bool> UnblockGuildAsync(ulong gid)
        {
            if (!this.bGuilds.TryRemove(gid))
                return false;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.BlockedGuilds.Remove(new BlockedGuild { Id = gid });
                await db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<int> UnblockGuildsAsync(IEnumerable<ulong> gids)
        {
            if (!gids.Any())
                return 0;

            int unblocked = 0;

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                foreach (ulong gid in gids.Distinct()) {
                    if (!this.bGuilds.TryRemove(gid))
                        continue;
                    db.BlockedGuilds.Remove(new BlockedGuild { Id = gid });
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
                db.BlockedUsers.Remove(new BlockedUser { Id = uid });
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
                foreach (ulong uid in uids.Distinct()) {
                    if (!this.bUsers.TryRemove(uid))
                        continue;
                    db.BlockedUsers.Remove(new BlockedUser { Id = uid });
                    unblocked++;
                }
                await db.SaveChangesAsync();
            }

            return unblocked;
        }
    }
}
