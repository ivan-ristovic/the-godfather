using System.Collections.Concurrent;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Modules.Administration.Services;

public sealed class AntifloodService : ITheGodfatherService
{
    public bool IsDisabled => false;

    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTimeOffset>> guildJoins = new();


    public IReadOnlyList<ulong> RegisterJoin(ulong gid, ulong uid, AntifloodSettings settings)
    {
        if (!settings.Enabled)
            return Array.Empty<ulong>();

        DateTimeOffset now = DateTimeOffset.UtcNow;
        ConcurrentDictionary<ulong, DateTimeOffset> joins = this.guildJoins.GetOrAdd(gid, _ => new ConcurrentDictionary<ulong, DateTimeOffset>());
        foreach ((ulong existingUid, DateTimeOffset expiresAt) in joins.ToArray())
            if (expiresAt <= now)
                joins.TryRemove(existingUid, out _);

        joins[uid] = now + TimeSpan.FromSeconds(settings.Cooldown);
        if (joins.Count < settings.Sensitivity)
            return Array.Empty<ulong>();

        ulong[] offenders = joins.Keys.ToArray();
        joins.Clear();
        return offenders;
    }
}
