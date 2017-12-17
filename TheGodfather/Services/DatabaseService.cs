#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Helpers;
using TheGodfather.Exceptions;

using Npgsql;
using NpgsqlTypes;
using System.Collections.ObjectModel;
#endregion

namespace TheGodfather.Services
{
    public class DatabaseService
    {
        private string _connectionString { get; }
        private SemaphoreSlim _sem { get; }
        private DatabaseConfig _cfg { get; }


        public DatabaseService(DatabaseConfig config)
        {
            _sem = new SemaphoreSlim(100, 100);

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
                throw new DatabaseServiceException("", e);
            }

            _sem.Release();
            return new ReadOnlyCollection<IReadOnlyDictionary<string, string>>(dicts);
        }

        public async Task<IReadOnlyDictionary<string, string>> GetStatsForUserAsync(ulong uid)
        {
            var res = await ExecuteRawQueryAsync($"SELECT * FROM gf.stats WHERE uid = {uid};")
                .ConfigureAwait(false);
            
            if (res != null && res.Any())
                return res.First();
            else
                return null;
        }

        public async Task UpdateStat(ulong uid, string col, int add)
        {
            var stats = await GetStatsForUserAsync(uid);
            if (stats != null && stats.Any())
                await ExecuteRawQueryAsync($"UPDATE gf.stats SET {col} = {col} + {add} WHERE uid = {uid};");
            else
                await ExecuteRawQueryAsync($"INSERT INTO gf.stats (uid, {col}) VALUES ({uid}, {add});");
        }
    }
}
