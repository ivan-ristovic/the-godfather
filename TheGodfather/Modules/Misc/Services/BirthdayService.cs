using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Misc.Services
{
    public sealed class BirthdayService : DbAbstractionServiceBase<Birthday, (ulong gid, ulong cid), ulong>
    {
        public override bool IsDisabled => false;


        public BirthdayService(DbContextBuilder dbb)
            : base(dbb) { }


        public override DbSet<Birthday> DbSetSelector(TheGodfatherDbContext db)
            => db.Birthdays;

        public override IQueryable<Birthday> GroupSelector(IQueryable<Birthday> bds, (ulong gid, ulong cid) id)
            => bds.Where(ar => ar.GuildIdDb == (long)id.gid && ar.ChannelIdDb == (long)id.cid);

        public override Birthday EntityFactory((ulong gid, ulong cid) id, ulong uid)
            => new Birthday { GuildId = id.gid, ChannelId = id.cid, UserId = uid };

        public override ulong EntityIdSelector(Birthday bd)
            => bd.UserId;

        public override (ulong, ulong) EntityGroupSelector(Birthday bd)
            => (bd.GuildId, bd.ChannelId);

        public override object[] EntityPrimaryKeySelector((ulong gid, ulong cid) id, ulong uid)
            => new object[] { (long)id.gid, (long)id.cid, (long)uid };

        public async Task<IReadOnlyList<Birthday>> GetUserBirthdaysAsync(ulong gid, ulong uid)
        {
            List<Birthday> bds;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                bds = await db.Birthdays.Where(b => b.GuildIdDb == (long)gid && b.UserIdDb == (long)uid).ToListAsync();
            }
            return bds.AsReadOnly();
        }

        public async Task<IReadOnlyList<Birthday>> GetAllBirthdaysAsync(ulong gid)
        {
            List<Birthday> bds;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                bds = await db.Birthdays.Where(b => b.GuildIdDb == (long)gid).ToListAsync();
            }
            return bds.AsReadOnly();
        }
    }
}
