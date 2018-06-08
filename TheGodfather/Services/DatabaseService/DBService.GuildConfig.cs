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
        public async Task<IReadOnlyDictionary<ulong, CachedGuildConfig>> GetPartialGuildConfigurations()
        {
            var dict = new Dictionary<ulong, CachedGuildConfig>();

            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "SELECT * FROM gf.guild_cfg;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            dict.Add((ulong)(long)reader["gid"], new CachedGuildConfig() {
                                BlockBooterWebsites = (bool)reader["linkfilter_booters"],
                                BlockDiscordInvites = (bool)reader["linkfilter_invites"],
                                BlockDisturbingWebsites = (bool)reader["linkfilter_disturbing"],
                                BlockIpLoggingWebsites = (bool)reader["linkfilter_iploggers"],
                                BlockUrlShorteners = (bool)reader["linkfilter_shorteners"],
                                LinkfilterEnabled = (bool)reader["linkfilter_enabled"],
                                LogChannelId = (ulong)(long)reader["log_cid"],
                                Prefix = reader["prefix"] is DBNull ? null : (string)reader["prefix"],
                                SuggestionsEnabled = (bool)reader["suggestions_enabled"],
                            });
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return new ReadOnlyDictionary<ulong, CachedGuildConfig>(dict);
        }


        public async Task RegisterGuildAsync(ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "INSERT INTO gf.guild_cfg VALUES (@gid) ON CONFLICT DO NOTHING;";
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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "UPDATE gf.guild_cfg SET prefix = NULL WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }


        public async Task UpdateGuildSettingsAsync(ulong gid, CachedGuildConfig cfg)
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
                    cmd.CommandText = "UPDATE gf.guild_cfg SET (prefix, suggestions_enabled, log_cid, linkfilter_enabled, linkfilter_invites, linkfilter_booters, linkfilter_disturbing, linkfilter_iploggers, linkfilter_shorteners) = (@prefix, @suggestions_enabled, @log_cid, @linkfilter_enabled, @linkfilter_invites, @linkfilter_booters, @linkfilter_disturbing, @linkfilter_iploggers, @linkfilter_shorteners) WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                    if (string.IsNullOrWhiteSpace(cfg.Prefix))
                        cmd.Parameters.AddWithValue("prefix", NpgsqlDbType.Varchar, DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("prefix", NpgsqlDbType.Varchar, cfg.Prefix);
                    cmd.Parameters.AddWithValue("suggestions_enabled", NpgsqlDbType.Boolean, cfg.SuggestionsEnabled);
                    cmd.Parameters.AddWithValue("log_cid", NpgsqlDbType.Bigint, cfg.LogChannelId);
                    cmd.Parameters.AddWithValue("linkfilter_enabled", NpgsqlDbType.Boolean, cfg.LinkfilterEnabled);
                    cmd.Parameters.AddWithValue("linkfilter_invites", NpgsqlDbType.Boolean, cfg.BlockDiscordInvites);
                    cmd.Parameters.AddWithValue("linkfilter_booters", NpgsqlDbType.Boolean, cfg.BlockBooterWebsites);
                    cmd.Parameters.AddWithValue("linkfilter_disturbing", NpgsqlDbType.Boolean, cfg.BlockDisturbingWebsites);
                    cmd.Parameters.AddWithValue("linkfilter_iploggers", NpgsqlDbType.Boolean, cfg.BlockIpLoggingWebsites);
                    cmd.Parameters.AddWithValue("linkfilter_shorteners", NpgsqlDbType.Boolean, cfg.BlockUrlShorteners);
                    
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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
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
                using (var cmd = await OpenConnectionAndCreateCommandAsync()) {
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
