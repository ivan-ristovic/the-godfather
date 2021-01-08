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


        public StarboardService(DbContextBuilder dbb, GuildConfigService gcs)
            : base(dbb)
        {
            this.gcs = gcs;
        }


        public bool IsStarboardEnabled(ulong? gid, out string? emoji)
        {
            emoji = null;

            if (gid is null)
                return false;

            CachedGuildConfig gcfg = this.gcs.GetCachedConfig(gid);
            emoji = gcfg.StarboardEmoji;
            return gcfg.StarboardEnabled;
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
                using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                    this.DbSetSelector(db).Add(msg);
                    await db.SaveChangesAsync();
                }
            } else {
                starsPre = msg.Stars;
                if (count == 0) {
                    await this.RemoveAsync(msg);
                } else {
                    msg.Stars += count;
                    using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                        this.DbSetSelector(db).Update(msg);
                        await db.SaveChangesAsync();
                    }
                }
            }

            return new StarboardModificationResult(msg, starsPre, this.GetMinimumStarCount(gid));
        }

        public async Task AddStarboardLinkAsync(ulong gid, ulong cid, ulong mid, ulong smid)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            this.DbSetSelector(db).Update(new StarboardMessage {
                GuildId = gid,
                ChannelId = cid,
                MessageId = mid,
                StarMessageId = smid,
            });
            await db.SaveChangesAsync();
        }

        public async Task<ulong> GetStarboardChannelAsync(ulong gid)
        {
            GuildConfig gcfg = await gcs.GetConfigAsync(gid);
            return gcfg.StarboardChannelId;
        }

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
