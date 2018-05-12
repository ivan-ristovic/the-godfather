#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using TheGodfather.Common;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task<IReadOnlyDictionary<ulong, PartialGuildConfig>> GetPartialGuildConfigurations()
        {
            var dict = new Dictionary<ulong, PartialGuildConfig>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.guild_cfg;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            dict.Add((ulong)(long)reader["gid"], new PartialGuildConfig() {
                                Prefix = reader["prefix"] is DBNull ? null : (string)reader["prefix"],
                                SuggestionsEnabled = (bool)reader["suggestions_enabled"],
                            });
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return new ReadOnlyDictionary<ulong, PartialGuildConfig>(dict);
        }


        public async Task RegisterGuildAsync(ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.guild_cfg VALUES (@gid, NULL, NULL) ON CONFLICT DO NOTHING;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task UnregisterGuildAsync(ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.guild_cfg WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }


        public async Task SetPrefixAsync(ulong gid, string prefix)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "UPDATE gf.guild_cfg SET prefix = @prefix WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    cmd.Parameters.AddWithValue("prefix", NpgsqlDbType.Varchar, prefix);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task ResetPrefixAsync(ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "UPDATE gf.guild_cfg SET prefix = NULL WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }


        public async Task<ulong> GetWelcomeChannelIdAsync(ulong gid)
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

        public async Task<ulong> GetLeaveChannelIdAsync(ulong gid)
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

        public async Task<string> GetLeaveMessageAsync(ulong gid)
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

        public async Task<string> GetWelcomeMessageAsync(ulong gid)
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

        public Task RemoveWelcomeChannelAsync(ulong gid)
            => SetWelcomeChannelAsync(gid, 0);

        public Task RemoveWelcomeMessageAsync(ulong gid)
            => SetWelcomeMessageAsync(gid, null);

        public Task RemoveLeaveChannelAsync(ulong gid)
            => SetLeaveChannelAsync(gid, 0);

        public Task RemoveLeaveMessageAsync(ulong gid)
            => SetLeaveMessageAsync(gid, null);

        public async Task SetWelcomeChannelAsync(ulong gid, ulong cid)
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

        public async Task SetWelcomeMessageAsync(ulong gid, string message)
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

        public async Task SetLeaveChannelAsync(ulong gid, ulong cid)
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

        public async Task SetLeaveMessageAsync(ulong gid, string message)
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
    }
}
