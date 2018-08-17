#region USING_DIRECTIVES
using Npgsql;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Exceptions;
using TheGodfather.Services.Common;
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

        public Task InitializeAsync()
            => this.ExecuteCommandAsync(cmd => Task.CompletedTask);


        internal async Task CheckIntegrityAsync()
        {
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid, gid, balance FROM gf.accounts LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid, cid, bday, last_updated FROM gf.birthdays LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT cid, reason FROM gf.blocked_channels LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid, reason FROM gf.blocked_users LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid, gid, name, strength, vitality, max_vitality FROM gf.chickens LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT wid, name, price, upgrades_stat, modifier FROM gf.chicken_upgrades LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid, gid, wid FROM gf.chicken_active_upgrades LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT id, url, savedurl FROM gf.feeds LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT id, cid, qname FROM gf.subscriptions LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT id, gid, filter FROM gf.filters LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = @"SELECT gid, welcome_cid, leave_cid, welcome_msg, leave_msg, prefix, 
                    suggestions_enabled, log_cid, linkfilter_enabled, linkfilter_invites, 
                    linkfilter_booters, linkfilter_disturbing, linkfilter_iploggers, 
                    linkfilter_shorteners, silent_respond, currency, ratelimit_enabled, ratelimit_action, 
                    ratelimit_sens, antiflood_enabled, antiflood_sens, antiflood_action, antiflood_cooldown,
                    mute_rid 
                    FROM gf.guild_cfg LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT id, insult FROM gf.insults LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT gid, name, url FROM gf.memes LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid FROM gf.privileged LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid, count FROM gf.msgcount LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT uid, count FROM gf.msgcount LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT gid, rank, name FROM gf.ranks LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT gid, trigger, response, id FROM gf.text_reactions LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT gid, trigger, reaction, id FROM gf.emoji_reactions LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT id, type, uid, cid, gid, comment, execution_time FROM gf.saved_tasks LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT id, gid, name, price FROM gf.items LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT id, uid FROM gf.purchases LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT gid, rid FROM gf.assignable_roles LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT gid, rid FROM gf.automatic_roles LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT status, type, id FROM gf.statuses LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT ip, joinport, queryport, name FROM gf.swat_servers LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = @"SELECT uid, duels_won, duels_lost, hangman_won, numraces_won, 
                   quizes_won, races_won, ttt_won, ttt_lost, chain4_won, chain4_lost, caro_won, 
                   caro_lost, othello_won, othello_lost FROM gf.stats LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT name, ip, additional_info FROM gf.swat_ips LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT name, ip, reason FROM gf.swat_banlist LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
            await this.ExecuteCommandAsync(cmd => {
                cmd.CommandText = "SELECT gid, id, type FROM gf.log_exempt LIMIT 1;";
                return cmd.ExecuteScalarAsync();
            });
        }


        private async Task<NpgsqlConnection> OpenConnectionAsync()
        {
            var connection = new NpgsqlConnection(this.connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }
    }
}
