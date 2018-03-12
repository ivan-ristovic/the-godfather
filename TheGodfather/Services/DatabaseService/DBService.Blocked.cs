#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task<IReadOnlyList<ulong>> GetBlockedUsersAsync()
        {
            var blocked = new List<ulong>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.blocked_users;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            blocked.Add((ulong)(long)reader["uid"]);
                    }
                }
            } finally {
                _sem.Release();
            }

            return blocked.AsReadOnly();
        }

        public async Task AddBlockedUserAsync(ulong uid, string reason = null)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(reason)) {
                        cmd.CommandText = "INSERT INTO gf.blocked_users VALUES (@uid, NULL);";
                        cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);
                    } else {
                        cmd.CommandText = "INSERT INTO gf.blocked_users VALUES (@uid, @reason);";
                        cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);
                        cmd.Parameters.AddWithValue("reason", NpgsqlDbType.Varchar, reason);
                    }

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveBlockedUserAsync(ulong uid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.blocked_users WHERE uid = @uid;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<IReadOnlyList<ulong>> GetBlockedChannelsAsync()
        {
            return new List<ulong>();
        }

        public async Task AddBlockedChannelAsync(ulong cid)
        {

        }

        public async Task RemoveBlockedChannelAsync(ulong cid)
        {

        }
    }
}
