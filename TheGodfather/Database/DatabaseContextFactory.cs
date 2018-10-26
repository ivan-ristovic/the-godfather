using Microsoft.EntityFrameworkCore.Design;
using System;

namespace TheGodfather.Database
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            Console.WriteLine("Enter db name:");
            string db = Console.ReadLine();

            var dbb = new DatabaseContextBuilder(new DatabaseConfig() {
                DatabaseName = db,
                Username = "postgres",
                Password = "",
                Hostname = "localhost",
                Port = 5432
            });
            return dbb.CreateContext();
        }
    }
}
