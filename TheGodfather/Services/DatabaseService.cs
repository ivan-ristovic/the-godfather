#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Helpers;
using TheGodfather.Exceptions;
using TheGodfather.Helpers.Swat;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public class DatabaseService
    {
        private string _connectionString { get; }
        private SemaphoreSlim _sem { get; }
        private SemaphoreSlim _tsem { get; }
        private DatabaseConfig _cfg { get; }


        public DatabaseService(DatabaseConfig config)
        {
            _sem = new SemaphoreSlim(100, 100);
            _tsem = new SemaphoreSlim(1, 1);

            if (config == null)
                _cfg = DatabaseConfig.Default;
            else
                _cfg = config;

            var csb = new NpgsqlConnectionStringBuilder() {
                Host = _cfg.Hostname,
                Port = _cfg.Port,
                Database = _cfg.Database,
                Username = _cfg.Username,
                Password = _cfg.Password,
                Pooling = true
                /*
                SslMode = SslMode.Require,
                TrustServerCertificate = true
                */
            };
            _connectionString = csb.ConnectionString;
        }


        public async Task InitializeAsync()
        {
            await _sem.WaitAsync();

            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);
                }
            } catch (NpgsqlException e) {
                throw new DatabaseServiceException("Database connection failed. Check your login details in the config.json file.", e);
            }

            _sem.Release();
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> ExecuteRawQueryAsync(string query)
        {
            await _sem.WaitAsync();
            var dicts = new List<IReadOnlyDictionary<string, string>>();

            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = query;

                    using (var rdr = await cmd.ExecuteReaderAsync()) {
                        while (await rdr.ReadAsync()) {
                            var dict = new Dictionary<string, string>();

                            for (var i = 0; i < rdr.FieldCount; i++)
                                dict[rdr.GetName(i)] = rdr[i] is DBNull ? "<null>" : rdr[i].ToString();

                            dicts.Add(new ReadOnlyDictionary<string, string>(dict));
                        }
                    }
                }
            } catch (NpgsqlException e) {
                throw new DatabaseServiceException(e);
            }

            _sem.Release();
            return new ReadOnlyCollection<IReadOnlyDictionary<string, string>>(dicts);
        }


        #region BANK_SERVICES
        public async Task<bool> HasBankAccountAsync(ulong uid)
        {
            int? balance = await GetBalanceForUserAsync(uid)
                .ConfigureAwait(false);
            return balance.HasValue;
        }

        public async Task<bool> RetrieveCreditsAsync(ulong uid, int amount)
        {
            int? balance = await GetBalanceForUserAsync(uid).ConfigureAwait(false);
            if (!balance.HasValue || balance.Value < amount)
                return false;

            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "UPDATE gf.accounts SET balance = balance - @amount WHERE uid = @uid;";
                cmd.Parameters.AddWithValue("amount", NpgsqlDbType.Integer, amount);
                cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
            return true;
        }

        public async Task<int?> GetBalanceForUserAsync(ulong uid)
        {
            await _sem.WaitAsync();

            int? balance = null;

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @uid LIMIT 1;";
                cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);

                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    balance = (int)res;
            }

            _sem.Release();
            return balance;
        }

        public async Task OpenAccountForUserAsync(ulong uid)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "INSERT INTO gf.accounts VALUES(@uid, 25);";
                cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task IncreaseBalanceForUserAsync(ulong uid, int amount)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "UPDATE gf.accounts SET balance = balance + @amount WHERE uid = @uid;";
                cmd.Parameters.AddWithValue("amount", NpgsqlDbType.Integer, amount);
                cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, (long)uid);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task TransferCurrencyAsync(ulong source, ulong target, long amount)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString)) {
                await con.OpenAsync().ConfigureAwait(false);

                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @target;";
                    cmd.Parameters.AddWithValue("target", NpgsqlDbType.Bigint, (long)target);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    
                    if (res == null || res is DBNull)
                        await OpenAccountForUserAsync(target);
                }

                await _tsem.WaitAsync().ConfigureAwait(false);
                try {
                    using (var transaction = con.BeginTransaction()) {
                        var cmd1 = con.CreateCommand();
                        cmd1.Transaction = transaction;
                        cmd1.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @source OR uid = @target FOR UPDATE;";
                        cmd1.Parameters.AddWithValue("source", NpgsqlDbType.Bigint, (long)source);
                        cmd1.Parameters.AddWithValue("target", NpgsqlDbType.Bigint, (long)target);

                        await cmd1.ExecuteNonQueryAsync().ConfigureAwait(false);

                        var cmd2 = con.CreateCommand();
                        cmd2.Transaction = transaction;
                        cmd2.CommandText = "SELECT balance FROM gf.accounts WHERE uid = @source;";
                        cmd2.Parameters.AddWithValue("source", NpgsqlDbType.Bigint, (long)source);

                        var res = await cmd2.ExecuteScalarAsync().ConfigureAwait(false);
                        if (res == null || res is DBNull || (int)res < amount) {
                            await transaction.RollbackAsync().ConfigureAwait(false);
                            throw new CommandFailedException("Source user's currency amount is insufficient for the transfer.");
                        }

                        var cmd3 = con.CreateCommand();
                        cmd3.Transaction = transaction;
                        cmd3.CommandText = "UPDATE gf.accounts SET balance = balance - @amount WHERE uid = @source;";
                        cmd3.Parameters.AddWithValue("amount", NpgsqlDbType.Integer, amount);
                        cmd3.Parameters.AddWithValue("source", NpgsqlDbType.Bigint, (long)source);

                        await cmd3.ExecuteNonQueryAsync().ConfigureAwait(false);

                        var cmd4 = con.CreateCommand();
                        cmd4.Transaction = transaction;
                        cmd4.CommandText = "UPDATE gf.accounts SET balance = balance + @amount WHERE uid = @target;";
                        cmd4.Parameters.AddWithValue("amount", NpgsqlDbType.Integer, amount);
                        cmd4.Parameters.AddWithValue("target", NpgsqlDbType.Bigint, (long)target);

                        await cmd4.ExecuteNonQueryAsync().ConfigureAwait(false);

                        await transaction.CommitAsync().ConfigureAwait(false);

                        cmd1.Dispose();
                        cmd2.Dispose();
                        cmd3.Dispose();
                        cmd4.Dispose();
                    }
                } finally {
                    _tsem.Release();
                    _sem.Release();
                }
            }
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetTopAccountsAsync()
        {
            var res = await ExecuteRawQueryAsync("SELECT * FROM gf.accounts ORDER BY balance DESC LIMIT 10")
                .ConfigureAwait(false);
            return res;
        }
        #endregion

        #region FILTER_SERVICES
        public async Task<IReadOnlyList<Tuple<ulong, string>>> GetAllGuildFiltersAsync()
        {
            await _sem.WaitAsync();
            var filters = new List<Tuple<ulong, string>>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.filters;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        filters.Add(new Tuple<ulong, string>((ulong)(long)reader["gid"], (string)reader["filter"]));
                }
            }

            _sem.Release();
            return filters.AsReadOnly();
        }

        public async Task<IReadOnlyList<string>> GetFiltersForGuildAsync(ulong gid)
        {
            await _sem.WaitAsync();
            var filters = new List<string>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.filters WHERE gid = @gid;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        filters.Add((string)reader["filter"]);
                }
            }

            _sem.Release();
            return filters.AsReadOnly();
        }

        public async Task AddFilterAsync(ulong gid, string filter)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "INSERT INTO gf.filters(gid, filter) VALUES (@gid, @filter);";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                cmd.Parameters.AddWithValue("filter", NpgsqlDbType.Varchar, filter);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task DeleteFilterAsync(ulong gid, string filter)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "DELETE FROM gf.filters WHERE gid = @gid AND filter = @filter;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                cmd.Parameters.AddWithValue("filter", NpgsqlDbType.Varchar, filter);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task ClearGuildFiltersAsync(ulong gid)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "DELETE FROM gf.filters WHERE gid = @gid;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }
        #endregion

        #region MEME_SERVICES
        public async Task<IReadOnlyDictionary<string, string>> GetGuildMemesAsync(ulong gid)
        {
            await _sem.WaitAsync();
            var dict = new Dictionary<string, string>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT name, url FROM gf.memes WHERE gid = @gid;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        dict[(string)reader["name"]] = (string)reader["url"];
                }
            }

            _sem.Release();
            return new ReadOnlyDictionary<string, string>(dict);
        }

        public async Task<string> GetRandomMemeAsync(ulong gid)
        {
            await _sem.WaitAsync();

            string url = null;

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT url FROM gf.memes TABLESAMPLE SYSTEM_ROWS(1) WHERE gid = @gid;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    url = (string)res;
            }

            _sem.Release();
            return url;
        }

        public async Task<string> GetMemeUrlAsync(ulong gid, string name)
        {
            await _sem.WaitAsync();

            string url = null;

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT url FROM gf.memes WHERE gid = @gid AND name = @name LIMIT 1;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);

                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    url = (string)res;
            }

            _sem.Release();
            return url;
        }

        public async Task AddMemeAsync(ulong gid, string name, string url)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "INSERT INTO gf.memes VALUES (@gid, @name, @url);";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);
                cmd.Parameters.AddWithValue("url", NpgsqlDbType.Varchar, url);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task RemoveMemeAsync(ulong gid, string name)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "DELETE FROM gf.memes WHERE gid = @gid AND name = @name;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task DeleteAllGuildMemesAsync(ulong gid)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "DELETE FROM gf.memes WHERE gid = @gid;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }
        #endregion

        #region PREFIX_SERVICES
        public async Task<IReadOnlyDictionary<ulong, string>> GetGuildPrefixesAsync()
        {
            await _sem.WaitAsync();
            var dict = new Dictionary<ulong, string>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.prefixes;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        dict[(ulong)(long)reader["gid"]] = (string)reader["prefix"];
                }
            }

            _sem.Release();
            return new ReadOnlyDictionary<ulong, string>(dict);
        }

        public async Task SetGuildPrefixAsync(ulong gid, string prefix)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.prefixes WHERE gid = @gid LIMIT 1;";
                cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                using (var cmd1 = con.CreateCommand()) {
                    if (res == null || res is DBNull) {
                        cmd.CommandText = "INSERT INTO gf.prefixes VALUES(@gid, @prefix);";
                        cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                        cmd.Parameters.AddWithValue("prefix", NpgsqlDbType.Varchar, prefix);
                    } else {
                        cmd.CommandText = "UPDATE gf.prefixes SET prefix = @prefix WHERE gid = @gid;";
                        cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, (long)gid);
                        cmd.Parameters.AddWithValue("prefix", NpgsqlDbType.Varchar, prefix);
                    }
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

            _sem.Release();
        }
        #endregion

        #region STATS_SERVICES
        public async Task<IReadOnlyDictionary<string, string>> GetStatsForUserAsync(ulong uid)
        {
            var res = await ExecuteRawQueryAsync($"SELECT * FROM gf.stats WHERE uid = {uid};")
                .ConfigureAwait(false);

            if (res != null && res.Any())
                return res.First();
            else
                return null;
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetOrderedStatsAsync(string orderstr, params string[] selectors)
        {
            var res = await ExecuteRawQueryAsync($@"
                SELECT uid, {string.Join(", ", selectors)} 
                FROM gf.stats
                ORDER BY {orderstr} DESC
                LIMIT 5
            ").ConfigureAwait(false);

            return res;
        }

        public async Task UpdateStatAsync(ulong uid, string col, int add)
        {
            var stats = await GetStatsForUserAsync(uid).ConfigureAwait(false);

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                if (stats != null && stats.Any())
                    cmd.CommandText = $"UPDATE gf.stats SET {col} = {col} + {add} WHERE uid = {uid};";
                else
                    cmd.CommandText = $"INSERT INTO gf.stats (uid, {col}) VALUES ({uid}, {add});";

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
        #endregion

        #region STATUS_SERVICES
        public async Task<IReadOnlyDictionary<int, string>> GetStatusesAsync()
        {
            await _sem.WaitAsync();

            var dict = new Dictionary<int, string>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.statuses;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        dict[(int)reader["id"]] = (string)reader["status"];
                }
            }

            _sem.Release();
            return new ReadOnlyDictionary<int, string>(dict);
        }

        public async Task<string> GetRandomStatusAsync()
        {
            await _sem.WaitAsync();

            string status = null;

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT status FROM gf.statuses TABLESAMPLE SYSTEM_ROWS(1);";

                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    status = (string)res;
            }

            _sem.Release();
            return status ?? "@TheGodfather help";
        }

        public async Task AddStatusAsync(string status)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "INSERT INTO gf.statuses(status) VALUES (@status);";
                cmd.Parameters.AddWithValue("status", NpgsqlDbType.Varchar, status);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task DeleteStatusAsync(int id)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "DELETE FROM gf.statuses WHERE id = @id;";
                cmd.Parameters.AddWithValue("id", NpgsqlDbType.Integer, id);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }
        #endregion

        #region SWAT_SERVICES
        public async Task<IReadOnlyList<SwatServer>> GetAllSwatServersAsync()
        {
            await _sem.WaitAsync();

            var servers = new List<SwatServer>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT name, ip, joinport, queryport FROM gf.swat_servers;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        servers.Add(new SwatServer((string)reader["name"], (string)reader["ip"], (int)reader["joinport"], (int)reader["queryport"]));
                }
            }

            _sem.Release();
            return servers.AsReadOnly();
        }

        public async Task AddSwatServerAsync(string name, SwatServer server)
        {
            await _sem.WaitAsync();

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

            _sem.Release();
        }

        public async Task RemoveSwatServerAsync(string name)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "DELETE FROM gf.swat_servers WHERE name = @name;";
                cmd.Parameters.AddWithValue("name", NpgsqlDbType.Varchar, name);

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task<SwatServer> GetSwatServerAsync(string ip, int queryport, string name = null)
        {
            var server = await GetSwatServerFromDatabaseAsync(name)
                .ConfigureAwait(false);
            return server ?? SwatServer.CreateFromIP(ip, queryport, name);
        }

        private async Task<SwatServer> GetSwatServerFromDatabaseAsync(string name)
        {
            if (name == null)
                return null;

            await _sem.WaitAsync();

            SwatServer server = null;

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

            _sem.Release();
            return server;
        }
        #endregion
    }
}
