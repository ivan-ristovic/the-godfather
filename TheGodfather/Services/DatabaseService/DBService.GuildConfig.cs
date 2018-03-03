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
        public async Task<ulong> GetGuildWelcomeChannelIdAsync(ulong gid)
        {
            ulong cid = 0;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT welcome_cid FROM gf.guild_cfg WHERE gid = @gid LIMIT 1;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        cid = (ulong)(long)res;
                }
            } finally {
                _sem.Release();
            }

            return cid;
        }

        public async Task<ulong> GetGuildLeaveChannelIdAsync(ulong gid)
        {
            ulong cid = 0;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT leave_cid FROM gf.guild_cfg WHERE gid = @gid LIMIT 1;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        cid = (ulong)(long)res;
                }
            } finally {
                _sem.Release();
            }

            return cid;
        }

        public async Task SetGuildWelcomeChannelAsync(ulong gid, ulong cid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "UPDATE gf.guild_cfg SET welcome_cid = @cid WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task SetGuildLeaveChannelAsync(ulong gid, ulong cid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "UPDATE gf.guild_cfg SET leave_cid = @cid WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveGuildWelcomeChannelAsync(ulong gid)
            => await SetGuildWelcomeChannelAsync(gid, 0).ConfigureAwait(false);

        public async Task RemoveGuildLeaveChannelAsync(ulong gid)
            => await SetGuildLeaveChannelAsync(gid, 0).ConfigureAwait(false);

        public async Task<IReadOnlyList<ulong>> GetSelfAssignableRolesListAsync(ulong gid)
        {
            var roles = new List<ulong>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.assignable_roles WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            roles.Add((ulong)(long)reader["rid"]);
                    }
                }
            } finally {
                _sem.Release();
            }

            return roles.AsReadOnly();
        }

        public async Task AddSelfAssignableRoleAsync(ulong gid, ulong rid)
        {
            if (await SelfAssignableRoleExistsAsync(gid, rid))
                return;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.assignable_roles VALUES (@gid, @rid);";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, rid);

                    var res = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveSelfAssignableRoleAsync(ulong gid, ulong rid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.assignable_roles WHERE gid = @gid AND rid = @rid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, rid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<bool> SelfAssignableRoleExistsAsync(ulong gid, ulong rid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT rid FROM gf.assignable_roles WHERE gid = @gid AND rid = @rid LIMIT 1;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("rid", NpgsqlDbType.Bigint, rid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        return true;
                }
            } finally {
                _sem.Release();
            }
            return false;
        }

        public async Task DeleteAllSelfAssignableRolesAsync(ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.assignable_roles WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
