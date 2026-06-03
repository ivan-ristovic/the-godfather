using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Modules.Administration.Services;

public sealed class AntispamService : ExemptableProtectionServiceBase<ExemptedSpamEntity>
{
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserSpamInfo>> guildSpamInfo = new();


    public AntispamService(DbContextBuilder dbb)
        : base(dbb, TimeSpan.FromMinutes(3)) { }


    public override DbSet<ExemptedSpamEntity> DbSetSelector(TheGodfatherDbContext db)
        => db.ExemptsSpam;

    public bool ShouldPunish(
        ulong gid,
        ulong uid,
        ulong cid,
        ulong? parentId,
        IEnumerable<ulong> roleIds,
        string? content,
        AntispamSettings settings)
    {
        if (!settings.Enabled || this.IsExempted(gid, cid, parentId, uid, roleIds))
            return false;

        ConcurrentDictionary<ulong, UserSpamInfo> spamInfo = this.guildSpamInfo.GetOrAdd(gid, _ => new ConcurrentDictionary<ulong, UserSpamInfo>());
        UserSpamInfo userInfo = spamInfo.GetOrAdd(uid, _ => new UserSpamInfo(settings.Sensitivity));
        bool allowed = userInfo.TryDecrementAllowedMessageCount(content);
        if (!allowed)
            userInfo.Reset();
        return !allowed;
    }

    protected override void RefreshCallback(object? _)
    {
        foreach (ConcurrentDictionary<ulong, UserSpamInfo> guildInfo in this.guildSpamInfo.Values)
        foreach (ulong uid in guildInfo.Where(kvp => !kvp.Value.IsActive).Select(kvp => kvp.Key).ToArray())
            guildInfo.TryRemove(uid, out UserSpamInfo? _);
    }
}
