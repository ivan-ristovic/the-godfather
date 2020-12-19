using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Chickens.Services
{
    public sealed class ChickenService : DbAbstractionServiceBase<Chicken, ulong, ulong>
    {
        public override bool IsDisabled => false;


        public ChickenService(DbContextBuilder dbb)
            : base(dbb) { }


        public override DbSet<Chicken> DbSetSelector(TheGodfatherDbContext db)
            => db.Chickens;

        public override IQueryable<Chicken> GroupSelector(IQueryable<Chicken> ars, ulong gid)
            => ars.Where(ar => ar.GuildIdDb == (long)gid);

        public override Chicken EntityFactory(ulong gid, ulong uid)
            => new Chicken { GuildId = gid, UserId = uid };

        public override ulong EntityIdSelector(Chicken c)
            => c.UserId;

        public override ulong EntityGroupSelector(Chicken c)
            => c.GuildId;

        public override object[] EntityPrimaryKeySelector(ulong gid, ulong uid)
            => new object[] { (long)gid, (long)uid };

        public async Task UpdateAsync(ChickenFightResult res)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            db.Chickens.Update(res.Winner);
            if (res.Loser.Stats.TotalVitality > 0)
                db.Chickens.Update(res.Loser);
            else
                db.Chickens.Remove(res.Loser);
            await db.SaveChangesAsync();
        }

        public Chicken? GetByName(ulong gid, string name)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            Chicken? chicken = db.Chickens
                .Include(c => c.Upgrades)
                    .ThenInclude(u => u.Upgrade)
                .Where(c => c.GuildIdDb == (long)gid)
                .AsEnumerable()
                .FirstOrDefault(c => string.Compare(c.Name, name, true) == 0);
            return chicken;
        }

        public async Task<Chicken?> GetCompleteAsync(ulong gid, ulong uid)
        {
            Chicken? chicken = null;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                chicken = await db.Chickens
                    .Include(c => c.Upgrades)
                        .ThenInclude(u => u.Upgrade)
                    .SingleOrDefaultAsync(c => c.GuildIdDb == (long)gid && c.UserIdDb == (long)uid);
            }
            return chicken;
        }

        public async Task<bool> HealAsync(ulong gid, ulong uid, int amount)
        {
            Chicken? chicken = await this.GetAsync(gid, uid);
            if (chicken is null)
                return false;

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                chicken.Vitality = (chicken.Vitality + amount) > chicken.BareMaxVitality
                    ? chicken.BareMaxVitality
                    : chicken.Vitality + amount;
                db.Chickens.Update(chicken);
                await db.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> RenameAsync(ulong gid, ulong uid, string name)
        {
            Chicken? chicken = await this.GetAsync(gid, uid);
            if (chicken is null)
                return false;

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                chicken.Name = name;
                db.Chickens.Update(chicken);
                await db.SaveChangesAsync();
            }

            return true;
        }

        public new async Task<IReadOnlyList<Chicken>> GetAllAsync(ulong gid)
        {
            List<Chicken> chickens;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                chickens = await db.Chickens
                    .Where(c => c.GuildIdDb == (long)gid)
                    .Include(c => c.Upgrades)
                        .ThenInclude(u => u.Upgrade)
                    .ToListAsync();
            }
            return chickens;
        }

        public async Task<IReadOnlyList<Chicken>> GetTopAsync(ulong gid, int amount = 10)
        {
            IReadOnlyList<Chicken> all = await this.GetAllAsync(gid);
            return all
                .OrderBy(c => c.Stats.TotalStrength)
                .Take(amount)
                .ToList()
                .AsReadOnly();
        }

        public async Task<IReadOnlyList<Chicken>> GetGlobalTopAsync(int amount = 10)
        {
            List<Chicken> chickens;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                chickens = await db.Chickens
                    .Include(c => c.Upgrades)
                        .ThenInclude(u => u.Upgrade)
                    .ToListAsync();
            }
            return chickens
                .OrderBy(c => c.Stats.TotalStrength)
                .Take(amount)
                .ToList()
                .AsReadOnly();
        }

        public async Task UpdateAsync(IEnumerable<Chicken> chickens)
        {
            if (!chickens.Any())
                return;

            using TheGodfatherDbContext db = this.dbb.CreateContext();
            db.UpdateRange(chickens);
            await db.SaveChangesAsync();
        }

        public Task UpdateAsync(params Chicken[] chickens)
            => this.UpdateAsync(chickens.AsEnumerable());
    }
}
