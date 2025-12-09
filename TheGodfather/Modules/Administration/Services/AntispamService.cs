using System.Collections.Concurrent;
using System.Threading;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Common.Collections;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;

namespace TheGodfather.Modules.Administration.Services;

public sealed class AntispamService : ProtectionServiceBase
{
    private readonly ConcurrentDictionary<ulong, ConcurrentHashSet<ExemptedEntity>> guildExempts;
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserSpamInfo>> guildSpamInfo;
    private readonly Timer refreshTimer;


    private static void RefreshCallback(object? _)
    {
        AntispamService service = _ as AntispamService ?? throw new ArgumentException("Failed to cast provided argument in timer callback");

        foreach (ulong gid in service.guildSpamInfo.Keys) {
            IEnumerable<ulong> toRemove = service.guildSpamInfo[gid]
                .Where(kvp => !kvp.Value.IsActive)
                .Select(kvp => kvp.Key);

            foreach (ulong uid in toRemove)
                service.guildSpamInfo[gid].TryRemove(uid, out UserSpamInfo? _);
        }

        Log.Debug("Cleared outdated antispam information");
    }


    public AntispamService(DbContextBuilder dbb, LoggingService ls, SchedulingService ss, GuildConfigService gcs)
        : base(dbb, ls, ss, gcs, "_gf: Antispam")
    {
        this.guildExempts = new ConcurrentDictionary<ulong, ConcurrentHashSet<ExemptedEntity>>();
        this.guildSpamInfo = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserSpamInfo>>();
        this.refreshTimer = new Timer(RefreshCallback, this, TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
    }


    public override bool TryAddGuildToWatch(ulong gid)
        => this.guildSpamInfo.TryAdd(gid, new ConcurrentDictionary<ulong, UserSpamInfo>());

    public override bool TryRemoveGuildFromWatch(ulong gid)
    {
        bool success = true;
        success &= this.guildExempts.TryRemove(gid, out _);
        success &= this.guildSpamInfo.TryRemove(gid, out _);
        return success;
    }

    public async Task<IReadOnlyList<ExemptedSpamEntity>> GetExemptsAsync(ulong gid)
    {
        List<ExemptedSpamEntity> exempts;
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        exempts = await db.ExemptsSpam.AsQueryable().Where(ex => ex.GuildIdDb == (long)gid).ToListAsync();
        return exempts.AsReadOnly();
    }

    public async Task ExemptAsync(ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        db.ExemptsSpam.AddExemptions(gid, type, ids);
        await db.SaveChangesAsync();
        this.UpdateExemptsForGuildAsync(gid);
    }

    public async Task UnexemptAsync(ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
    {
        await using TheGodfatherDbContext db = this.dbb.CreateContext();
        db.ExemptsSpam.RemoveRange(
            db.ExemptsSpam.AsQueryable().Where(ex => ex.GuildId == gid && ex.Type == type && ids.Any(id => id == ex.Id))
        );
        await db.SaveChangesAsync();
        this.UpdateExemptsForGuildAsync(gid);
    }

    public void UpdateExemptsForGuildAsync(ulong gid)
    {
        using TheGodfatherDbContext db = this.dbb.CreateContext();
        this.guildExempts[gid] = new ConcurrentHashSet<ExemptedEntity>(
            db.ExemptsSpam.AsQueryable().Where(ee => ee.GuildIdDb == (long)gid)
        );
    }

    public async Task HandleNewMessageAsync(MessageCreateEventArgs e, AntispamSettings settings)
    {
        if (!this.guildSpamInfo.ContainsKey(e.Guild.Id)) {
            if (!this.TryAddGuildToWatch(e.Guild.Id))
                throw new ConcurrentOperationException("Failed to add guild to antispam watch list!");
            this.UpdateExemptsForGuildAsync(e.Guild.Id);
        }

        DiscordMember member = e.Author as DiscordMember ?? throw new ConcurrentOperationException("Message sender not part of guild.");
        if (this.guildExempts.TryGetValue(e.Guild.Id, out ConcurrentHashSet<ExemptedEntity>? exempts) && exempts.AnyAppliesTo(e))
            return;

        ConcurrentDictionary<ulong, UserSpamInfo> gSpamInfo = this.guildSpamInfo[e.Guild.Id];
        UserSpamInfo spamInfo = gSpamInfo.GetOrAdd(e.Author.Id, new UserSpamInfo(settings.Sensitivity));
        if (!spamInfo.TryDecrementAllowedMessageCount(e.Message.Content)) {
            await this.PunishMemberAsync(e.Guild, member, settings.Action);
            spamInfo.Reset();
        }
    }

    public override void Dispose() 
        => this.refreshTimer.Dispose();
}