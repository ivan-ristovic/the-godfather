using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using static TheGodfather.Database.DatabaseContextBuilder;

namespace TheGodfatherTests.Database
{
    public abstract class DatabaseTestsBase
    {
        private static readonly DatabaseContextBuilder _dbb;

        static DatabaseTestsBase()
        {
            _dbb = new DatabaseContextBuilder(new DatabaseConfig() {
                Provider = DatabaseProvider.InMemoryTestingDatabase
            });
        }


        protected void AlterAndVerify(Action<DatabaseContext> alter, Action<DatabaseContext> verify, bool ensureSave = false)
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
    }
}
