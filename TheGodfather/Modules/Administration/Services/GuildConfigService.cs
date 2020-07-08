using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Database.Models;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class GuildConfigService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private ConcurrentDictionary<ulong, CachedGuildConfig> gcfg;
        private readonly BotConfig cfg;
        private readonly DbContextBuilder dbb;


        public GuildConfigService(BotConfigService cfg, DbContextBuilder dbb, bool loadData = true)
        {
            this.cfg = cfg.CurrentConfiguration;
            this.dbb = dbb;
            this.gcfg = new ConcurrentDictionary<ulong, CachedGuildConfig>();
            if (loadData)
                this.LoadData();
        }


        public void LoadData()
        {
            Log.Debug("Loading guild config");
            try {
                using (TheGodfatherDbContext db = this.dbb.CreateDbContext()) {
                    this.gcfg = new ConcurrentDictionary<ulong, CachedGuildConfig>(
                        db.Configs
                            .AsEnumerable()
                            .Select(gcfg => new KeyValuePair<ulong, CachedGuildConfig>(gcfg.GuildId, gcfg.CachedConfig)
                    ));
                }
            } catch (Exception e) {
                Log.Error(e, "Loading guild configs failed");
            }
        }


        public bool IsGuildRegistered(ulong gid)
            => this.gcfg.TryGetValue(gid, out _);

        public CachedGuildConfig? GetCachedConfig(ulong gid)
            => this.gcfg.GetValueOrDefault(gid);

        public string GetGuildPrefix(ulong gid)
        {
            return this.gcfg.TryGetValue(gid, out CachedGuildConfig? gcfg) && !string.IsNullOrWhiteSpace(gcfg?.Prefix)
                ? this.gcfg[gid].Prefix
                : this.cfg.Prefix;
        }

        public async Task<GuildConfig> GetConfigAsync(ulong gid)
        {
            GuildConfig? gcfg = null;
            using (TheGodfatherDbContext db = this.dbb.CreateDbContext())
                gcfg = await db.Configs.FindAsync((long)gid);
            return gcfg ?? new GuildConfig();
        }

        public async Task<GuildConfig?> ModifyConfigAsync(ulong gid, Action<GuildConfig> modifyAction)
        {
            if (modifyAction is null || !this.gcfg.ContainsKey(gid))
                return null;

            GuildConfig? gcfg = null;
            using (TheGodfatherDbContext db = this.dbb.CreateDbContext()) {
                gcfg = await db.Configs.FindAsync((long)gid) ?? new GuildConfig();
                modifyAction(gcfg);
                db.Configs.Update(gcfg);
                await db.SaveChangesAsync();
            }

            this.gcfg.AddOrUpdate(gid, gcfg.CachedConfig, (k, v) => gcfg.CachedConfig);
            return gcfg;
        }

        public async Task<bool> RegisterGuildAsync(ulong gid)
        {
            bool success = this.gcfg.TryAdd(gid, new CachedGuildConfig());
            using (TheGodfatherDbContext db = this.dbb.CreateDbContext()) {
                var gcfg = new GuildConfig { GuildId = gid };
                if (!db.Configs.Contains(gcfg)) {
                    db.Configs.Add(gcfg);
                    await db.SaveChangesAsync();
                    Log.Debug("Registered guild: {GuildId}", gid);
                }
            }
            return success;
        }

        public async Task UnregisterGuildAsync(ulong gid)
        {
            this.gcfg.TryRemove(gid, out _);
            using (TheGodfatherDbContext db = this.dbb.CreateDbContext()) {
                GuildConfig gcfg = await db.Configs.FindAsync((long)gid);
                if (gcfg is { }) {
                    db.Configs.Remove(gcfg);
                    await db.SaveChangesAsync();
                    Log.Debug("Unregistered guild: {GuildId}", gid);
                }
            }
        }

        public bool IsChannelExempted(ulong gid, ulong cid, ulong? parentId)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateDbContext()) {
                return db.ExemptsLogging
                    .Where(e => e.GuildId == gid)
                    .Any(e => e.Type == ExemptedEntityType.Channel && (e.Id == cid || e.Id == parentId));
            }
        }

        public bool IsMemberExempted(ulong gid, ulong uid, IReadOnlyList<ulong> rids)
        {
            bool exempted = false;

            using (TheGodfatherDbContext db = this.dbb.CreateDbContext()) {
                exempted |= db.ExemptsLogging
                    .Where(e => e.GuildId == gid)
                    .Any(e => e.Type == ExemptedEntityType.Member && e.Id == uid);
                if (rids?.Any() ?? false) {
                    exempted |= db.ExemptsLogging
                        .Where(e => e.GuildId == gid)
                        .Any(e => e.Type == ExemptedEntityType.Role && rids.Contains(e.Id));
                }
            }

            return exempted;
        }
    }
}
