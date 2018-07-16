#region USING_DIRECTIVES
using Npgsql;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheGodfather.Modules.SWAT.Common;
#endregion

namespace TheGodfather.Services.Database.Swat
{
    internal static class DBServiceSwatExtensions
    {
        public static Task AddSwatServerAsync(this DBService db, string name, SwatServer server)
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

        public static Task RemoveSwatServerAsync(this DBService db, string name)
        {
            return db.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "DELETE FROM gf.swat_servers WHERE name = @name;";
                cmd.Parameters.Add(new NpgsqlParameter<string>("name", name));

                return cmd.ExecuteNonQueryAsync();
            });
        }
    }
}
