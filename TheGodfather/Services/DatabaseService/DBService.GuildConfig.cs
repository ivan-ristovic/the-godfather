#region USING_DIRECTIVES
using System;
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

        public async Task<string> GetGuildWelcomeMessageAsync(ulong gid)
        {
            string msg = null;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT welcome_msg FROM gf.guild_cfg WHERE gid = @gid LIMIT 1;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        msg = (string)res;
                }
            } finally {
                _sem.Release();
            }

            return msg;
        }

        public async Task<string> GetGuildLeaveMessageAsync(ulong gid)
        {
            string msg = null;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT leave_msg FROM gf.guild_cfg WHERE gid = @gid LIMIT 1;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        msg = (string)res;
                }
            } finally {
                _sem.Release();
            }

            return msg;
        }

        public async Task SetGuildWelcomeMessageAsync(ulong gid, string message)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "UPDATE gf.guild_cfg SET welcome_msg = @message WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    if (string.IsNullOrWhiteSpace(message))
                        cmd.Parameters.AddWithValue("message", NpgsqlDbType.Varchar, DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("message", NpgsqlDbType.Varchar, message);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task SetGuildLeaveMessageAsync(ulong gid, string message)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "UPDATE gf.guild_cfg SET leave_msg = @message WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    if (string.IsNullOrWhiteSpace(message))
                        cmd.Parameters.AddWithValue("message", NpgsqlDbType.Varchar, DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("message", NpgsqlDbType.Varchar, message);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveGuildWelcomeMessageAsync(ulong gid)
            => await SetGuildWelcomeMessageAsync(gid, null).ConfigureAwait(false);

        public async Task RemoveGuildLeaveMessageAsync(ulong gid)
            => await SetGuildLeaveMessageAsync(gid, null).ConfigureAwait(false);
    }
}
