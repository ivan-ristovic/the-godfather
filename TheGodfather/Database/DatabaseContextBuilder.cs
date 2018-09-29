#region USING_DIRECTIVES
using Npgsql;
using System;
#endregion

namespace TheGodfather.Database
{
    public class DatabaseContextBuilder
    {
        private string ConnectionString { get; }


        public DatabaseContextBuilder(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public DatabaseContextBuilder(DatabaseConfig cfg)
        {
            cfg = cfg ?? DatabaseConfig.Default;

            var csb = new NpgsqlConnectionStringBuilder() {
                Host = cfg.Hostname,
                Port = cfg.Port,
                Database = cfg.DatabaseName,
                Username = cfg.Username,
                Password = cfg.Password,
                Pooling = true,
                MaxAutoPrepare = 50,
                AutoPrepareMinUsages = 3
                //SslMode = SslMode.Require,
                //TrustServerCertificate = true
            };

            this.ConnectionString = csb.ConnectionString;
        }


        public DatabaseContext CreateContext()
        {
            try {
                return new DatabaseContext(this.ConnectionString);
            } catch (Exception e) {
                Console.WriteLine("Error during database initialization:");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}