#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task<bool> BankContainsUserAsync(ulong uid, ulong gid)
        {
            long? balance = await GetUserCreditAmountAsync(uid, gid)
                .ConfigureAwait(false);
            return balance.HasValue;
        }

        public async Task CloseBankAccountForUserAsync(ulong uid, ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync()) 
                using (var cmd = con.CreateCommand()) { 
                    cmd.CommandText = "DELETE FROM gf.accounts WHERE uid = @uid AND gid = @gid;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetTenRichestUsersAsync(ulong gid = 0)
        {
            IReadOnlyList<IReadOnlyDictionary<string, string>> res;
            if (gid != 0) 
                res = await ExecuteRawQueryAsync($"SELECT * FROM gf.accounts WHERE gid = {gid} ORDER BY balance DESC LIMIT 10").ConfigureAwait(false);
            else
                res = await ExecuteRawQueryAsync("SELECT uid, SUM(balance) AS total_balance FROM gf.accounts GROUP BY uid ORDER BY total_balance DESC LIMIT 10").ConfigureAwait(false);
            return res;
        }

        public async Task<long?> GetUserCreditAmountAsync(ulong uid, ulong gid)
        {
            long? balance = null;

            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @uid AND gid = @gid LIMIT 1;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        balance = (long)res;
                }
            } finally {
                _sem.Release();
            }
            
            return balance;
        }

        public async Task GiveCreditsToUserAsync(ulong uid, ulong gid, long amount)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "UPDATE gf.accounts SET balance = balance + @amount WHERE uid = @uid AND gid = @gid;";
                    cmd.Parameters.AddWithValue("amount", NpgsqlDbType.Bigint, amount);
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task OpenBankAccountForUserAsync(ulong uid, ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO gf.accounts(uid, gid, balance) VALUES(@uid, @gid, 10000);";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<bool> TakeCreditsFromUserAsync(ulong uid, ulong gid, long amount)
        {
            long? balance = await GetUserCreditAmountAsync(uid, gid)
                .ConfigureAwait(false);
            if (!balance.HasValue || balance.Value < amount)
                return false;

            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "UPDATE gf.accounts SET balance = balance - @amount WHERE uid = @uid AND gid = @gid;";
                    cmd.Parameters.AddWithValue("amount", NpgsqlDbType.Bigint, amount);
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }

            return true;
        }

        public async Task TransferCreditsAsync(ulong source, ulong target, ulong gid, long amount)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString)) {
                    await con.OpenAsync().ConfigureAwait(false);

                    using (var cmd = con.CreateCommand()) {
                        cmd.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @target AND gid = @gid;";
                        cmd.Parameters.AddWithValue("target", NpgsqlDbType.Bigint, (long)target);
                        cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                        var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);

                        if (res == null || res is DBNull)
                            await OpenBankAccountForUserAsync(target, gid);
                    }

                    await _tsem.WaitAsync().ConfigureAwait(false);
                    try {
                        using (var transaction = con.BeginTransaction()) {
                            var cmd1 = con.CreateCommand();
                            cmd1.Transaction = transaction;
                            cmd1.CommandText = "SELECT balance FROM gf.accounts WHERE (uid = @source OR uid = @target) AND gid = @gid FOR UPDATE;";
                            cmd1.Parameters.AddWithValue("source", NpgsqlDbType.Bigint, (long)source);
                            cmd1.Parameters.AddWithValue("target", NpgsqlDbType.Bigint, (long)target);
                            cmd1.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                            await cmd1.ExecuteNonQueryAsync().ConfigureAwait(false);

                            var cmd2 = con.CreateCommand();
                            cmd2.Transaction = transaction;
                            cmd2.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @source AND gid = @gid;";
                            cmd2.Parameters.AddWithValue("source", NpgsqlDbType.Bigint, (long)source);
                            cmd2.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                            var res = await cmd2.ExecuteScalarAsync().ConfigureAwait(false);
                            if (res == null || res is DBNull || (long)res < amount) {
                                await transaction.RollbackAsync().ConfigureAwait(false);
                                throw new DatabaseServiceException("Source user's currency amount is insufficient for the transfer.");
                            }

                            var cmd3 = con.CreateCommand();
                            cmd3.Transaction = transaction;
                            cmd3.CommandText = "UPDATE gf.accounts SET balance = balance - @amount WHERE uid = @source AND gid = @gid;";
                            cmd3.Parameters.AddWithValue("amount", NpgsqlDbType.Bigint, amount);
                            cmd3.Parameters.AddWithValue("source", NpgsqlDbType.Bigint, (long)source);
                            cmd3.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                            await cmd3.ExecuteNonQueryAsync().ConfigureAwait(false);

                            var cmd4 = con.CreateCommand();
                            cmd4.Transaction = transaction;
                            cmd4.CommandText = "UPDATE gf.accounts SET balance = balance + @amount WHERE uid = @target AND gid = @gid;";
                            cmd4.Parameters.AddWithValue("amount", NpgsqlDbType.Bigint, amount);
                            cmd4.Parameters.AddWithValue("target", NpgsqlDbType.Bigint, (long)target);
                            cmd4.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                            await cmd4.ExecuteNonQueryAsync().ConfigureAwait(false);

                            await transaction.CommitAsync().ConfigureAwait(false);

                            cmd1.Dispose();
                            cmd2.Dispose();
                            cmd3.Dispose();
                            cmd4.Dispose();
                        }
                    } finally {
                        _tsem.Release();
                    }
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task UpdateBankAccountsAsync()
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "UPDATE gf.accounts SET balance = GREATEST(CEILING(1.0015 * balance), 10);";

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
