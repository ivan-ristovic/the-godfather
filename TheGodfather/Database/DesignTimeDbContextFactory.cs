using Microsoft.EntityFrameworkCore.Design;

namespace TheGodfather.Database;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TheGodfatherDbContext>
{
    public TheGodfatherDbContext CreateDbContext(params string[] _)
    {
        var cfg = new DbConfig {
            Provider = DbProvider.PostgreSql
        };
        return new DbContextBuilder(cfg).CreateContext();
    }
}
