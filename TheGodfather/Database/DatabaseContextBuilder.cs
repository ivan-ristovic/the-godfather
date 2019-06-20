#region USING_DIRECTIVES
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
#endregion

namespace TheGodfather.Database
{
    public class DatabaseContextBuilder
    {
        public enum DatabaseProvider
        {
            Sqlite = 0,
            PostgreSql = 1,
            SqlServer = 2,
            SqliteInMemory = 3
        }


        private string ConnectionString { get; }
        private DatabaseProvider Provider { get; }
        private DbContextOptions<DatabaseContext> Options { get; }


        public DatabaseContextBuilder(DatabaseProvider provider, string connectionString, DbContextOptions<DatabaseContext> options = null)
        {
            this.Provider = provider;
            this.ConnectionString = connectionString;
            this.Options = options;
        }

        public DatabaseContextBuilder(DatabaseConfig cfg, DbContextOptions<DatabaseContext> options = null)
        {
            cfg = cfg ?? DatabaseConfig.Default;
            this.Provider = cfg.Provider;
            this.Options = options;

            switch (this.Provider) {
                case DatabaseProvider.PostgreSql:
                    this.ConnectionString = new NpgsqlConnectionStringBuilder {
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
                case DatabaseProvider.Sqlite:
                    this.ConnectionString = $"Data Source={cfg.DatabaseName}.db;";
                    break;
                case DatabaseProvider.SqlServer:
                    this.ConnectionString = $@"Data Source=(localdb)\ProjectsV13;Initial Catalog={cfg.DatabaseName};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                    break;
                case DatabaseProvider.SqliteInMemory:
                    this.ConnectionString = @"DataSource=:memory:;foreign keys=true;";
                    break;
                default:
                    throw new NotSupportedException("Unsupported database provider!");
            }
        }


        public DatabaseContext CreateContext()
        {
            try {
                return this.Options is null
                    ? new DatabaseContext(this.Provider, this.ConnectionString)
                    : new DatabaseContext(this.Provider, this.ConnectionString, this.Options);
            } catch (Exception e) {
                Console.WriteLine("Error during database initialization:");
                Console.WriteLine(e);
                throw;
            }
        }

        public DatabaseContext CreateContext(DbContextOptions<DatabaseContext> options)
        {
            try {
                return new DatabaseContext(this.Provider, this.ConnectionString, options);
            } catch (Exception e) {
                Console.WriteLine("Error during database initialization:");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}