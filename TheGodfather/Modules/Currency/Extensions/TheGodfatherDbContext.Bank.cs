using System;
using System.Threading.Tasks;
using TheGodfather.Database;
using TheGodfather.Database.Models;

namespace TheGodfather.Modules.Currency.Extensions
{
    public static class TheGodfatherDbContextBankExtensions
    {
        public static async Task<bool> TryDecreaseBankAccountAsync(this TheGodfatherDbContext db, ulong uid, ulong gid, long amount)
        {
            BankAccount account = await db.BankAccounts.FindAsync((long)gid, (long)uid);
            if (account is null || account.Balance < amount)
                return false;
            account.Balance -= amount;
            db.BankAccounts.Update(account);
            return true;
        }

        public static async Task ModifyBankAccountAsync(this TheGodfatherDbContext db, ulong uid, ulong gid, Func<long, long> balanceModifier)
        {
            bool created = false;

            BankAccount account = await db.BankAccounts.FindAsync((long)gid, (long)uid);
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
        }
    }
}
