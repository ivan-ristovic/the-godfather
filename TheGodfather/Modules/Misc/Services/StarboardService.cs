using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Misc.Services
{
    public sealed class StarboardService : DbAbstractionServiceBase<StarboardMessage, ulong, (ulong cid, ulong mid)>
    {
        public override bool IsDisabled => false;

        private readonly GuildConfigService gcs;
        private readonly List<StarboardModificationResult> pendingUpdates;


        public StarboardService(DbContextBuilder dbb, GuildConfigService gcs)
            : base(dbb)
        {
            this.gcs = gcs;
            this.pendingUpdates = new();
        }


        public bool IsStarboardEnabled(ulong gid, out ulong cid, out string emoji)
        {
            CachedGuildConfig gcfg = this.gcs.GetCachedConfig(gid);
            emoji = gcfg.StarboardEmoji ?? CachedGuildConfig.DefaultStarboardEmoji;
            cid = gcfg.StarboardChannelId;
            return gcfg.StarboardEnabled;
        }

        public Task<GuildConfig> ModifySettingsAsync(ulong gid, ulong? cid, string? emoji = null, int? sens = null)
        {
            return this.gcs.ModifyConfigAsync(gid, gcfg => {
                gcfg.StarboardChannelId = cid ?? 0;
                gcfg.StarboardEmoji = emoji;
                gcfg.StarboardSensitivity = sens ?? CachedGuildConfig.DefaultStarboardSensitivity;
            });
        }

        public async Task<StarboardModificationResult> UpdateStarCountAsync(ulong gid, ulong cid, ulong mid, int count)
        {
            int starsPre = 0;
            StarboardMessage? msg = await this.GetAsync(gid, (cid, mid));
            if (msg is null) {
                msg = new StarboardMessage {
                    ChannelId = cid,
                    GuildId = gid,
                    MessageId = mid,
                    Stars = count,
                };
            } else {
                starsPre = msg.Stars;
                msg.Stars = count == 0 ? 0 : msg.Stars + count;
            }

            var res = new StarboardModificationResult(msg, starsPre, this.GetMinimumStarCount(gid));
            lock (this.pendingUpdates) {
                this.pendingUpdates.RemoveAll(r => r.Entry.Equals(res.Entry));
                this.pendingUpdates.Add(res);
            }
            return res;
        }

        public IReadOnlyDictionary<ulong, List<StarboardModificationResult>> GetPendingUpdates()
        {
            Dictionary<ulong, List<StarboardModificationResult>> res;
            lock (this.pendingUpdates) {
                res = this.pendingUpdates.GroupBy(r => r.Entry.GuildId).ToDictionary(g => g.Key, g => g.ToList());
                this.Sync();
            }
            return res;
        }

        public async Task AddStarboardLinkAsync(ulong gid, ulong cid, ulong mid, ulong smid)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            StarboardMessage? msg = await this.DbSetSelector(db).FindAsync((long)gid, (long)cid, (long)mid);
            if (msg is null) {
                this.DbSetSelector(db).Add(new StarboardMessage {
                    GuildId = gid,
                    ChannelId = cid,
                    MessageId = mid,
                    StarMessageId = smid,
                });
            } else {
                msg.StarMessageId = smid;
                this.DbSetSelector(db).Update(msg);
            }
            await db.SaveChangesAsync();
        }

        public ulong GetStarboardChannel(ulong gid)
            => this.gcs.GetCachedConfig(gid).StarboardChannelId;

        public int GetStarboardSensitivity(ulong gid)
            => this.gcs.GetCachedConfig(gid).StarboardSensitivity;

        public Task SetStarboardSensitivityAsync(ulong gid, int sens)
            => this.gcs.ModifyConfigAsync(gid, gcfg => gcfg.StarboardSensitivity = sens);

        public int GetMinimumStarCount(ulong gid)
            => this.gcs.GetCachedConfig(gid).StarboardSensitivity;

        public override DbSet<StarboardMessage> DbSetSelector(TheGodfatherDbContext db)
            => db.StarboardMessages;

        public override IQueryable<StarboardMessage> GroupSelector(IQueryable<StarboardMessage> entities, ulong grid)
            => entities.Where(sm => sm.GuildIdDb == (long)grid);

        public override StarboardMessage EntityFactory(ulong grid, (ulong, ulong) id)
            => new StarboardMessage { GuildId = grid, };

        public override (ulong, ulong) EntityIdSelector(StarboardMessage entity)
            => (entity.ChannelId, entity.MessageId);

        public override ulong EntityGroupSelector(StarboardMessage entity)
            => entity.GuildId;

        public override object[] EntityPrimaryKeySelector(ulong grid, (ulong, ulong) id)
            => new object[] { (long)grid, (long)id.Item1, (long)id.Item2 };


        private bool Sync()
        {
            if (!this.pendingUpdates.Any())
                return true;

            bool succ = false;
            bool upd = false;
            try {
                using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                    foreach (StarboardModificationResult res in this.pendingUpdates) {
                        StarboardMessage? dbMsg = db.StarboardMessages.Find((long)res.Entry.GuildId, (long)res.Entry.ChannelId, (long)res.Entry.MessageId);
                        if (dbMsg is null) {
                            if (res.Entry.Stars != 0) {
                                this.DbSetSelector(db).Add(res.Entry);
                                upd = true;
                            }
                        } else {
                            upd = true;
                            if (res.Entry.Stars < this.GetMinimumStarCount(res.Entry.GuildId))
                                this.DbSetSelector(db).Remove(dbMsg);
                            else
                                this.DbSetSelector(db).Update(res.Entry);
                        }
                    }
                    if (upd)
                        db.SaveChanges();
                }
            } finally {
                if (upd)
                    this.pendingUpdates.Clear();
            }

            return succ;
        }
    }
}
