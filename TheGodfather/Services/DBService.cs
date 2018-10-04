#region USING_DIRECTIVES
using Npgsql;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Database;
using TheGodfather.Exceptions;
#endregion

namespace TheGodfather.Services
{
    public class DBService
    {
        private readonly string connectionString;
        private readonly SemaphoreSlim accessSemaphore;
        private readonly SemaphoreSlim transactionSemaphore;
        private readonly DatabaseConfig cfg;


        public DBService(DatabaseConfig config = null)
        {
            this.cfg = config ?? DatabaseConfig.Default;
            this.accessSemaphore = new SemaphoreSlim(100, 100);
            this.transactionSemaphore = new SemaphoreSlim(1, 1);

            var csb = new NpgsqlConnectionStringBuilder() {
                Host = this.cfg.Hostname,
                Port = this.cfg.Port,
                Database = this.cfg.DatabaseName,
                Username = this.cfg.Username,
                Password = this.cfg.Password,
                Pooling = true,
                MaxAutoPrepare = 50,
                AutoPrepareMinUsages = 3
                //SslMode = SslMode.Require,
                //TrustServerCertificate = true
            };
            this.connectionString = csb.ConnectionString;
        }


        public async Task ExecuteCommandAsync(Func<NpgsqlCommand, Task> action)
        {
            await this.accessSemaphore.WaitAsync().ConfigureAwait(false);
            try {
                using (NpgsqlConnection connection = await this.OpenConnectionAsync().ConfigureAwait(false))
                using (NpgsqlCommand command = connection.CreateCommand())
                    await action(command).ConfigureAwait(false);
            } finally {
                this.accessSemaphore.Release();
            }
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> ExecuteRawQueryAsync(string query)
        {
            var dicts = new List<IReadOnlyDictionary<string, string>>();

            await this.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = query;
                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        var dict = new Dictionary<string, string>();

                        for (int i = 0; i < reader.FieldCount; i++)
                            dict[reader.GetName(i)] = reader[i] is DBNull ? "<null>" : reader[i].ToString();

                        dicts.Add(new ReadOnlyDictionary<string, string>(dict));
                    }
                }
            });

            return dicts.AsReadOnly();
        }

        public async Task ExecuteTransactionAsync(Func<NpgsqlConnection, SemaphoreSlim, Task> action)
        {
            await this.accessSemaphore.WaitAsync().ConfigureAwait(false);
            try {
                using (NpgsqlConnection connection = await this.OpenConnectionAsync().ConfigureAwait(false))
                    await action(connection, this.transactionSemaphore).ConfigureAwait(false);
            } catch (NpgsqlException e) {
                throw new DatabaseOperationException("Database operation failed!", e);
            } finally {
                this.accessSemaphore.Release();
            }
        }


        private async Task<NpgsqlConnection> OpenConnectionAsync()
        {
            var connection = new NpgsqlConnection(this.connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }


        // TEMP
        public DatabaseContextBuilder ContextBuilder { get; internal set; }
    }
}
