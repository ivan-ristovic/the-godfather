#region USING_DIRECTIVES
using Npgsql;
using System;
#endregion

namespace TheGodfather.Database
{
    public class DatabaseContextBuilder
    {
        public enum DatabaseProvider
        {
            SQLite = 0,
            PostgreSQL = 1,
            SQLServer = 2
        }


        private string ConnectionString { get; }
        private DatabaseProvider Provider { get; }


        public DatabaseContextBuilder(DatabaseProvider provider, string connectionString)
        {
            this.Provider = provider;
            this.ConnectionString = connectionString;
        }

        public DatabaseContextBuilder(DatabaseConfig cfg)
        {
            cfg = cfg ?? DatabaseConfig.Default;
            this.Provider = cfg.Provider;

            switch (this.Provider) {
                case DatabaseProvider.PostgreSQL:
                    this.ConnectionString = new NpgsqlConnectionStringBuilder() {
                        Host = cfg.Hostname,
                        Port = cfg.Port,
                        Database = cfg.DatabaseName,
                        Username = cfg.Username,
                        Password = cfg.Password,
                        Pooling = true,
                        MaxAutoPrepare = 50,
                        AutoPrepareMinUsages = 3,
                        SslMode = SslMode.Prefer,
                        TrustServerCertificate = true
                    }.ConnectionString;
                    break;
                case DatabaseProvider.SQLite:
                    this.ConnectionString = $"Data Source={cfg.DatabaseName}";
                    break;
                case DatabaseProvider.SQLServer:
                    this.ConnectionString = $@"Data Source=(localdb)\ProjectsV13;Initial Catalog={cfg.DatabaseName};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                    break;
                default:
                    throw new NotSupportedException("Unsupported database provider!");
            }
        }


        public DatabaseContext CreateContext()
        {
            try {
                return new DatabaseContext(this.Provider, this.ConnectionString);
            } catch (Exception e) {
                Console.WriteLine("Error during database initialization:");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}