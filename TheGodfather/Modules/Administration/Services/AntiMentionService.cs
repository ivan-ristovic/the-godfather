using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Modules.Administration.Services;

public sealed class AntiMentionService : ExemptableProtectionServiceBase<ExemptedMentionEntity>
{
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserMentionInfo>> guildMentionInfo = new();


    public AntiMentionService(DbContextBuilder dbb)
        : base(dbb, TimeSpan.FromMinutes(3)) { }


    public override DbSet<ExemptedMentionEntity> DbSetSelector(TheGodfatherDbContext db)
        => db.ExemptsMention;

    public bool ShouldPunish(
        ulong gid,
        ulong uid,
        ulong cid,
        ulong? parentId,
        IEnumerable<ulong> roleIds,
        int mentionCount,
        AntiMentionSettings settings)
    {
        if (!settings.Enabled || mentionCount <= 0 || this.IsExempted(gid, cid, parentId, uid, roleIds))
            return false;

        ConcurrentDictionary<ulong, UserMentionInfo> mentionInfo = this.guildMentionInfo.GetOrAdd(gid, _ => new ConcurrentDictionary<ulong, UserMentionInfo>());
        UserMentionInfo userInfo = mentionInfo.GetOrAdd(uid, _ => new UserMentionInfo(settings.Sensitivity));
        bool allowed = userInfo.TryDecrementAllowedMentionCount(mentionCount);
        if (!allowed)
            userInfo.Reset();
        return !allowed;
    }

    protected override void RefreshCallback(object? _)
    {
        foreach (ConcurrentDictionary<ulong, UserMentionInfo> guildInfo in this.guildMentionInfo.Values)
        foreach (ulong uid in guildInfo.Where(kvp => !kvp.Value.IsActive).Select(kvp => kvp.Key).ToArray())
            guildInfo.TryRemove(uid, out UserMentionInfo? _);
    }
}
