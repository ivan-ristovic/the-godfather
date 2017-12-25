#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DatabaseService
    {
        public async Task<IReadOnlyDictionary<ulong, ulong>> GetMessageCountForAllUsersAsync()
        {
            await _sem.WaitAsync();

            var msgcount = new Dictionary<ulong, ulong>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.msgcount;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        msgcount[(ulong)(long)reader["uid"]] = (ulong)reader["count"];
                }
            }

            _sem.Release();
            return new ReadOnlyDictionary<ulong, ulong>(msgcount);
        }

        public async Task<ulong> GetMessageCountForUserAsync(ulong uid)
        {
            await _sem.WaitAsync();

            ulong msgcount = 0;

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT count FROM gf.msgcount WHERE uid = @uid LIMIT 1;";
                cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);

                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res == null || res is DBNull)
                    msgcount = (ulong)(long)res;
            }

            _sem.Release();
            return msgcount;
        }

        public async Task AddUserToMessageCountAsync(ulong uid)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "INSERT INTO gf.msgcount VALUES(@uid, 1);";
                cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task UpdateMessageCountAsync(ulong uid, ulong count)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT count FROM gf.msgcount WHERE uid = @uid LIMIT 1;";
                cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);

                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                using (var cmd1 = con.CreateCommand()) {
                    if (res == null || res is DBNull) {
                        cmd.CommandText = "INSERT INTO gf.msgcount VALUES(@uid, @count);";
                        cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                        cmd.Parameters.AddWithValue("count", NpgsqlDbType.Bigint, (long)count);
                    } else {
                        cmd.CommandText = "UPDATE gf.msgcount SET count = @count WHERE uid = @uid;";
                        cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                        cmd.Parameters.AddWithValue("count", NpgsqlDbType.Bigint, (long)count);
                    }
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

            _sem.Release();
        }
    }
}
