using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using static TheGodfather.Database.DatabaseContextBuilder;

namespace TheGodfatherTests
{
    public static class TestDatabaseProvider
    {
        public static DatabaseContextBuilder Database { get; private set; }
        public static string ConnectionString { get; private set; }
        public static SqliteConnection DatabaseConnection { get; private set; }


        static TestDatabaseProvider()
        {
            ConnectionString = "DataSource=:memory:;foreign keys=true;";
            DatabaseConnection = new SqliteConnection(ConnectionString);

            var cfg = new DatabaseConfig() {
                Provider = DatabaseProvider.SqliteInMemory
            };
            DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(DatabaseConnection)
                .Options;

            Database = new DatabaseContextBuilder(cfg, options);
        }


        public static void AlterAndVerify(Action<DatabaseContext> alter, Action<DatabaseContext> verify, bool ensureSave = false)
        {
            DatabaseConnection.Open();

            try {
                CreateDatabase();
                SeedGuildData();

                using (DatabaseContext context = Database.CreateContext()) {
                    alter(context);
                    if (ensureSave)
                        context.SaveChanges();
                }

                using (DatabaseContext context = Database.CreateContext())
                    verify(context);
            } finally {
                DatabaseConnection.Close();
            }
        }

        public static void SetupAlterAndVerify(Action<DatabaseContext> setup,
                                               Action<DatabaseContext> alter,
                                               Action<DatabaseContext> verify,
                                               bool ensureSave = false)
        {
            DatabaseConnection.Open();

            try {
                CreateDatabase();
                SeedGuildData();

                using (DatabaseContext context = Database.CreateContext()) {
                    setup(context);
                    context.SaveChanges();
                }

                using (DatabaseContext context = Database.CreateContext()) {
                    alter(context);
                    if (ensureSave)
                        context.SaveChanges();
                }

                using (DatabaseContext context = Database.CreateContext())
                    verify(context);
            } finally {
                DatabaseConnection.Close();
            }
        }


        private static void CreateDatabase()
        {
            using (DatabaseContext context = Database.CreateContext()) {
                context.Database.EnsureCreated();
            }
        }

        private static void SeedGuildData()
        {
            using (DatabaseContext context = Database.CreateContext()) {
                foreach (ulong id in MockData.Ids)
                    context.GuildConfig.Add(new DatabaseGuildConfig() { GuildId = id });
                context.SaveChanges();
            }
        }
    }
}
