using Microsoft.EntityFrameworkCore.Design;
using System;
using static TheGodfather.Database.DatabaseContextBuilder;

namespace TheGodfather.Database
{
    public class DesignTimeDatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(params string[] args)
        {
            Console.WriteLine("Database provider (0 - SQLite, 1 - PostgreSQL, 2 - SQLServer):");
            Enum.TryParse(Console.ReadLine(), out DatabaseProvider provider);
            Console.WriteLine("Database:");
            string db = Console.ReadLine();
            Console.WriteLine("User:");
            string user = Console.ReadLine();
            Console.WriteLine("Password:");
            string password = Console.ReadLine();

            var dbb = new DatabaseContextBuilder(new DatabaseConfig() {
                DatabaseName = db,
                Provider = provider,
                Username = user,
                Password = password,
                Hostname = "localhost",
                Port = 5432
            });

            return dbb.CreateContext();
        }
    }
}
