using Microsoft.EntityFrameworkCore.Design;

using Newtonsoft.Json;

using System.IO;
using System.Text;

using TheGodfather.Common;

namespace TheGodfather.Database
{
    public class DesignTimeDatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(params string[] args)
        {
            BotConfig cfg = BotConfig.Default;
            string json = "{}";
            var utf8 = new UTF8Encoding(false);
            var fi = new FileInfo("Resources/config.json");
            if (fi.Exists) {
                try {
                    using (FileStream fs = fi.OpenRead())
                    using (var sr = new StreamReader(fs, utf8))
                        json = sr.ReadToEnd();
                    cfg = JsonConvert.DeserializeObject<BotConfig>(json);
                } catch {
                    cfg = BotConfig.Default;
                }
            }
            
            return new DatabaseContextBuilder(cfg.DatabaseConfig).CreateContext();
        }
    }
}
