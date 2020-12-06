using System.Linq;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Currency.Services
{
    public sealed class BankAccountService : DbAbstractionServiceBase<BankAccount, ulong, ulong>
    {
        public override bool IsDisabled => false;


        public BankAccountService(DbContextBuilder dbb)
            : base(dbb) { }


        public override DbSet<BankAccount> DbSetSelector(TheGodfatherDbContext db)
            => db.BankAccounts;

        public override BankAccount EntityFactory(ulong grid, ulong id) 
            => new BankAccount { UserId = id, GuildId = grid };

        public override ulong EntityGroupSelector(BankAccount entity)
            => entity.GuildId;

        public override ulong EntityIdSelector(BankAccount entity)
            => entity.UserId;

        public override object[] EntityPrimaryKeySelector(ulong grid, ulong id)
            => new object[] { grid, id };

        public override IQueryable<BankAccount> GroupSelector(IQueryable<BankAccount> entities, ulong grid)
            => entities.Where(acc => (ulong)acc.GuildIdDb == grid);
    }
}
