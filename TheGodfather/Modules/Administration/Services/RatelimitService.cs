using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class RatelimitService : ProtectionServiceBase
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentHashSet<ExemptedEntity>> guildExempts;
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserRatelimitInfo>> guildRatelimitInfo;
        private readonly Timer refreshTimer;


        private static void RefreshCallback(object? _)
        {
            RatelimitService service = _ as RatelimitService ?? throw new ArgumentException("Failed to cast provided argument in timer callback");

            foreach (ulong gid in service.guildRatelimitInfo.Keys) {
                IEnumerable<ulong> toRemove = service.guildRatelimitInfo[gid]
                    .Where(kvp => !kvp.Value.IsActive)
                    .Select(kvp => kvp.Key);

                foreach (ulong uid in toRemove)
                    service.guildRatelimitInfo[gid].TryRemove(uid, out UserRatelimitInfo _);
            }
        }


        public RatelimitService(DbContextBuilder dbb, LoggingService ls, SchedulingService ss, GuildConfigService gcs)
            : base(dbb, ls, ss, gcs, "_gf: Ratelimit hit")
        {
            this.guildExempts = new ConcurrentDictionary<ulong, ConcurrentHashSet<ExemptedEntity>>();
            this.guildRatelimitInfo = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserRatelimitInfo>>();
            this.refreshTimer = new Timer(RefreshCallback, this, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
        }


        public override bool TryAddGuildToWatch(ulong gid)
            => this.guildRatelimitInfo.TryAdd(gid, new ConcurrentDictionary<ulong, UserRatelimitInfo>());

        public override bool TryRemoveGuildFromWatch(ulong gid)
            => this.guildRatelimitInfo.TryRemove(gid, out _);

        public async Task<IReadOnlyList<ExemptedRatelimitEntity>> GetExemptsAsync(ulong gid)
        {
            List<ExemptedRatelimitEntity> exempts;
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            exempts = await db.ExemptsRatelimit.AsQueryable().Where(ex => ex.GuildIdDb == (long)gid).ToListAsync();
            return exempts.AsReadOnly();
        }

        public async Task ExemptAsync(ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            db.ExemptsRatelimit.AddExemptions(gid, type, ids);
            await db.SaveChangesAsync();
            this.UpdateExemptsForGuildAsync(gid);
        }

        public async Task UnexemptAsync(ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            db.ExemptsRatelimit.RemoveRange(
                db.ExemptsRatelimit.AsQueryable().Where(ex => ex.GuildId == gid && ex.Type == type && ids.Any(id => id == ex.Id))
            );
            await db.SaveChangesAsync();
            this.UpdateExemptsForGuildAsync(gid);
        }

        public void UpdateExemptsForGuildAsync(ulong gid)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            this.guildExempts[gid] = new ConcurrentHashSet<ExemptedEntity>(
                db.ExemptsRatelimit.AsQueryable().Where(ee => ee.GuildIdDb == (long)gid)
            );
        }

        public async Task HandleNewMessageAsync(MessageCreateEventArgs e, RatelimitSettings settings)
        {
            if (!this.guildRatelimitInfo.ContainsKey(e.Guild.Id)) {
                if (!this.TryAddGuildToWatch(e.Guild.Id))
                    throw new ConcurrentOperationException("Failed to add guild to ratelimit watch list!");
                this.UpdateExemptsForGuildAsync(e.Guild.Id);
            }

            DiscordMember member = e.Author as DiscordMember ?? throw new ConcurrentOperationException("Message sender not part of guild.");
            if (this.guildExempts.TryGetValue(e.Guild.Id, out ConcurrentHashSet<ExemptedEntity>? exempts) && exempts.AnyAppliesTo(e))
                return;

            ConcurrentDictionary<ulong, UserRatelimitInfo> gRateInfo = this.guildRatelimitInfo[e.Guild.Id];
            UserRatelimitInfo rateInfo = gRateInfo.GetOrAdd(e.Author.Id, new UserRatelimitInfo(settings.Sensitivity));
            if (!rateInfo.TryDecrementAllowedMessageCount()) {
                await this.PunishMemberAsync(e.Guild, member, settings.Action);
                rateInfo.Reset();
            }
        }

        public override void Dispose()
        {
            this.refreshTimer.Dispose();
        }
    }
}
