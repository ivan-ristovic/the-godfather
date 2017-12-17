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
        private string ConnectionString { get; }
        private SemaphoreSlim Semaphore { get; }
        private DatabaseConfig Configuration { get; }


        public DatabaseService(DatabaseConfig config)
        {
            Semaphore = new SemaphoreSlim(100, 100);

            if (config == null)
                Configuration = DatabaseConfig.Default;
            else
                Configuration = config;

            var csb = new NpgsqlConnectionStringBuilder() {
                Host = Configuration.Hostname,
                Port = Configuration.Port,
                Database = Configuration.Database,
                Username = Configuration.Username,
                Password = Configuration.Password,
                Pooling = true
                /*
                SslMode = SslMode.Require,
                TrustServerCertificate = true
                */
            };
            ConnectionString = csb.ConnectionString;
        }


        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> ExecuteRawQueryAsync(string query)
        {
            await Semaphore.WaitAsync();
            var dicts = new List<IReadOnlyDictionary<string, string>>();
            
            try {
                using (var con = new NpgsqlConnection(ConnectionString))
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
            } catch {

            }

            Semaphore.Release();
            return new ReadOnlyCollection<IReadOnlyDictionary<string, string>>(dicts);
        }
    }
}
