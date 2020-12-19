using System;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<bool> AddToBankAccountAsync(ulong gid, ulong uid, long change)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            BankAccount? account = await this.GetAsync(gid, uid);
            if (account is null || account.Balance < change)
                return false;
            
            account.Balance += change;
            db.BankAccounts.Update(account);
            
            await db.SaveChangesAsync();
            return true;
        }

        public async Task ModifyBankAccountAsync(ulong gid, ulong uid, Func<long, long> balanceModifier)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            
            bool created = false;

            BankAccount? account = await this.GetAsync(gid, uid);
            if (account is null) {
                account = new BankAccount {
                    GuildId = gid,
                    UserId = uid
                };
                created = true;
            }

            account.Balance = balanceModifier(account.Balance);
            if (account.Balance < 0)
                account.Balance = 0;

            if (created)
                db.BankAccounts.Add(account);
            else
                db.BankAccounts.Update(account);

            await db.SaveChangesAsync();
        }
    }
}
