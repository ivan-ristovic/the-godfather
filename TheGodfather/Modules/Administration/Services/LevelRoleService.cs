using System.Linq;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class LevelRoleService : DbAbstractionServiceBase<LevelRole, ulong, short>
    {
        public override bool IsDisabled => false;


        public LevelRoleService(DbContextBuilder dbb)
            : base(dbb) { }


        public override DbSet<LevelRole> DbSetSelector(TheGodfatherDbContext db)
            => db.LevelRoles;

        public override IQueryable<LevelRole> GroupSelector(IQueryable<LevelRole> lrs, ulong gid)
            => lrs.Where(ar => ar.GuildIdDb == (long)gid);

        public override LevelRole EntityFactory(ulong gid, short rank)
            => new LevelRole { GuildId = gid, Rank = rank };

        public override short EntityIdSelector(LevelRole lr)
            => lr.Rank;

        public override ulong EntityGroupSelector(LevelRole lr)
            => lr.GuildId;

        public override object[] EntityPrimaryKeySelector(ulong gid, short rank)
            => new object[] { (long)gid, rank };
    }
}
