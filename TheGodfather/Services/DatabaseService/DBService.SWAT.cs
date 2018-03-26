#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Modules.SWAT.Common;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        public async Task AddSwatServerAsync(string name, SwatServer server)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.swat_servers(ip, joinport, queryport, name) VALUES (@ip, @joinport, @queryport, @name);";
                    cmd.Parameters.AddWithValue("ip", NpgsqlDbType.Varchar, server.IP);
                    cmd.Parameters.AddWithValue("joinport", NpgsqlDbType.Integer, server.JoinPort);
                    cmd.Parameters.AddWithValue("queryport", NpgsqlDbType.Integer, server.QueryPort);
                    cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, server.Name);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<IReadOnlyList<SwatServer>> GetAllSwatServersAsync()
        {
            var servers = new List<SwatServer>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

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
                }
            } finally {
                _sem.Release();
            }

            return servers.AsReadOnly();
        }

        public async Task<SwatServer> GetSwatServerAsync(string ip, int queryport, string name = null)
        {
            var server = await GetSwatServerFromDatabaseAsync(name)
                .ConfigureAwait(false);
            return server ?? SwatServer.FromIP(ip, queryport, name);
        }

        public async Task<SwatServer> GetSwatServerFromDatabaseAsync(string name)
        {
            if (name == null)
                return null;

            SwatServer server = null;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.swat_servers WHERE name = @name LIMIT 1;";
                    cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        if (await reader.ReadAsync().ConfigureAwait(false))
                            server = new SwatServer((string)reader["name"], (string)reader["ip"], (int)reader["joinport"], (int)reader["queryport"]);
                    }
                }
            } finally {
                _sem.Release();
            }

            return server;
        }

        public async Task RemoveSwatServerAsync(string name)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.swat_servers WHERE name = @name;";
                    cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
