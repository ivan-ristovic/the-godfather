#region USING_DIRECTIVES
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Services.Database.Bank;
#endregion

namespace TheGodfather.Services.Database
{
    // TODO : Remove partial once all the parts are refactored to extensions
    public partial class DBService
    {
        private readonly string connectionString;
        private readonly SemaphoreSlim accessSemaphore;
        private readonly SemaphoreSlim transactionSemaphore;
        private readonly DatabaseConfig cfg;


        public DBService(DatabaseConfig config)
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
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand())
                    await action(cmd).ConfigureAwait(false);
            } catch (NpgsqlException e) {
                throw new DatabaseOperationException("Database operation failed!", e);
            } finally {
                this.accessSemaphore.Release();
            }
        }

        public async Task ExecuteTransactionAsync(Func<NpgsqlConnection, SemaphoreSlim, Task> action)
        {
            await this.accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                    await action(con, this.transactionSemaphore);
            } catch (NpgsqlException e) {
                throw new DatabaseOperationException("Database operation failed!", e);
            } finally {
                this.accessSemaphore.Release();
            }
        }

        public Task InitializeAsync()
            => ExecuteCommandAsync(cmd => Task.CompletedTask );

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> ExecuteRawQueryAsync(string query)
        {
            var dicts = new List<IReadOnlyDictionary<string, string>>();

            await ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = query;
                using (var reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        var dict = new Dictionary<string, string>();

                        for (int i = 0; i < reader.FieldCount; i++)
                            dict[reader.GetName(i)] = reader[i] is DBNull ? "<null>" : reader[i].ToString();

                        dicts.Add(new ReadOnlyDictionary<string, string>(dict));
                    }
                }
            });

            return dicts.AsReadOnly();
        }


        internal async Task CheckIntegrityAsync()
        {
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid, gid, balance FROM gf.accounts LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid, cid, bday, last_updated FROM gf.birthdays LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT cid, reason FROM gf.blocked_channels LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid, reason FROM gf.blocked_users LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid, gid, name, strength, vitality, max_vitality FROM gf.chickens LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT wid, name, price, upgrades_stat, modifier FROM gf.chicken_upgrades LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid, gid, wid FROM gf.chicken_active_upgrades LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT id, url, savedurl FROM gf.feeds LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT id, cid, qname FROM gf.subscriptions LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT id, gid, filter FROM gf.filters LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = @"SELECT gid, welcome_cid, leave_cid, welcome_msg, leave_msg, prefix, 
                   suggestions_enabled, log_cid, linkfilter_enabled, linkfilter_invites, 
                   linkfilter_booters, linkfilter_disturbing, linkfilter_iploggers, 
                   linkfilter_shorteners FROM gf.guild_cfg LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT id, insult FROM gf.insults LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT gid, name, url FROM gf.memes LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
        }


        private async Task<NpgsqlConnection> OpenConnectionAsync()
        {
            var con = new NpgsqlConnection(this.connectionString);
            await con.OpenAsync().ConfigureAwait(false);
            return con;
        }
    }
}
