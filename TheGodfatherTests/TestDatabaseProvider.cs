using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;

namespace TheGodfatherTests
{
    public static class TestDatabaseProvider
    {
        public static DbContextBuilder Database { get; private set; }
        public static string ConnectionString { get; private set; }
        public static SqliteConnection DatabaseConnection { get; private set; }


        static TestDatabaseProvider()
        {
            ConnectionString = "DataSource=:memory:;foreign keys=true;";
            DatabaseConnection = new SqliteConnection(ConnectionString);

            var cfg = new DbConfig() {
                Provider = DbProvider.SqliteInMemory
            };
            DbContextOptions<TheGodfatherDbContext> options = new DbContextOptionsBuilder<TheGodfatherDbContext>()
                .UseSqlite(DatabaseConnection)
                .Options;

            // TODO
            Database = new DbContextBuilder(cfg/*, options*/);
        }


        public static void AlterAndVerify(Action<TheGodfatherDbContext> alter, Action<TheGodfatherDbContext> verify, bool ensureSave = false)
        {
            DatabaseConnection.Open();
            try {
                CreateDatabase();
                SeedGuildData();

                using (TheGodfatherDbContext context = Database.CreateDbContext()) {
                    alter(context);
                    if (ensureSave)
                        context.SaveChanges();
                }

                using (TheGodfatherDbContext context = Database.CreateDbContext())
                    verify(context);
            } finally {
                DatabaseConnection.Close();
            }
        }

        public static async Task AlterAndVerifyAsync(Func<TheGodfatherDbContext, Task> alter, Func<TheGodfatherDbContext, Task> verify, bool ensureSave = false)
        {
            DatabaseConnection.Open();
            try {
                CreateDatabase();
                SeedGuildData();

                using (TheGodfatherDbContext context = Database.CreateDbContext()) {
                    await alter(context);
                    if (ensureSave)
                        await context.SaveChangesAsync();
                }

                using (TheGodfatherDbContext context = Database.CreateDbContext())
                    await verify(context);
            } finally {
                DatabaseConnection.Close();
            }
        }

        public static void SetupAlterAndVerify(Action<TheGodfatherDbContext> setup,
                                               Action<TheGodfatherDbContext> alter,
                                               Action<TheGodfatherDbContext> verify,
                                               bool ensureSave = false)
        {
            DatabaseConnection.Open();
            try {
                CreateDatabase();
                SeedGuildData();

                using (TheGodfatherDbContext context = Database.CreateDbContext()) {
                    setup(context);
                    context.SaveChanges();
                }

                using (TheGodfatherDbContext context = Database.CreateDbContext()) {
                    alter(context);
                    if (ensureSave)
                        context.SaveChanges();
                }

                using (TheGodfatherDbContext context = Database.CreateDbContext())
                    verify(context);
            } finally {
                DatabaseConnection.Close();
            }
        }

        public static async Task SetupAlterAndVerifyAsync(Func<TheGodfatherDbContext, Task> setup,
                                                          Func<TheGodfatherDbContext, Task> alter,
                                                          Func<TheGodfatherDbContext, Task> verify,
                                                          bool ensureSave = false)
        {
            DatabaseConnection.Open();
            try {
                CreateDatabase();
                SeedGuildData();

                using (TheGodfatherDbContext context = Database.CreateDbContext()) {
                    await setup(context);
                    await context.SaveChangesAsync();
                }

                using (TheGodfatherDbContext context = Database.CreateDbContext()) {
                    await alter(context);
                    if (ensureSave)
                        await context.SaveChangesAsync();
                }

                using (TheGodfatherDbContext context = Database.CreateDbContext())
                    await verify(context);
            } finally {
                DatabaseConnection.Close();
            }
        }


        private static void CreateDatabase()
        {
            using (TheGodfatherDbContext context = Database.CreateDbContext())
                context.Database.EnsureCreated();
        }

        private static void SeedGuildData()
        {
            using (TheGodfatherDbContext context = Database.CreateDbContext()) {
                context.GuildConfigs.AddRange(MockData.Ids.Select(id => new GuildConfig() { GuildId = id }));
                context.SaveChanges();
            }
        }
    }
}
