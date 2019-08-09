using System;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace TheGodfather.Database
{
    public class DatabaseContextBuilder
    {
        private string ConnectionString { get; }
        private DatabaseManagementSystem Provider { get; }
        private DbContextOptions<DatabaseContext> Options { get; }


        public DatabaseContextBuilder(DatabaseManagementSystem provider, string connectionString, DbContextOptions<DatabaseContext> options = null)
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
                case DatabaseManagementSystem.PostgreSql:
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
                case DatabaseManagementSystem.Sqlite:
                    this.ConnectionString = $"Data Source={cfg.DatabaseName}.db;";
                    break;
                case DatabaseManagementSystem.SqlServer:
                    this.ConnectionString = $@"Data Source=(localdb)\ProjectsV13;Initial Catalog={cfg.DatabaseName};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                    break;
                case DatabaseManagementSystem.SqliteInMemory:
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