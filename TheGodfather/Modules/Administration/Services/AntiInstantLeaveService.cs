using System.Collections.Concurrent;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Modules.Administration.Services;

public sealed class AntiInstantLeaveService : ITheGodfatherService
{
    public bool IsDisabled => false;

    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTimeOffset>> guildNewMembers = new();


    public void RegisterJoin(ulong gid, ulong uid, AntiInstantLeaveSettings settings)
    {
        if (!settings.Enabled)
            return;

        ConcurrentDictionary<ulong, DateTimeOffset> members = this.guildNewMembers.GetOrAdd(gid, _ => new ConcurrentDictionary<ulong, DateTimeOffset>());
        members[uid] = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(settings.Cooldown);
    }

    public bool TryConsumeInstantLeave(ulong gid, ulong uid)
    {
        if (!this.guildNewMembers.TryGetValue(gid, out ConcurrentDictionary<ulong, DateTimeOffset>? members))
            return false;

        if (!members.TryRemove(uid, out DateTimeOffset expiresAt))
            return false;

        return expiresAt > DateTimeOffset.UtcNow;
    }
}
