using System.Collections.Concurrent;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Common.Collections;

namespace TheGodfather.Modules.Administration.Services;

public abstract class ExemptableProtectionServiceBase<TExempt> : ITheGodfatherService, IDisposable
    where TExempt : ExemptedEntity, new()
{
    public bool IsDisabled => false;

    protected readonly DbContextBuilder dbb;

    private ConcurrentDictionary<ulong, ConcurrentHashSet<TExempt>> guildExempts = new();
    private readonly Timer refreshTimer;


    protected ExemptableProtectionServiceBase(DbContextBuilder dbb, TimeSpan refreshInterval)
    {
        this.dbb = dbb;
        this.refreshTimer = new Timer(this.RefreshCallback, null, refreshInterval, refreshInterval);
    }


    public virtual void Dispose()
        => this.refreshTimer.Dispose();

    public abstract DbSet<TExempt> DbSetSelector(TheGodfatherDbContext db);

    public async Task<IReadOnlyList<TExempt>> GetExemptsAsync(ulong gid)
    {
        List<TExempt> exempts;
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        exempts = await this.DbSetSelector(db).AsQueryable().Where(ex => ex.GuildIdDb == (long)gid).ToListAsync();
        return exempts.AsReadOnly();
    }

    public async Task ExemptAsync(ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        DbSet<TExempt> set = this.DbSetSelector(db);
        int added = 0;
        foreach (ulong id in ids.Distinct()) {
            TExempt? existing = await set.FindAsync((long)id, (long)gid, type);
            if (existing is not null)
                continue;

            set.Add(new TExempt { GuildId = gid, Id = id, Type = type });
            added++;
        }

        if (added > 0)
            await db.SaveChangesAsync();
        this.UpdateExemptsForGuild(gid);
    }

    public async Task UnexemptAsync(ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
    {
        ulong[] idArray = ids.Distinct().ToArray();
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        List<TExempt> toRemove = this.DbSetSelector(db)
            .AsQueryable()
            .Where(ex => ex.GuildIdDb == (long)gid && ex.Type == type)
            .AsEnumerable()
            .Where(ex => idArray.Contains(ex.Id))
            .ToList();
        this.DbSetSelector(db).RemoveRange(toRemove);
        if (toRemove.Count > 0)
            await db.SaveChangesAsync();
        this.UpdateExemptsForGuild(gid);
    }

    public void UpdateExemptsForGuild(ulong gid)
    {
        using TheGodfatherDbContext db = this.dbb.CreateContext();
        this.guildExempts[gid] = new ConcurrentHashSet<TExempt>(
            this.DbSetSelector(db).AsQueryable().Where(ex => ex.GuildIdDb == (long)gid)
        );
    }

    protected bool IsExempted(ulong gid, ulong cid, ulong? parentId, ulong uid, IEnumerable<ulong> roleIds)
    {
        if (!this.guildExempts.ContainsKey(gid))
            this.UpdateExemptsForGuild(gid);

        return this.guildExempts.TryGetValue(gid, out ConcurrentHashSet<TExempt>? exempts) && exempts.Any(ex => ex.Type switch {
            ExemptedEntityType.Channel => ex.Id == cid || ex.Id == parentId,
            ExemptedEntityType.Member => ex.Id == uid,
            ExemptedEntityType.Role => roleIds.Contains(ex.Id),
            _ => false
        });
    }

    protected virtual void RefreshCallback(object? _)
    { }
}
