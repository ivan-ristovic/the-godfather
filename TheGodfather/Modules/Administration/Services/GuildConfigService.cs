using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class GuildConfigService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private ConcurrentDictionary<ulong, CachedGuildConfig> gcfg;
        private readonly BotConfig cfg;
        private readonly DatabaseContextBuilder dbb;


        public GuildConfigService(BotConfig cfg, DatabaseContextBuilder dbb, bool loadData = true)
        {
            this.cfg = cfg;
            this.dbb = dbb;
            this.gcfg = new ConcurrentDictionary<ulong, CachedGuildConfig>();
            if (loadData)
                this.LoadData();
        }


        public void LoadData()
        {
            Log.Debug("Loading guild config...");
            try {
                using (DatabaseContext db = this.dbb.CreateContext()) {
                    this.gcfg = new ConcurrentDictionary<ulong, CachedGuildConfig>(db.GuildConfig.Select(
                        gcfg => new KeyValuePair<ulong, CachedGuildConfig>(gcfg.GuildId, gcfg.CachedConfig
                    )));
                }
            } catch (Exception e) {
                Log.Error(e, "Loading guild configs failed");
            }
        }


        public bool IsGuildRegistered(ulong gid)
            => this.gcfg.TryGetValue(gid, out _);

        public CachedGuildConfig GetCachedConfig(ulong gid)
            => this.gcfg.GetValueOrDefault(gid);

        public string GetGuildPrefix(ulong gid)
        {
            return this.gcfg.TryGetValue(gid, out CachedGuildConfig gcfg) && !string.IsNullOrWhiteSpace(gcfg.Prefix)
                ? this.gcfg[gid].Prefix
                : this.cfg.Prefix;
        }

        public async Task<DatabaseGuildConfig> GetConfigAsync(ulong gid)
        {
            DatabaseGuildConfig gcfg = null;
            using (DatabaseContext db = this.dbb.CreateContext())
                gcfg = await db.GuildConfig.FindAsync((long)gid);
            return gcfg;
        }

        public async Task<DatabaseGuildConfig> ModifyConfigAsync(ulong gid, Action<DatabaseGuildConfig> modifyAction)
        {
            if (modifyAction is null || !this.gcfg.ContainsKey(gid))
                return null;

            DatabaseGuildConfig gcfg = null;
            using (DatabaseContext db = this.dbb.CreateContext()) {
                gcfg = await db.GuildConfig.FindAsync((long)gid) ?? new DatabaseGuildConfig();
                modifyAction(gcfg);
                db.GuildConfig.Update(gcfg);
                await db.SaveChangesAsync();
            }

            this.gcfg.AddOrUpdate(gid, gcfg.CachedConfig, (k, v) => gcfg.CachedConfig);
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
                DatabaseGuildConfig gcfg = await db.GuildConfig.FindAsync((long)gid);
                if (!(gcfg is null)) {
                    db.GuildConfig.Remove(gcfg);
                    await db.SaveChangesAsync();
                }
            }
        }

        public bool IsChannelExempted(ulong gid, ulong cid, ulong? parentId)
        {
            using (DatabaseContext db = this.dbb.CreateContext()) {
                return db.LoggingExempts
                    .Where(e => e.GuildId == gid)
                    .Any(e => e.Type == ExemptedEntityType.Channel && (e.Id == cid || e.Id == parentId));
            }
        }

        public bool IsMemberExempted(ulong gid, ulong uid, IReadOnlyList<ulong> rids)
        {
            bool exempted = false;

            using (DatabaseContext db = this.dbb.CreateContext()) {
                exempted |= db.LoggingExempts
                    .Where(e => e.GuildId == gid)
                    .Any(e => e.Type == ExemptedEntityType.Member && e.Id == uid);
                exempted |= db.LoggingExempts
                    .Where(e => e.GuildId == gid)
                    .Any(e => e.Type == ExemptedEntityType.Role && rids.Contains(e.Id));
            }

            return exempted;
        }
    }
}
