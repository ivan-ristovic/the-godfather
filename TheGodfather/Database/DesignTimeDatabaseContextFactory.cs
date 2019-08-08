using Microsoft.EntityFrameworkCore.Design;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Database
{
    public class DesignTimeDatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        private readonly BotConfigService cfg;
        private readonly AsyncExecutionService async;


        public DesignTimeDatabaseContextFactory()
        {
            this.cfg = new BotConfigService();
            this.async = new AsyncExecutionService();
        }


        public DatabaseContext CreateDbContext(params string[] _)
        {
            BotConfig cfg = this.async.Execute(this.cfg.LoadConfigAsync("Resources/config.json"));
            return new DatabaseContextBuilder(cfg.DatabaseConfig).CreateContext();
        }
    }
}
