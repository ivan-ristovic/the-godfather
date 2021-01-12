using System;
using System.Collections.Generic;
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
            => new object[] { (long)grid, (long)id };

        public override IQueryable<BankAccount> GroupSelector(IQueryable<BankAccount> entities, ulong grid)
            => entities.Where(acc => (ulong)acc.GuildIdDb == grid);

        public Task IncreaseBankAccountAsync(ulong gid, ulong uid, long change)
            => this.ModifyBankAccountAsync(gid, uid, b => b + change);

        public async Task<bool> TryDecreaseBankAccountAsync(ulong gid, ulong uid, long change)
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

        public async Task<IReadOnlyList<BankAccount>> GetTopAccountsAsync(ulong? gid = null, int amount = 10)
        {
            List<BankAccount> topAccounts;
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            topAccounts = await 
                (gid is { } ? db.BankAccounts.Where(a => a.GuildId == gid) : db.BankAccounts)
                .OrderByDescending(a => a.Balance)
                .Take(amount)
                .ToListAsync();
            return topAccounts;
        }

        public async Task<bool> TransferAsync(ulong gid, ulong src, ulong dst, long amount)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                try {
                    await db.Database.BeginTransactionAsync();

                    if (!await this.TryDecreaseBankAccountAsync(gid, src, amount))
                        return false;
                    await this.IncreaseBankAccountAsync(gid, dst, amount);
                    await db.SaveChangesAsync();

                    db.Database.CommitTransaction();
                } catch {
                    db.Database.RollbackTransaction();
                    throw;
                }
            }
            return true;
        }

        public async Task RemoveAllAsync(ulong uid)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            db.BankAccounts.RemoveRange(db.BankAccounts.Where(acc => acc.UserIdDb == (long)uid));
            await db.SaveChangesAsync();
        }
    }
}
