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
    public partial class DBService
    {
        public async Task<IReadOnlyDictionary<ulong, ulong>> GetExperienceForAllUsersAsync()
        {
            var msgcount = new Dictionary<ulong, ulong>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.msgcount;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            msgcount[(ulong)(long)reader["uid"]] = (ulong)(long)reader["count"];
                    }
                }
            } finally {
                _sem.Release();
            }

            return new ReadOnlyDictionary<ulong, ulong>(msgcount);
        }
        
        public async Task UpdateExperienceForUserAsync(ulong uid, ulong count)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT count FROM gf.msgcount WHERE uid = @uid LIMIT 1;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res == null || res is DBNull) {
                        using (var cmd1 = con.CreateCommand()) {
                            cmd.CommandText = "INSERT INTO gf.msgcount VALUES(@uid, @count);";
                            cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                            cmd.Parameters.AddWithValue("count", NpgsqlDbType.Bigint, (long)count);
                            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    } else if ((ulong)(long)res != count) {
                        using (var cmd1 = con.CreateCommand()) {
                            cmd.CommandText = "UPDATE gf.msgcount SET count = @count WHERE uid = @uid;";
                            cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);
                            cmd.Parameters.AddWithValue("count", NpgsqlDbType.Bigint, (long)count);
                            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
