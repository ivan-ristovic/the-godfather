using System.Collections.Concurrent;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration.Services;

public sealed class GuildConfigService : ITheGodfatherService
{
    public bool IsDisabled => false;

    private ConcurrentDictionary<ulong, CachedGuildConfig> gcfgs;
    private readonly BotConfig cfg;
    private readonly DbContextBuilder dbb;


    public GuildConfigService(BotConfigService cfg, DbContextBuilder dbb, bool loadData = true)
    {
        this.cfg = cfg.CurrentConfiguration;
        this.dbb = dbb;
        this.gcfgs = new ConcurrentDictionary<ulong, CachedGuildConfig>();
        if (loadData)
            this.LoadData();
    }


    public void LoadData()
    {
        Log.Debug("Loading guild config");
        try {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            this.gcfgs = new ConcurrentDictionary<ulong, CachedGuildConfig>(
                db.Configs
                    .AsEnumerable()
                    .Select(gcfg => new KeyValuePair<ulong, CachedGuildConfig>(gcfg.GuildId, gcfg.CachedConfig)
                    ));
        } catch (Exception e) {
            Log.Error(e, "Loading guild configs failed");
        }
    }


    public bool IsGuildRegistered(ulong gid)
        => this.gcfgs.TryGetValue(gid, out _);

    public CachedGuildConfig GetCachedConfig(ulong? gid)
        => gid is null ? new CachedGuildConfig() : this.gcfgs.GetValueOrDefault(gid.Value, new CachedGuildConfig());

    public string GetGuildPrefix(ulong? gid)
    {
        if (gid is null)
            return this.cfg.Prefix;
        return this.gcfgs.TryGetValue(gid.Value, out CachedGuildConfig? gcfg) && !string.IsNullOrWhiteSpace(gcfg?.Prefix)
            ? this.gcfgs[gid.Value].Prefix
            : this.cfg.Prefix;
    }

    public async Task<GuildConfig> GetConfigAsync(ulong gid)
    {
        GuildConfig? gcfg = null;
        await using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
            gcfg = await db.Configs.FindAsync((long)gid);
        }

        return gcfg ?? new GuildConfig();
    }

    public async Task<GuildConfig> ModifyConfigAsync(ulong gid, Action<GuildConfig> modifyAction)
    {
        if (!this.gcfgs.ContainsKey(gid))
            throw new KeyNotFoundException($"Failed to find the guild in internal list: {gid}");

        GuildConfig? gcfg = null;
        await using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
            gcfg = await db.Configs.FindAsync((long)gid) ?? new GuildConfig();
            modifyAction(gcfg);
            db.Configs.Update(gcfg);
            await db.SaveChangesAsync();
        }

        this.gcfgs.AddOrUpdate(gid, gcfg.CachedConfig, (_, _) => gcfg.CachedConfig);
        return gcfg;
    }

    public async Task<bool> RegisterGuildAsync(ulong gid)
    {
        bool success = this.gcfgs.TryAdd(gid, new CachedGuildConfig());
        await using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
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
        this.gcfgs.TryRemove(gid, out _);
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        GuildConfig? gcfg = await db.Configs.FindAsync((long)gid);
        if (gcfg is { }) {
            db.Configs.Remove(gcfg);
            await db.SaveChangesAsync();
            Log.Debug("Unregistered guild: {GuildId}", gid);
        }
    }

    public bool IsChannelExempted(ulong gid, ulong cid, ulong? parentId = null)
    {
        using TheGodfatherDbContext db = this.dbb.CreateContext();
        return db.ExemptsLogging
            .AsQueryable()
            .Where(e => e.GuildIdDb == (long)gid)
            .AsEnumerable()
            .Any(e => e.Type == ExemptedEntityType.Channel && (e.Id == cid || e.Id == parentId));
    }

    public bool IsMemberExempted(ulong gid, ulong uid, IEnumerable<ulong>? rids = null)
    {
        bool exempted = false;

        using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
            exempted |= db.ExemptsLogging
                .AsQueryable()
                .Where(e => e.GuildIdDb == (long)gid)
                .AsEnumerable()
                .Any(e => (e.Type == ExemptedEntityType.Member && e.Id == uid)
                          || (e.Type == ExemptedEntityType.Role && (rids?.Contains(e.Id) ?? false))
                );
        }

        return exempted;
    }
}