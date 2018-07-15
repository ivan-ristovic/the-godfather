#region USING_DIRECTIVES
using DSharpPlus.Entities;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TheGodfather.Common;
#endregion

namespace TheGodfather.Services.Database.GuildConfig
{
    internal static class DBServiceGuildConfigExtensions
    {
        public static async Task<IReadOnlyDictionary<ulong, CachedGuildConfig>> GetAllPartialGuildConfigurations(this DBService db)
        {
            var dict = new Dictionary<ulong, CachedGuildConfig>();

            await db.ExecuteCommandAsync(async (cmd) => {
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
            });

            return new ReadOnlyDictionary<ulong, CachedGuildConfig>(dict);
        }

        public static async Task<DiscordChannel> GetLeaveChannelAsync(this DBService db, DiscordGuild guild)
        {
            ulong cid = 0;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT leave_cid FROM gf.guild_cfg WHERE gid = @gid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)guild.Id));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    cid = (ulong)(long)res;
            });

            return cid != 0 ? guild.GetChannel(cid) : null;
        }

        public static async Task<string> GetLeaveMessageForGuildAsync(this DBService db, ulong gid)
        {
            string msg = null;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT leave_msg FROM gf.guild_cfg WHERE gid = @gid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    msg = (string)res;
            });

            return msg;
        }

        public static async Task<DiscordChannel> GetWelcomeChannelAsync(this DBService db, DiscordGuild guild)
        {
            ulong cid = 0;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT welcome_cid FROM gf.guild_cfg WHERE gid = @gid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)guild.Id));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    cid = (ulong)(long)res;
            });

            return cid != 0 ? guild.GetChannel(cid) : null;
        }

        public static async Task<string> GetWelcomeMessageAsync(this DBService db, ulong gid)
        {
            string msg = null;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT welcome_msg FROM gf.guild_cfg WHERE gid = @gid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    msg = (string)res;
            });

            return msg;
        }

        public static Task RegisterGuildAsync(this DBService db, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.guild_cfg VALUES (@gid) ON CONFLICT DO NOTHING;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveWelcomeChannelAsync(this DBService db, ulong gid)
            => db.SetWelcomeChannelAsync(gid, 0);

        public static Task RemoveWelcomeMessageAsync(this DBService db, ulong gid)
            => db.SetWelcomeMessageAsync(gid, null);

        public static Task RemoveLeaveChannelAsync(this DBService db, ulong gid)
            => db.SetLeaveChannelAsync(gid, 0);

        public static Task RemoveLeaveMessageAsync(this DBService db, ulong gid)
            => db.SetLeaveMessageAsync(gid, null);

        public static Task ResetPrefixAsync(this DBService db, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET prefix = NULL WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task SetLeaveChannelAsync(this DBService db, ulong gid, ulong cid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET leave_cid = @cid WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter("cid", (long)cid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task SetLeaveMessageAsync(this DBService db, ulong gid, string message)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET leave_msg = @message WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));
                if (string.IsNullOrWhiteSpace(message))
                    cmd.Parameters.Add(new NpgsqlParameter<string>("message", message));
                else
                    cmd.Parameters.Add(new NpgsqlParameter("message", message));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task SetPrefixAsync(this DBService db, ulong gid, string prefix)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET prefix = @prefix WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter("prefix", prefix));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task SetWelcomeChannelAsync(this DBService db, ulong gid, ulong cid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET welcome_cid = @cid WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter("cid", (long)cid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task SetWelcomeMessageAsync(this DBService db, ulong gid, string message)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET welcome_msg = @message WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));
                if (string.IsNullOrWhiteSpace(message))
                    cmd.Parameters.Add(new NpgsqlParameter<string>("message", message));
                else
                    cmd.Parameters.Add(new NpgsqlParameter("message", message));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task UnregisterGuildAsync(this DBService db, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.guild_cfg WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task UpdateGuildSettingsAsync(this DBService db, ulong gid, CachedGuildConfig cfg)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET (prefix, suggestions_enabled, log_cid, linkfilter_enabled, linkfilter_invites, linkfilter_booters, linkfilter_disturbing, linkfilter_iploggers, linkfilter_shorteners) = (@prefix, @suggestions_enabled, @log_cid, @linkfilter_enabled, @linkfilter_invites, @linkfilter_booters, @linkfilter_disturbing, @linkfilter_iploggers, @linkfilter_shorteners) WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));
                if (string.IsNullOrWhiteSpace(cfg.Prefix))
                    cmd.Parameters.Add(new NpgsqlParameter<string>("prefix", null));
                else
                    cmd.Parameters.Add(new NpgsqlParameter("prefix", cfg.Prefix));
                cmd.Parameters.Add(new NpgsqlParameter("log_cid", (long)cfg.LogChannelId));
                cmd.Parameters.Add(new NpgsqlParameter("suggestions_enabled", cfg.SuggestionsEnabled));
                cmd.Parameters.Add(new NpgsqlParameter("linkfilter_enabled", cfg.LinkfilterEnabled));
                cmd.Parameters.Add(new NpgsqlParameter("linkfilter_invites", cfg.BlockDiscordInvites));
                cmd.Parameters.Add(new NpgsqlParameter("linkfilter_booters", cfg.BlockBooterWebsites));
                cmd.Parameters.Add(new NpgsqlParameter("linkfilter_disturbing", cfg.BlockDisturbingWebsites));
                cmd.Parameters.Add(new NpgsqlParameter("linkfilter_iploggers", cfg.BlockIpLoggingWebsites));
                cmd.Parameters.Add(new NpgsqlParameter("linkfilter_shorteners", cfg.BlockUrlShorteners));

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
