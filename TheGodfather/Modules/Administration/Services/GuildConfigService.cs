using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class GuildConfigService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private readonly ConcurrentDictionary<ulong, CachedGuildConfig> gcfg;
        private readonly BotConfig cfg;
        private readonly DatabaseContextBuilder dbb;


        public GuildConfigService(BotConfig cfg, DatabaseContextBuilder dbb)
        {
            this.cfg = cfg;
            this.dbb = dbb;
            this.gcfg = new ConcurrentDictionary<ulong, CachedGuildConfig>();
        }


        public bool IsGuildRegistered(ulong gid)
            => this.gcfg.TryGetValue(gid, out _);

        public CachedGuildConfig GetCachedConfig(ulong gid)
            => this.gcfg.GetOrAdd(gid, CachedGuildConfig.Default);

        public void ModifyCachedConfig(ulong gid, Func<CachedGuildConfig, CachedGuildConfig> modifyAction)
            => this.gcfg[gid] = modifyAction(this.gcfg[gid]);

        public string GetGuildPrefix(ulong gid)
        {
            return this.gcfg.TryGetValue(gid, out CachedGuildConfig gcfg) && !string.IsNullOrWhiteSpace(gcfg.Prefix)
                ? this.gcfg[gid].Prefix
                : this.cfg.DefaultPrefix;
        }

        public DiscordChannel GetLogChannelForGuild(DiscordGuild guild)
        {
            CachedGuildConfig gcfg = this.GetCachedConfig(guild.Id);
            return gcfg.LoggingEnabled ? guild.GetChannel(gcfg.LogChannelId) : null;
        }

        public async Task<DatabaseGuildConfig> GetConfigAsync(ulong gid)
        {
            DatabaseGuildConfig gcfg = null;
            using (DatabaseContext db = this.dbb.CreateContext())
                gcfg = await db.GuildConfig.FindAsync((long)gid) ?? new DatabaseGuildConfig();
            return gcfg;
        }

        public async Task<DatabaseGuildConfig> ModifyConfigAsync(ulong gid, Action<DatabaseGuildConfig> modifyAction)
        {
            DatabaseGuildConfig gcfg = null;
            using (DatabaseContext db = this.dbb.CreateContext()) {
                gcfg = await db.GuildConfig.FindAsync((long)gid) ?? new DatabaseGuildConfig();
                modifyAction(gcfg);
                db.GuildConfig.Update(gcfg);
                await db.SaveChangesAsync();
            }

            // TODO wtf did i write here???
            CachedGuildConfig cgcfg = this.GetCachedConfig(gid);
            cgcfg = gcfg.CachedConfig;
            this.ModifyCachedConfig(gid, _ => cgcfg);

            return gcfg;
        }

        public async Task<bool> RegisterGuildAsync(ulong gid)
        {
            bool success = this.gcfg.TryAdd(gid, CachedGuildConfig.Default);
            using (DatabaseContext db = this.dbb.CreateContext()) {
                var gcfg = new DatabaseGuildConfig { GuildId = gid };
                if (!db.GuildConfig.Contains(gcfg)) {
                    db.GuildConfig.Add(gcfg);
                    await db.SaveChangesAsync();
                }
            }
            return success;
        }

        public async Task UnregisterGuildAsync(ulong gid)
        {
            this.gcfg.TryRemove(gid, out _);
            using (DatabaseContext db = this.dbb.CreateContext()) {
                db.GuildConfig.Remove(db.GuildConfig.Find(gid));
                await db.SaveChangesAsync();
            }
        }
    }
}
