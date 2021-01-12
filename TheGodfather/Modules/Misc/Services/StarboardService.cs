using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Common.Collections;
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
        private readonly ConcurrentHashSet<StarboardMessage> updated;


        public StarboardService(DbContextBuilder dbb, GuildConfigService gcs)
            : base(dbb)
        {
            this.gcs = gcs;
            this.updated = new();
        }


        public bool IsStarboardEnabled(ulong gid, out ulong cid, out string emoji)
        {
            CachedGuildConfig gcfg = this.gcs.GetCachedConfig(gid);
            emoji = gcfg.StarboardEmoji ?? CachedGuildConfig.DefaultStarboardEmoji;
            cid = gcfg.StarboardChannelId;
            return gcfg.StarboardEnabled;
        }

        public async Task<GuildConfig> ModifySettingsAsync(ulong gid, ulong? cid, string? emoji = null, int? sens = null)
        {
            if (cid is null)
                await this.ClearAsync(gid);
            return await this.gcs.ModifyConfigAsync(gid, gcfg => {
                gcfg.StarboardChannelId = cid ?? 0;
                gcfg.StarboardEmoji = emoji;
                gcfg.StarboardSensitivity = sens ?? CachedGuildConfig.DefaultStarboardSensitivity;
            });
        }

        public void RegisterModifiedMessage(ulong gid, ulong cid, ulong mid)
            => this.updated.Add(new StarboardMessage { ChannelId = cid, GuildId = gid, MessageId = mid });

        public async Task<StarboardModificationResult> SyncWithDbAsync(StarboardMessage msg)
        {
            StarboardActionType type = StarboardActionType.None;

            using TheGodfatherDbContext db = this.dbb.CreateContext();
            StarboardMessage? dbmsg = await db.StarboardMessages.FindAsync(msg.GuildIdDb, msg.ChannelIdDb, msg.MessageIdDb);
            if (dbmsg is null) {
                if (msg.Stars >= this.GetMinimumStarCount(msg.GuildId)) {
                    db.StarboardMessages.Add(msg);
                    type = StarboardActionType.Send;
                    dbmsg = msg;
                }
            } else {
                if (msg.Stars >= this.GetMinimumStarCount(msg.GuildId)) {
                    dbmsg.Stars = msg.Stars;
                    db.StarboardMessages.Update(dbmsg);
                    type = StarboardActionType.Modify;
                } else {
                    db.StarboardMessages.Remove(dbmsg);
                    type = StarboardActionType.Delete;
                }
            }

            if (type != StarboardActionType.None)
                await db.SaveChangesAsync();

            return new StarboardModificationResult(dbmsg, type);
        }

        public IReadOnlyDictionary<ulong, List<StarboardMessage>> GetUpdatedMessages()
        {
            var toUpdate = this.updated.GroupBy(sm => sm.GuildId).ToDictionary(g => g.Key, g => g.ToList());
            toUpdate.Clear();
            return toUpdate;
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
    }
}
