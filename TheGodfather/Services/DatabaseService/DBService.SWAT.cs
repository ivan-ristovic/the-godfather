#region USING_DIRECTIVES
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheGodfather.Modules.SWAT.Common;
#endregion

namespace TheGodfather.Services.Database.Swat
{
    internal static class DBServiceSwatExtensions
    {
        public static Task AddSwatIpBanAsync(this DBService db, string ip, string name, string reason = null)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.swat_banlist(name, ip, reason) VALUES (@ip, @name, @reason);";
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", name));
                cmd.Parameters.Add(new NpgsqlParameter<string>("ip", ip));

                if (string.IsNullOrWhiteSpace(reason))
                    cmd.Parameters.AddWithValue("reason", NpgsqlDbType.Varchar, DBNull.Value);
                else
                    cmd.Parameters.Add(new NpgsqlParameter<string>("reason", reason));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task AddSwatIpEntryAsync(this DBService db, string ip, string name, string info = null)
        {
            return db.ExecuteCommandAsync(cmd => {
                if (string.IsNullOrWhiteSpace(info)) {
                    cmd.CommandText = "INSERT INTO gf.swat_ips(name, ip, additional_info) VALUES (@ip, @name, default);";
                } else {
                    cmd.CommandText = "INSERT INTO gf.swat_ips(name, ip, additional_info) VALUES (@ip, @name, @info);";
                    cmd.Parameters.Add(new NpgsqlParameter<string>("info", info));
                }
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", name));
                cmd.Parameters.Add(new NpgsqlParameter<string>("ip", ip));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task AddSwatServerAsync(this DBService db, SwatServer server)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "INSERT INTO gf.swat_servers(ip, joinport, queryport, name) VALUES (@ip, @joinport, @queryport, @name);";
                cmd.Parameters.Add(new NpgsqlParameter<string>("ip", server.Ip));
                cmd.Parameters.Add(new NpgsqlParameter<int>("joinport", server.JoinPort));
                cmd.Parameters.Add(new NpgsqlParameter<int>("queryport", server.QueryPort));
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", server.Name));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<IReadOnlyList<SwatDatabaseEntry>> GetAllSwatBanlistEntriesAsync(this DBService db)
        {
            var entries = new List<SwatDatabaseEntry>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT name, ip, reason FROM gf.swat_banlist ORDER BY name;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        entries.Add(new SwatDatabaseEntry() {
                            Ip = (string)reader["ip"],
                            Name = (string)reader["name"],
                            AdditionalInfo = reader["reason"] is DBNull ? null : (string)reader["reason"]
                        });
                    }
                }
            });

            return entries.AsReadOnly();
        }

        public static async Task<IReadOnlyList<SwatDatabaseEntry>> GetAllSwatIpEntriesAsync(this DBService db)
        {
            var entries = new List<SwatDatabaseEntry>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT name, ip, additional_info FROM gf.swat_ips ORDER BY name;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        entries.Add(new SwatDatabaseEntry() {
                            Ip = (string)reader["ip"],
                            Name = (string)reader["name"],
                            AdditionalInfo = reader["additional_info"] is DBNull ? null : (string)reader["additional_info"]
                        });
                    }
                }
            });

            return entries.AsReadOnly();
        }

        public static async Task<IReadOnlyList<SwatServer>> GetAllSwatServersAsync(this DBService db)
        {
            var servers = new List<SwatServer>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT name, ip, joinport, queryport FROM gf.swat_servers;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        servers.Add(new SwatServer(
                            (string)reader["name"],
                            (string)reader["ip"],
                            (int)reader["joinport"],
                            (int)reader["queryport"]
                        ));
                    }
                }
            });

            return servers.AsReadOnly();
        }

        public static async Task<SwatServer> GetSwatServerAsync(this DBService db, string ip, int queryport, string name = null)
            => await db.GetSwatServerFromDatabaseAsync(name) ?? SwatServer.FromIP(ip, queryport, name);

        public static async Task<SwatServer> GetSwatServerFromDatabaseAsync(this DBService db, string name)
        {
            if (name == null)
                return null;

            SwatServer server = null;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.swat_servers WHERE name = @name LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", name));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    if (await reader.ReadAsync().ConfigureAwait(false)) {
                        server = new SwatServer(
                            (string)reader["name"],
                            (string)reader["ip"],
                            (int)reader["joinport"],
                            (int)reader["queryport"]
                        );
                    }
                }
            });

            return server;
        }

        public static Task RemoveSwatIpBanAsync(this DBService db, string ip)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.swat_banlist WHERE ip = @ip;";
                cmd.Parameters.Add(new NpgsqlParameter<string>("ip", ip));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveSwatIpEntryAsync(this DBService db, string ip)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.swat_ips WHERE ip = @ip;";
                cmd.Parameters.Add(new NpgsqlParameter<string>("ip", ip));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static Task RemoveSwatServerAsync(this DBService db, string name)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.swat_servers WHERE name = @name;";
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", name));

                return cmd.ExecuteNonQueryAsync();
            });
        }

        public static async Task<IReadOnlyList<SwatDatabaseEntry>> SwatDatabaseNameSearchAsync(this DBService db, string name, int limit = 10)
        {
            var entries = new List<SwatDatabaseEntry>();
            
            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT name, ip, additional_info FROM gf.swat_ips WHERE LOWER(name) LIKE @name ORDER BY ABS(gf.levenshtein(LOWER(name), @name)) LIMIT @limit;";
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", $"%{name.ToLower()}%"));
                cmd.Parameters.Add(new NpgsqlParameter<int>("limit", limit));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        entries.Add(new SwatDatabaseEntry() {
                            Ip = (string)reader["ip"],
                            Name = (string)reader["name"],
                            AdditionalInfo = reader["additional_info"] is DBNull ? null : (string)reader["additional_info"]
                        });
                    }
                }
            });

            return entries.AsReadOnly();
        }

        public static async Task<IReadOnlyList<SwatDatabaseEntry>> SwatDatabaseIpSearchAsync(this DBService db, string ip, int limit = 10)
        {
            var entries = new List<SwatDatabaseEntry>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT name, ip, additional_info FROM gf.swat_ips WHERE ip LIKE @ip LIMIT @limit;";
                cmd.Parameters.Add(new NpgsqlParameter<string>("ip", ip + '%'));
                cmd.Parameters.Add(new NpgsqlParameter<int>("limit", limit));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        entries.Add(new SwatDatabaseEntry() {
                            Ip = (string)reader["ip"],
                            Name = (string)reader["name"],
                            AdditionalInfo = reader["additional_info"] is DBNull ? null : (string)reader["additional_info"]
                        });
                    }
                }
            });

            return entries.AsReadOnly();
        }
    }
}
