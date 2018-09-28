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
        public static async Task<IReadOnlyDictionary<ulong, CachedGuildConfig>> GetAllCachedGuildConfigurationsAsync(this DBService db)
        {
            var dict = new Dictionary<ulong, CachedGuildConfig>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.guild_cfg;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        dict.Add((ulong)(long)reader["gid"], new CachedGuildConfig() {
                            AntispamSettings = new AntispamSettings() {
                                Enabled = (bool)reader["antispam_enabled"],
                                Action = (PunishmentActionType)(short)reader["antispam_action"],
                                Sensitivity = (short)reader["antispam_sens"],
                            },
                            Currency = reader["currency"] is DBNull ? null : (string)reader["currency"],
                            LinkfilterSettings = new LinkfilterSettings() {
                                Enabled = (bool)reader["linkfilter_enabled"],
                                BlockBooterWebsites = (bool)reader["linkfilter_booters"],
                                BlockDiscordInvites = (bool)reader["linkfilter_invites"],
                                BlockDisturbingWebsites = (bool)reader["linkfilter_disturbing"],
                                BlockIpLoggingWebsites = (bool)reader["linkfilter_iploggers"],
                                BlockUrlShorteners = (bool)reader["linkfilter_shorteners"],
                            },
                            LogChannelId = (ulong)(long)reader["log_cid"],
                            Prefix = reader["prefix"] is DBNull ? null : (string)reader["prefix"],
                            RatelimitSettings = new RatelimitSettings() {
                                Enabled = (bool)reader["ratelimit_enabled"],
                                Action = (PunishmentActionType)(short)reader["ratelimit_action"],
                                Sensitivity = (short)reader["ratelimit_sens"],
                            },
                            ReactionResponse = (bool)reader["silent_respond"],
                            SuggestionsEnabled = (bool)reader["suggestions_enabled"],
                        });
                    }
                }
            });

            return new ReadOnlyDictionary<ulong, CachedGuildConfig>(dict);
        }
        
        public static Task RegisterGuildAsync(this DBService db, ulong gid)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.guild_cfg VALUES (@gid) ON CONFLICT DO NOTHING;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));

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
                cmd.CommandText = "UPDATE gf.guild_cfg SET " +
                    "(prefix, silent_respond, suggestions_enabled, log_cid, linkfilter_enabled, " +
                    "linkfilter_invites, linkfilter_booters, linkfilter_disturbing, linkfilter_iploggers, " +
                    "linkfilter_shorteners, currency, ratelimit_enabled, ratelimit_action, ratelimit_sens," +
                    "antispam_enabled, antispam_sens, antispam_action) = " +
                    "(@prefix, @silent_respond, @suggestions_enabled, @log_cid, @linkfilter_enabled, " +
                    "@linkfilter_invites, @linkfilter_booters, @linkfilter_disturbing, @linkfilter_iploggers, " +
                    "@linkfilter_shorteners, @currency, @ratelimit_enabled, @ratelimit_action, @ratelimit_sens," +
                    "@antispam_enabled, @antispam_sens, @antispam_action) " +
                    "WHERE gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter<long>("gid", (long)gid));
                if (string.IsNullOrWhiteSpace(cfg.Prefix))
                    cmd.Parameters.AddWithValue("prefix", NpgsqlDbType.Varchar, DBNull.Value);
                else
                    cmd.Parameters.Add(new NpgsqlParameter<string>("prefix", cfg.Prefix));
                cmd.Parameters.Add(new NpgsqlParameter<long>("log_cid", (long)cfg.LogChannelId));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("silent_respond", cfg.ReactionResponse));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("suggestions_enabled", cfg.SuggestionsEnabled));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("linkfilter_enabled", cfg.LinkfilterSettings.Enabled));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("linkfilter_invites", cfg.LinkfilterSettings.BlockDiscordInvites));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("linkfilter_booters", cfg.LinkfilterSettings.BlockBooterWebsites));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("linkfilter_disturbing", cfg.LinkfilterSettings.BlockDisturbingWebsites));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("linkfilter_iploggers", cfg.LinkfilterSettings.BlockIpLoggingWebsites));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("linkfilter_shorteners", cfg.LinkfilterSettings.BlockUrlShorteners));
                if (string.IsNullOrWhiteSpace(cfg.Currency))
                    cmd.Parameters.AddWithValue("currency", NpgsqlDbType.Varchar, DBNull.Value);
                else
                    cmd.Parameters.Add(new NpgsqlParameter<string>("currency", cfg.Currency));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("ratelimit_enabled", cfg.RatelimitSettings.Enabled));
                cmd.Parameters.Add(new NpgsqlParameter<short>("ratelimit_action", (short)cfg.RatelimitSettings.Action));
                cmd.Parameters.Add(new NpgsqlParameter<short>("ratelimit_sens", cfg.RatelimitSettings.Sensitivity));
                cmd.Parameters.Add(new NpgsqlParameter<bool>("antispam_enabled", cfg.AntispamSettings.Enabled));
                cmd.Parameters.Add(new NpgsqlParameter<short>("antispam_action", cfg.AntispamSettings.Sensitivity));
                cmd.Parameters.Add(new NpgsqlParameter<short>("antispam_sens", (short)cfg.AntispamSettings.Action));

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
