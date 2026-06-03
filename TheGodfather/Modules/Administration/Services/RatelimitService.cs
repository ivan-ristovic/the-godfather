using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Modules.Administration.Services;

public sealed class RatelimitService : ExemptableProtectionServiceBase<ExemptedRatelimitEntity>
{
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserRatelimitInfo>> guildRatelimitInfo = new();


    public RatelimitService(DbContextBuilder dbb)
        : base(dbb, TimeSpan.FromSeconds(20)) { }


    public override DbSet<ExemptedRatelimitEntity> DbSetSelector(TheGodfatherDbContext db)
        => db.ExemptsRatelimit;

    public bool ShouldPunish(
        ulong gid,
        ulong uid,
        ulong cid,
        ulong? parentId,
        IEnumerable<ulong> roleIds,
        RatelimitSettings settings)
    {
        if (!settings.Enabled || this.IsExempted(gid, cid, parentId, uid, roleIds))
            return false;

        ConcurrentDictionary<ulong, UserRatelimitInfo> rateInfo = this.guildRatelimitInfo.GetOrAdd(gid, _ => new ConcurrentDictionary<ulong, UserRatelimitInfo>());
        UserRatelimitInfo userInfo = rateInfo.GetOrAdd(uid, _ => new UserRatelimitInfo(settings.Sensitivity));
        bool allowed = userInfo.TryDecrementAllowedMessageCount();
        if (!allowed)
            userInfo.Reset();
        return !allowed;
    }

    protected override void RefreshCallback(object? _)
    {
        foreach (ConcurrentDictionary<ulong, UserRatelimitInfo> guildInfo in this.guildRatelimitInfo.Values)
        foreach (ulong uid in guildInfo.Where(kvp => !kvp.Value.IsActive).Select(kvp => kvp.Key).ToArray())
            guildInfo.TryRemove(uid, out UserRatelimitInfo? _);
    }
}
