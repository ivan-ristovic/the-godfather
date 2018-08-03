#region USING_DIRECTIVES
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheGodfather.Exceptions;
#endregion

namespace TheGodfather.Services.Database.Bank
{
    internal static class DBServiceBankExtensions
    {
        public static Task BulkIncreaseAllBankAccountsAsync(this DBService db)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.accounts SET balance = GREATEST(CEILING(1.0015 * balance), 10);";

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task CloseBankAccountAsync(this DBService db, ulong uid, ulong? gid = null)
        {
            return db.ExecuteCommandAsync(cmd => {
                if (gid.HasValue) {
                    cmd.CommandText = "DELETE FROM gf.accounts WHERE uid = @uid AND gid = @gid;";
                    cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid.Value));
                } else {
                    cmd.CommandText = "DELETE FROM gf.accounts WHERE uid = @uid;";
                }
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<bool> DecreaseBankAccountBalanceAsync(this DBService db, ulong uid, ulong gid, long amount)
        {
            long? balance = await db.GetBankAccountBalanceAsync(uid, gid);
            if (!balance.HasValue || balance.Value < amount)
                return false;

            await db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.accounts SET balance = balance - @amount WHERE uid = @uid AND gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("amount", amount));
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });

            return true;
        }

        public static async Task<long?> GetBankAccountBalanceAsync(this DBService db, ulong uid, ulong gid)
        {
            long? balance = null;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @uid AND gid = @gid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    balance = (long)res;
            });

            return balance;
        }

        public static async Task<IReadOnlyList<(ulong, long)>> GetTopBankAccountsAsync(this DBService db, ulong? gid = null)
        {
            var res = new List<(ulong, long)>();

            if (gid.HasValue) {
                await db.ExecuteCommandAsync(async (cmd) => {
                    cmd.CommandText = $"SELECT * FROM gf.accounts WHERE gid = @gid ORDER BY balance DESC LIMIT 10";
                    cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid.Value));

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            res.Add(((ulong)(long)reader["uid"], (long)reader["balance"]));
                });

            } else {
                await db.ExecuteCommandAsync(async (cmd) => {
                    cmd.CommandText = $"SELECT uid, SUM(balance) AS total_balance FROM gf.accounts GROUP BY uid ORDER BY total_balance DESC LIMIT 10";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            res.Add(((ulong)(long)reader["uid"], (long)reader["total_balance"]));
                });
            }

            return res.AsReadOnly();
        }

        public static async Task<bool> HasBankAccountAsync(this DBService db, ulong uid, ulong gid)
        {
            long? balance = await db.GetBankAccountBalanceAsync(uid, gid);
            return balance.HasValue;
        }

        public static Task IncreaseBankAccountBalanceAsync(this DBService db, ulong uid, ulong gid, long amount)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.accounts SET balance = balance + @amount WHERE uid = @uid AND gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("amount", amount));
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task OpenBankAccountAsync(this DBService db, ulong uid, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.accounts(uid, gid, balance) VALUES(@uid, @gid, 10000);";
                cmd.Parameters.Add(new NpgsqlParameter<long>("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task TransferBetweenBankAccountsAsync(this DBService db, ulong source, ulong target, ulong gid, long amount)
        {
            await db.ExecuteTransactionAsync(async (con, tsem) => {
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @target AND gid = @gid;";
                    cmd.Parameters.Add(new NpgsqlParameter<long>("target", (long)target));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                    object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);

                    if (res == null || res is DBNull)
                        await db.OpenBankAccountAsync(target, gid);
                }

                await tsem.WaitAsync().ConfigureAwait(false);
                try {
                    using (var transaction = con.BeginTransaction()) {
                        var cmd1 = con.CreateCommand();
                        cmd1.Transaction = transaction;
                        cmd1.CommandText = "SELECT balance FROM gf.accounts WHERE (uid = @source OR uid = @target) AND gid = @gid FOR UPDATE;";
                        cmd1.Parameters.Add(new NpgsqlParameter<long>("source", (long)source));
                        cmd1.Parameters.Add(new NpgsqlParameter<long>("target", (long)target));
                        cmd1.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                        await cmd1.ExecuteNonQueryAsync().ConfigureAwait(false);

                        var cmd2 = con.CreateCommand();
                        cmd2.Transaction = transaction;
                        cmd2.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @source AND gid = @gid;";
                        cmd2.Parameters.Add(new NpgsqlParameter<long>("source", (long)source));
                        cmd2.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                        object res = await cmd2.ExecuteScalarAsync().ConfigureAwait(false);
                        if (res == null || res is DBNull || (long)res < amount) {
                            await transaction.RollbackAsync().ConfigureAwait(false);
                            throw new DatabaseOperationException("Source user's currency amount is insufficient for the transfer.");
                        }

                        var cmd3 = con.CreateCommand();
                        cmd3.Transaction = transaction;
                        cmd3.CommandText = "UPDATE gf.accounts SET balance = balance - @amount WHERE uid = @source AND gid = @gid;";
                        cmd3.Parameters.Add(new NpgsqlParameter<long>("amount", amount));
                        cmd3.Parameters.Add(new NpgsqlParameter<long>("source", (long)source));
                        cmd3.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                        await cmd3.ExecuteNonQueryAsync().ConfigureAwait(false);

                        var cmd4 = con.CreateCommand();
                        cmd4.Transaction = transaction;
                        cmd4.CommandText = "UPDATE gf.accounts SET balance = balance + @amount WHERE uid = @target AND gid = @gid;";
                        cmd4.Parameters.Add(new NpgsqlParameter<long>("amount", amount));
                        cmd4.Parameters.Add(new NpgsqlParameter<long>("target", (long)target));
                        cmd4.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                        await cmd4.ExecuteNonQueryAsync().ConfigureAwait(false);

                        await transaction.CommitAsync().ConfigureAwait(false);

                        cmd1.Dispose();
                        cmd2.Dispose();
                        cmd3.Dispose();
                        cmd4.Dispose();
                    }
                } finally {
                    tsem.Release();
                }
            });
        }
    }
}
