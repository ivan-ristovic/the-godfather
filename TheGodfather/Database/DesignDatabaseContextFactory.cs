using Microsoft.EntityFrameworkCore.Design;

namespace TheGodfather.Database
{
    public class DesignDatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(params string[] args)
        {
            var dbb = new DatabaseContextBuilder(new DatabaseConfig() {
                DatabaseName = "gfdb_beta",
                Provider = DatabaseContextBuilder.DatabaseProvider.PostgreSQL,
                Username = "postgres",
                Password = "",
                Hostname = "localhost",
                Port = 5432
            });

            return dbb.CreateContext();
        }
    }
}
