using System.Linq;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Chickens.Services
{
    public sealed class ChickenBoughtUpgradeService : DbAbstractionServiceBase<ChickenBoughtUpgrade, (ulong gid, ulong uid), int>
    {
        public override bool IsDisabled => false;


        public ChickenBoughtUpgradeService(DbContextBuilder dbb)
            : base(dbb) { }


        public override DbSet<ChickenBoughtUpgrade> DbSetSelector(TheGodfatherDbContext db)
            => db.ChickensBoughtUpgrades;

        public override ChickenBoughtUpgrade EntityFactory((ulong gid, ulong uid) grid, int id)
            => new ChickenBoughtUpgrade { GuildId = grid.gid, UserId = grid.uid, Id = id };

        public override (ulong gid, ulong uid) EntityGroupSelector(ChickenBoughtUpgrade entity)
            => (entity.GuildId, entity.UserId);

        public override int EntityIdSelector(ChickenBoughtUpgrade entity)
            => entity.Id;

        public override object[] EntityPrimaryKeySelector((ulong gid, ulong uid) grid, int id)
            => new object[] { (long)grid.gid, (long)grid.uid, id };

        public override IQueryable<ChickenBoughtUpgrade> GroupSelector(IQueryable<ChickenBoughtUpgrade> entities, (ulong gid, ulong uid) grid)
            => entities.Where(bu => bu.GuildIdDb == (long)grid.gid && bu.UserIdDb == (long)grid.uid);
    }
}
