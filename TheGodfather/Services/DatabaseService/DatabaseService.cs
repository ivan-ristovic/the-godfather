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
    public partial class DatabaseService
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
            } finally {
                _sem.Release();
            }
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> ExecuteRawQueryAsync(string query)
        {
            var dicts = new List<IReadOnlyDictionary<string, string>>();

            await _sem.WaitAsync();
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
            } finally {
                _sem.Release();
            }

            return new ReadOnlyCollection<IReadOnlyDictionary<string, string>>(dicts);
        }

        public async Task<bool> AddGuildIfNotExistsAsync(ulong gid)
        {
            if (await GuildConfigExistsAsync(gid).ConfigureAwait(false))
                return false;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.guild_cfg VALUES (@gid, NULL, NULL);";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }

            return true;
        }

        public async Task<bool> GuildConfigExistsAsync(ulong gid)
        {
            bool exists = false;

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.guild_cfg WHERE gid = @gid LIMIT 1;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (res != null && !(res is DBNull))
                        exists = true;
                }
            } finally {
                _sem.Release();
            }

            return exists;
        }
    }
}
