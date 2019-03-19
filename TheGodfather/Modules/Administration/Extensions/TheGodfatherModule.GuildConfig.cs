#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
#endregion

namespace TheGodfather.Modules.Administration.Extensions
{
    public static class TheGodfatherModuleGuildConfigExtensions
    {
        public static async Task<DatabaseGuildConfig> GetGuildConfigAsync(this TheGodfatherModule module, ulong gid)
        {
            DatabaseGuildConfig gcfg = null;
            using (DatabaseContext db = module.Database.CreateContext())
                gcfg = await db.GuildConfig.FindAsync((long)gid) ?? new DatabaseGuildConfig();
            return gcfg;
        }

        public static async Task<DatabaseGuildConfig> ModifyGuildConfigAsync(this TheGodfatherModule module, 
            ulong gid, Action<DatabaseGuildConfig> action)
        {
            DatabaseGuildConfig gcfg = null;
            using (DatabaseContext db = module.Database.CreateContext()) {
                gcfg = await db.GuildConfig.FindAsync((long)gid) ?? new DatabaseGuildConfig();
                action(gcfg);
                db.GuildConfig.Update(gcfg);
                await db.SaveChangesAsync();
            }
            
            CachedGuildConfig cgcfg = module.Shared.GetGuildConfig(gid);
            cgcfg = gcfg.CachedConfig;
            module.Shared.UpdateGuildConfig(gid, _ => cgcfg);

            return gcfg;
        }
    }
}
