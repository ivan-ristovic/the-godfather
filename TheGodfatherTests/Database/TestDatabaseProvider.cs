using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using static TheGodfather.Database.DatabaseContextBuilder;

namespace TheGodfatherTests.Database
{
    public static class TestDatabaseProvider
    {
        private static readonly DatabaseContextBuilder _dbb;

        static TestDatabaseProvider()
        {
            _dbb = new DatabaseContextBuilder(new DatabaseConfig() {
                Provider = DatabaseProvider.InMemoryTestingDatabase
            });
        }


        public static void AlterAndVerify(Action<DatabaseContext> alter, Action<DatabaseContext> verify, bool ensureSave = false)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try {
                DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
                    .UseSqlite(connection)
                    .Options;

                using (DatabaseContext context = _dbb.CreateContext(options))
                    context.Database.EnsureCreated();

                using (DatabaseContext context = _dbb.CreateContext(options)) {
                    alter(context);
                    if (ensureSave)
                        context.SaveChanges();
                }

                using (DatabaseContext context = _dbb.CreateContext(options))
                    verify(context);
            } finally {
                connection.Close();
            }
        }

        public static void SetupAlterAndVerify(Action<DatabaseContext> setup,
                                               Action<DatabaseContext> alter,
                                               Action<DatabaseContext> verify,
                                               bool ensureSave = false)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try {
                DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
                    .UseSqlite(connection)
                    .Options;

                using (DatabaseContext context = _dbb.CreateContext(options))
                    context.Database.EnsureCreated();

                using (DatabaseContext context = _dbb.CreateContext(options)) {
                    setup(context);
                    context.SaveChanges();
                }

                using (DatabaseContext context = _dbb.CreateContext(options)) {
                    alter(context);
                    if (ensureSave)
                        context.SaveChanges();
                }

                using (DatabaseContext context = _dbb.CreateContext(options))
                    verify(context);
            } finally {
                connection.Close();
            }
        }
    }
}
