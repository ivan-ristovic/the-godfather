using System;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;

namespace TheGodfather.Database
{
    public class DbContextBuilder
    {
        private string ConnectionString { get; }
        private DbProvider Provider { get; }
        private DbContextOptions<TheGodfatherDbContext>? Options { get; }


        public DbContextBuilder(DbProvider provider, string connectionString, DbContextOptions<TheGodfatherDbContext>? options = null)
        {
            this.Provider = provider;
            this.ConnectionString = connectionString;
            this.Options = options;
        }

        public DbContextBuilder(DbConfig cfg, DbContextOptions<TheGodfatherDbContext>? options = null)
        {
            cfg ??= new DbConfig();
            this.Provider = cfg.Provider;
            this.Options = options;
            this.ConnectionString = this.Provider switch
            {
                DbProvider.PostgreSql => new NpgsqlConnectionStringBuilder {
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
                }.ConnectionString,
                DbProvider.Sqlite => $"Data Source={cfg.DatabaseName}.db;",
                DbProvider.SqlServer => $@"Data Source=(localdb)\ProjectsV13;Initial Catalog={cfg.DatabaseName};" +
                                          "Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;" +
                                          "ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
                DbProvider.SqliteInMemory => @"DataSource=:memory:;foreign keys=true;",
                _ => throw new NotSupportedException("Unsupported database provider!"),
            };
        }


        public TheGodfatherDbContext CreateContext()
        {
            try {
                return this.Options is null
                    ? new TheGodfatherDbContext(this.Provider, this.ConnectionString)
                    : new TheGodfatherDbContext(this.Provider, this.ConnectionString, this.Options);
            } catch (Exception e) {
                Log.Fatal(e, "An exception occured during database initialization:");
                throw;
            }
        }

        public TheGodfatherDbContext CreateContext(DbContextOptions<TheGodfatherDbContext> options)
        {
            try {
                return new TheGodfatherDbContext(this.Provider, this.ConnectionString, options);
            } catch (Exception e) {
                Log.Fatal(e, "An exception occured during database initialization:");
                throw;
            }
        }
    }
}