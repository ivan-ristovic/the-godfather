using System;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;

namespace TheGodfather.Database
{
    public class DatabaseContextBuilder
    {
        private string ConnectionString { get; }
        private DbProvider Provider { get; }
        private DbContextOptions<DatabaseContext>? Options { get; }


        public DatabaseContextBuilder(DbProvider provider, string connectionString, DbContextOptions<DatabaseContext>? options = null)
        {
            this.Provider = provider;
            this.ConnectionString = connectionString;
            this.Options = options;
        }

        public DatabaseContextBuilder(DbConfig cfg, DbContextOptions<DatabaseContext>? options = null)
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


        // TODO remove
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
        // END remove



        public TheGodfatherDbContext CreateDbContext()
        {
            try {
                return new TheGodfatherDbContext(this.Provider, this.ConnectionString);
                //return this.Options is null
                //    ? new TheGodfatherDbContext(this.Provider, this.ConnectionString)
                //    : new TheGodfatherDbContext(this.Provider, this.ConnectionString, this.Options);
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