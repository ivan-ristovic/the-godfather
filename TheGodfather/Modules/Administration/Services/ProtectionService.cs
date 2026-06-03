using Microsoft.EntityFrameworkCore;

namespace TheGodfather.Modules.Administration.Services;

public sealed class ProtectionService : ITheGodfatherService
{
    public bool IsDisabled => false;

    private readonly DbContextBuilder dbb;


    public ProtectionService(DbContextBuilder dbb)
    {
        this.dbb = dbb;
    }


    public async Task LogPunishmentAsync(ulong gid, ulong uid, Punishment.Action action)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        if (await db.Punishments.FindAsync((long)gid, (long)uid, action) is not null)
            return;

        db.Punishments.Add(new Punishment { GuildId = gid, UserId = uid, Type = action });
        await db.SaveChangesAsync();
    }

    public async Task RemoveLoggedPunishmentAsync(ulong gid, ulong uid, Punishment.Action action)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        Punishment? existing = await db.Punishments.FindAsync((long)gid, (long)uid, action);
        if (existing is null)
            return;

        db.Punishments.Remove(existing);
        await db.SaveChangesAsync();
    }

    public async Task<Punishment?> GetStrongestLoggedPunishmentAsync(ulong gid, ulong uid)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        return await db.Punishments
            .AsQueryable()
            .Where(p => p.GuildIdDb == (long)gid && p.UserIdDb == (long)uid)
            .OrderByDescending(p => p.Type)
            .FirstOrDefaultAsync();
    }
}
