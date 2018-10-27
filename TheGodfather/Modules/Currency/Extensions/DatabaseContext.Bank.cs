using System;
using System.Threading.Tasks;

using TheGodfather.Database;
using TheGodfather.Database.Entities;

namespace TheGodfather.Modules.Currency.Extensions
{
    public static class DatabaseContextBankExtensions
    {
        public static async Task<bool> TryDecreaseBankAccountAsync(this DatabaseContext db, ulong uid, ulong gid, long amount)
        {
            DatabaseBankAccount account = await db.BankAccounts.FindAsync((long)gid, (long)uid);
            if (account is null || account.Balance < amount)
                return false;
            account.Balance -= amount;
            db.BankAccounts.Update(account);
            return true;
        }

        public static async Task ModifyBankAccountAsync(this DatabaseContext db, ulong uid, ulong gid, Func<long, long> balanceModifier)
        {
            DatabaseBankAccount account = await db.BankAccounts.FindAsync((long)gid, (long)uid);
            if (account is null) {
                account = new DatabaseBankAccount() {
                    GuildId = gid,
                    UserId = uid
                };
                db.Add(account);
            }
            account.Balance = balanceModifier(account.Balance);
            db.BankAccounts.Update(account);
        }
    }
}
