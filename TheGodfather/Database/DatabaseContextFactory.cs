using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Linq;

namespace TheGodfather.Database
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(params string[] args)
        {
            string db;
            if (string.IsNullOrWhiteSpace(args?.First())) {
                Console.WriteLine("Enter db name:");
                db = Console.ReadLine();
            } else {
                db = args.First();
            }

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
