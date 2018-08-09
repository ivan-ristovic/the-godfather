#region USING_DIRECTIVES
using DSharpPlus.Entities;

using Npgsql;

using NpgsqlTypes;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Modules.Administration.Common;
#endregion

namespace TheGodfather.Services
{
    internal static class DBServiceGuildConfigExtensions
    {
        public static Task ExemptAsync(this DBService db, ulong gid, ulong xid, EntityType type)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.log_exempt(gid, id, type) VALUES (@gid, @xid, @type) ON CONFLICT DO NOTHING;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("xid", (long)xid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<char>("type", type.ToFlag()));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<IReadOnlyList<ExemptedEntity>> GetAllExemptedEntitiesAsync(this DBService db, ulong gid)
        {
            var exempted = new List<ExemptedEntity>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT id, type FROM gf.log_exempt WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        EntityType t = EntityType.Channel;
                        switch (((string)reader["type"]).First()) {
                            case 'c': t = EntityType.Channel; break;
                            case 'm': t = EntityType.Member; break;
                            case 'r': t = EntityType.Role; break;
                        }
                        exempted.Add(new ExemptedEntity() {
                            GuildId = gid,
                            Id = (ulong)(long)reader["id"],
                            Type = t
                        });
                    }
                }
            });

            return exempted.AsReadOnly();
        }

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
                            ReactionResponse = (bool)reader["silent_respond"],
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
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)guild.Id));

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
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

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
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)guild.Id));

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
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    msg = (string)res;
            });

            return msg;
        }

        public static async Task<bool> IsEntityExemptedAsync(this DBService db, ulong gid, ulong xid, EntityType type)
        {
            bool exempted = false;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT id FROM gf.log_exempt WHERE gid = @gid AND id = @xid AND type = @type LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("xid", (long)xid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<char>("type", type.ToFlag()));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    exempted = true;
            });

            return exempted;
        }

        public static Task RegisterGuildAsync(this DBService db, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.guild_cfg VALUES (@gid) ON CONFLICT DO NOTHING;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

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
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task SetLeaveChannelAsync(this DBService db, ulong gid, ulong cid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET leave_cid = @cid WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("cid", (long)cid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task SetLeaveMessageAsync(this DBService db, ulong gid, string message)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET leave_msg = @message WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("message", message));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task SetPrefixAsync(this DBService db, ulong gid, string prefix)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET prefix = @prefix WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("prefix", prefix));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task SetWelcomeChannelAsync(this DBService db, ulong gid, ulong cid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET welcome_cid = @cid WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("cid", (long)cid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task SetWelcomeMessageAsync(this DBService db, ulong gid, string message)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET welcome_msg = @message WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("message", message));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task UnregisterGuildAsync(this DBService db, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.guild_cfg WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task UpdateGuildSettingsAsync(this DBService db, ulong gid, CachedGuildConfig cfg)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET (prefix, silent_respond, suggestions_enabled, log_cid, linkfilter_enabled, linkfilter_invites, linkfilter_booters, linkfilter_disturbing, linkfilter_iploggers, linkfilter_shorteners) = (@prefix, @silent_respond, @suggestions_enabled, @log_cid, @linkfilter_enabled, @linkfilter_invites, @linkfilter_booters, @linkfilter_disturbing, @linkfilter_iploggers, @linkfilter_shorteners) WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                if (string.IsNullOrWhiteSpace(cfg.Prefix))
                    cmd.Parameters.AddWithValue("prefix", NpgsqlDbType.Varchar, DBNull.Value);
                else
                    cmd.Parameters.Add(new NpgsqlParameter<string>("prefix", cfg.Prefix));
                cmd.Parameters.Add(new NpgsqlParameter<long>("log_cid", (long)cfg.LogChannelId));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("silent_respond", cfg.ReactionResponse));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("suggestions_enabled", cfg.SuggestionsEnabled));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("linkfilter_enabled", cfg.LinkfilterEnabled));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("linkfilter_invites", cfg.BlockDiscordInvites));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("linkfilter_booters", cfg.BlockBooterWebsites));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("linkfilter_disturbing", cfg.BlockDisturbingWebsites));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("linkfilter_iploggers", cfg.BlockIpLoggingWebsites));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("linkfilter_shorteners", cfg.BlockUrlShorteners));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task UnexemptAsync(this DBService db, ulong gid, ulong xid, EntityType type)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.log_exempt WHERE gid = @gid AND id = @xid AND type = @type;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("xid", (long)xid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<char>("type", type.ToFlag()));

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
