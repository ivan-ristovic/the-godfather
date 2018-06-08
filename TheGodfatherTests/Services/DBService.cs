#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TheGodfather.Common;

using Npgsql;
using System.IO;
using Newtonsoft.Json;
#endregion

namespace TheGodfather.Services.Tests
{
    [TestClass()]
    public class DBServiceTests
    {
        private static string _connectionString;
        private static SemaphoreSlim _sem;
        private static SemaphoreSlim _tsem;
        private static DatabaseConfig _cfg;

        private static NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);

        private static async Task<NpgsqlCommand> CreateCommandAsync()
        {
            var conn = GetConnection();
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            return cmd;
        }


        [ClassInitialize]
        public static async Task Init(TestContext ctx)
        {
            string json;
            try {
                using (var sr = new StreamReader("Resources/config.json"))
                    json = await sr.ReadToEndAsync();
                var cfg = JsonConvert.DeserializeObject<BotConfig>(json);

                _sem = new SemaphoreSlim(100, 100);
                _tsem = new SemaphoreSlim(1, 1);

                if (cfg == null)
                    _cfg = DatabaseConfig.Default;
                else
                    _cfg = cfg.DatabaseConfig;

                var csb = new NpgsqlConnectionStringBuilder() {
                    Host = _cfg.Hostname,
                    Port = _cfg.Port,
                    Database = _cfg.Database,
                    Username = _cfg.Username,
                    Password = _cfg.Password,
                    Pooling = true
                    //SslMode = SslMode.Require,
                    //TrustServerCertificate = true
                };
                _connectionString = csb.ConnectionString;

            } catch {
                Assert.Fail("Config file not found or GIPHY key isn't valid.");
            }
        }


        [TestMethod()]
        public async Task GetBalanceAsync()
        {
            await _sem.WaitAsync();
            try {
                using (var cmd = await CreateCommandAsync()) {
                    cmd.CommandText = @"
                        SELECT balance 
                        FROM gf.accounts 
                        WHERE uid = :uid AND gid = :gid 
                        LIMIT 1;";

                    NpgsqlCommandBuilder.DeriveParameters(cmd);
                    cmd.Parameters[0].Value = 201309107267960832;
                    cmd.Parameters[1].Value = 337570344149975050;

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    Assert.IsTrue(res != null && !(res is DBNull));
                }
            } finally {
                _sem.Release();
            }
        }
    }
}
