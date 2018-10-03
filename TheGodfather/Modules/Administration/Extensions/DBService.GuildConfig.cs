#region USING_DIRECTIVES
using DSharpPlus.Entities;

using Npgsql;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration.Extensions
{
    internal static class DBServiceGuildConfigExtensions
    {
        #region LOG_EXEMPTS
        public static Task<IReadOnlyList<ExemptedEntity>> GetAllLoggingExemptsAsync(this DBService db, ulong gid)
            => db.GetAllExemptsAsync("log_exempt", gid);

        public static Task<bool> IsExemptedFromLoggingAsync(this DBService db, ulong gid, ulong xid, EntityType type)
            => db.IsExemptedAsync("log_exempt", gid, xid, type);

        public static Task ExemptLoggingAsync(this DBService db, ulong gid, ulong xid, EntityType type)
            => db.ExemptAsync("log_exempt", gid, xid, type);

        public static Task UnexemptLoggingAsync(this DBService db, ulong gid, ulong xid, EntityType type)
            => db.UnexemptAsync("log_exempt", gid, xid, type);
        #endregion

        #region ANTISPAM_EXEMPTS
        public static Task<IReadOnlyList<ExemptedEntity>> GetAllAntispamExemptsAsync(this DBService db, ulong gid)
            => db.GetAllExemptsAsync("antispam_exempt", gid);

        public static Task<bool> IsExemptedFromAntispamAsync(this DBService db, ulong gid, ulong xid, EntityType type) 
            => db.IsExemptedAsync("antispam_exempt", gid, xid, type);

        public static Task ExemptAntispamAsync(this DBService db, ulong gid, ulong xid, EntityType type)
            => db.ExemptAsync("antispam_exempt", gid, xid, type);

        public static Task UnexemptAntispamAsync(this DBService db, ulong gid, ulong xid, EntityType type)
            => db.UnexemptAsync("antispam_exempt", gid, xid, type);
        #endregion

        #region PROTECTION_SETTINGS
        public static async Task<AntifloodSettings> GetAntifloodSettingsAsync(this DBService db, ulong gid)
        {
            var settings = new AntifloodSettings();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = $"SELECT antiflood_enabled, antiflood_sens, antiflood_cooldown, antiflood_action FROM gf.guild_cfg WHERE gid = @gid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    if (await reader.ReadAsync().ConfigureAwait(false)) {
                        settings.Action = (PunishmentActionType)(short)reader["antiflood_action"];
                        settings.Cooldown = (short)reader["antiflood_cooldown"];
                        settings.Enabled = (bool)reader["antiflood_enabled"];
                        settings.Sensitivity = (short)reader["antiflood_sens"];
                    }
                }
            });

            return settings;
        }

        public static async Task<AntiInstantLeaveSettings> GetAntiInstantLeaveSettingsAsync(this DBService db, ulong gid)
        {
            var settings = new AntiInstantLeaveSettings();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = $"SELECT antijoinleave_enabled, antijoinleave_cooldown FROM gf.guild_cfg WHERE gid = @gid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    if (await reader.ReadAsync().ConfigureAwait(false)) {
                        settings.Enabled = (bool)reader["antijoinleave_enabled"];
                        settings.Cooldown = (short)reader["antijoinleave_cooldown"];
                    }
                }
            });

            return settings;
        }

        public static Task SetAntifloodSettingsAsync(this DBService db, ulong gid, AntifloodSettings settings)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = $"UPDATE gf.guild_cfg SET " +
                    $"(antiflood_enabled, antiflood_sens, antiflood_cooldown, antiflood_action) = " +
                    $"(@antiflood_enabled, @antiflood_sens, @antiflood_cooldown, @antiflood_action) " +
                    $"WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("antiflood_enabled", settings.Enabled));
                cmd.Parameters.Add(new NpgsqlParameter<short>("antiflood_action", (short)settings.Action));
                cmd.Parameters.Add(new NpgsqlParameter<short>("antiflood_cooldown", settings.Cooldown));
                cmd.Parameters.Add(new NpgsqlParameter<short>("antiflood_sens", settings.Sensitivity));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task SetAntiInstantLeaveSettingsAsync(this DBService db, ulong gid, AntiInstantLeaveSettings settings)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET (antijoinleave_enabled, antijoinleave_cooldown) = " +
                                  "(@antijoinleave_enabled, @antijoinleave_cooldown) WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("antijoinleave_enabled", settings.Enabled));
                cmd.Parameters.Add(new NpgsqlParameter<short>("antijoinleave_cooldown", settings.Cooldown));

                return cmd.ExecuteNonQueryAsync();
            });
        }
        #endregion

        #region CONFIG
        public static async Task<DiscordRole> GetMuteRoleAsync(this DBService db, DiscordGuild guild)
        {
            ulong rid = (ulong)await db.GetValueInternalAsync<long>(guild.Id, "mute_rid");
            return rid != 0 ? guild.GetRole(rid) : null;
        }

        public static async Task<DiscordChannel> GetWelcomeChannelAsync(this DBService db, DiscordGuild guild)
        {
            ulong cid = (ulong)await db.GetValueInternalAsync<long>(guild.Id, "welcome_cid");
            return cid != 0 ? guild.GetChannel(cid) : null;
        }

        public static async Task<DiscordChannel> GetLeaveChannelAsync(this DBService db, DiscordGuild guild)
        {
            ulong cid = (ulong)await db.GetValueInternalAsync<long>(guild.Id, "leave_cid");
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

        public static async Task<string> GetLeaveMessageAsync(this DBService db, ulong gid)
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

        public static Task RemoveWelcomeChannelAsync(this DBService db, ulong gid)
            => db.SetValueInternalAsync(gid, "welcome_cid", 0);

        public static Task RemoveWelcomeMessageAsync(this DBService db, ulong gid)
            => db.SetWelcomeMessageAsync(gid, null);

        public static Task RemoveLeaveChannelAsync(this DBService db, ulong gid)
            => db.SetValueInternalAsync(gid, "leave_cid", 0);

        public static Task RemoveLeaveMessageAsync(this DBService db, ulong gid)
            => db.SetLeaveMessageAsync(gid, null);

        public static Task SetMuteRoleAsync(this DBService db, ulong gid, ulong rid)
            => db.SetValueInternalAsync(gid, "mute_rid", (long)rid);

        public static Task SetWelcomeChannelAsync(this DBService db, ulong gid, ulong cid)
            => db.SetValueInternalAsync(gid, "welcome_cid", (long)cid);

        public static Task SetWelcomeMessageAsync(this DBService db, ulong gid, string message)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET welcome_msg = @message WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("message", message));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task SetLeaveChannelAsync(this DBService db, ulong gid, ulong cid)
            => db.SetValueInternalAsync(gid, "leave_cid", (long)cid);

        public static Task SetLeaveMessageAsync(this DBService db, ulong gid, string message)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "UPDATE gf.guild_cfg SET leave_msg = @message WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<string>("message", message));

                return cmd.ExecuteNonQueryAsync();
            });
        }
        #endregion


        #region HELPER_FUNCTIONS
        private static async Task<T> GetValueInternalAsync<T>(this DBService db, ulong gid, string col)
        {
            T value = default;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = $"SELECT {col} FROM gf.guild_cfg WHERE gid = @gid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    value = (T)res;
            });

            return value;
        }

        private static Task SetValueInternalAsync<T>(this DBService db, ulong gid, string col, T value)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = $"UPDATE gf.guild_cfg SET {col} = @value WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<T>("value", value));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        private static async Task<IReadOnlyList<ExemptedEntity>> GetAllExemptsAsync(this DBService db, string table, ulong gid)
        {
            var exempted = new List<ExemptedEntity>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = $"SELECT id, type FROM gf.{table} WHERE gid = @gid;";
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

        private static async Task<bool> IsExemptedAsync(this DBService db, string table, ulong gid, ulong xid, EntityType type)
        {
            bool exempted = false;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = $"SELECT id FROM gf.{table} WHERE gid = @gid AND id = @xid AND type = @type LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("xid", (long)xid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<char>("type", type.ToFlag()));

                object res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    exempted = true;
            });

            return exempted;
        }

        private static Task ExemptAsync(this DBService db, string table, ulong gid, ulong xid, EntityType type)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = $"INSERT INTO gf.{table}(gid, id, type) VALUES (@gid, @xid, @type) ON CONFLICT DO NOTHING;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("xid", (long)xid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<char>("type", type.ToFlag()));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        private static Task UnexemptAsync(this DBService db, string table, ulong gid, ulong xid, EntityType type)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = $"DELETE FROM gf.{table} WHERE gid = @gid AND id = @xid AND type = @type;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("xid", (long)xid));
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter<char>("type", type.ToFlag()));

                return cmd.ExecuteNonQueryAsync();
            });
        }
        #endregion
    }
}
