using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using TheGodfather.Common;

namespace TheGodfather.Database
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            Console.WriteLine("Enter db name:");
            string db = Console.ReadLine();
            Console.WriteLine("Enter username:");
            string user = Console.ReadLine();
            Console.WriteLine("Enter password:");
            string pass = Console.ReadLine();

            var dbb = new DatabaseContextBuilder(new DatabaseConfig() {
                DatabaseName = db,
                Username = user,
                Password = pass,
                Hostname = "localhost",
                Port = 5432
            });
            return dbb.CreateContext();
        }
    }
}
