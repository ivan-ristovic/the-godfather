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
        public async Task<bool> BankContainsUserAsync(ulong uid)
        {
            int? balance = await GetUserCreditAmountAsync(uid)
                .ConfigureAwait(false);
            return balance.HasValue;
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetTenRichestUsersAsync()
        {
            var res = await ExecuteRawQueryAsync("SELECT * FROM gf.accounts ORDER BY balance DESC LIMIT 10")
                .ConfigureAwait(false);
            return res;
        }

        public async Task<int?> GetUserCreditAmountAsync(ulong uid)
        {
            int? balance = null;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @uid LIMIT 1;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        balance = (int)res;
                }
            } finally {
                _sem.Release();
            }
            
            return balance;
        }

        public async Task GiveCreditsToUserAsync(ulong uid, int amount)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "UPDATE gf.accounts SET balance = balance + @amount WHERE uid = @uid;";
                    cmd.Parameters.AddWithValue("amount", NpgsqlDbType.Integer, amount);
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task OpenBankAccountForUserAsync(ulong uid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.accounts VALUES(@uid, 25);";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<bool> TakeCreditsFromUserAsync(ulong uid, int amount)
        {
            int? balance = await GetUserCreditAmountAsync(uid)
                .ConfigureAwait(false);
            if (!balance.HasValue || balance.Value < amount)
                return false;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "UPDATE gf.accounts SET balance = balance - @amount WHERE uid = @uid;";
                    cmd.Parameters.AddWithValue("amount", NpgsqlDbType.Integer, amount);
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }

            return true;
        }

        public async Task TransferCreditsAsync(ulong source, ulong target, long amount)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString)) {
                    await con.OpenAsync().ConfigureAwait(false);

                    using (var cmd = con.CreateCommand()) {
                        cmd.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @target;";
                        cmd.Parameters.AddWithValue("target", NpgsqlDbType.Bigint, (long)target);

                        var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);

                        if (res == null || res is DBNull)
                            await OpenBankAccountForUserAsync(target);
                    }

                    await _tsem.WaitAsync().ConfigureAwait(false);
                    try {
                        using (var transaction = con.BeginTransaction()) {
                            var cmd1 = con.CreateCommand();
                            cmd1.Transaction = transaction;
                            cmd1.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @source OR uid = @target FOR UPDATE;";
                            cmd1.Parameters.AddWithValue("source", NpgsqlDbType.Bigint, (long)source);
                            cmd1.Parameters.AddWithValue("target", NpgsqlDbType.Bigint, (long)target);

                            await cmd1.ExecuteNonQueryAsync().ConfigureAwait(false);

                            var cmd2 = con.CreateCommand();
                            cmd2.Transaction = transaction;
                            cmd2.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @source;";
                            cmd2.Parameters.AddWithValue("source", NpgsqlDbType.Bigint, (long)source);

                            var res = await cmd2.ExecuteScalarAsync().ConfigureAwait(false);
                            if (res == null || res is DBNull || (int)res < amount) {
                                await transaction.RollbackAsync().ConfigureAwait(false);
                                throw new CommandFailedException("Source user's currency amount is insufficient for the transfer.");
                            }

                            var cmd3 = con.CreateCommand();
                            cmd3.Transaction = transaction;
                            cmd3.CommandText = "UPDATE gf.accounts SET balance = balance - @amount WHERE uid = @source;";
                            cmd3.Parameters.AddWithValue("amount", NpgsqlDbType.Integer, amount);
                            cmd3.Parameters.AddWithValue("source", NpgsqlDbType.Bigint, (long)source);

                            await cmd3.ExecuteNonQueryAsync().ConfigureAwait(false);

                            var cmd4 = con.CreateCommand();
                            cmd4.Transaction = transaction;
                            cmd4.CommandText = "UPDATE gf.accounts SET balance = balance + @amount WHERE uid = @target;";
                            cmd4.Parameters.AddWithValue("amount", NpgsqlDbType.Integer, amount);
                            cmd4.Parameters.AddWithValue("target", NpgsqlDbType.Bigint, (long)target);

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
    }
}
