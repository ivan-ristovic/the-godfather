using System.Linq;
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
    }
}
