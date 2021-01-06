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
    public sealed class AntispamService : ProtectionService
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
                    service.guildSpamInfo[gid].TryRemove(uid, out UserSpamInfo _);
            }
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

        public async Task<IReadOnlyList<ExemptedAntispamEntity>> GetExemptsAsync(ulong gid)
        {
            List<ExemptedAntispamEntity> exempts;
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            exempts = await db.ExemptsAntispam.Where(ex => ex.GuildIdDb == (long)gid).ToListAsync();
            return exempts.AsReadOnly();
        }

        public async Task ExemptAsync(ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            db.ExemptsAntispam.AddExemptions(gid, type, ids);
            await db.SaveChangesAsync();
            this.UpdateExemptsForGuildAsync(gid);
        }

        public async Task UnexemptAsync(ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            db.ExemptsAntispam.RemoveRange(
                db.ExemptsAntispam.Where(ex => ex.GuildId == gid && ex.Type == type && ids.Any(id => id == ex.Id))
            );
            await db.SaveChangesAsync();
            this.UpdateExemptsForGuildAsync(gid);
        }

        public void UpdateExemptsForGuildAsync(ulong gid)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            this.guildExempts[gid] = new ConcurrentHashSet<ExemptedEntity>(
                db.ExemptsAntispam
                    .Where(ee => ee.GuildId == gid)
                    .Select(ee => new ExemptedEntity { GuildId = ee.GuildId, Id = ee.Id, Type = ee.Type })
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
            if (this.guildExempts.TryGetValue(e.Guild.Id, out ConcurrentHashSet<ExemptedEntity>? exempts)) {
                if (exempts.Any(ee => ee.Type == ExemptedEntityType.Channel && (ee.Id == e.Channel.Id || ee.Id == e.Channel.ParentId)))
                    return;
                if (exempts.Any(ee => ee.Type == ExemptedEntityType.Member && ee.Id == e.Author.Id))
                    return;
                if (exempts.Any(ee => ee.Type == ExemptedEntityType.Role && member.Roles.Any(r => r.Id == ee.Id)))
                    return;
            }

            ConcurrentDictionary<ulong, UserSpamInfo> gSpamInfo = this.guildSpamInfo[e.Guild.Id];
            if (!gSpamInfo.ContainsKey(e.Author.Id)) {
                if (!gSpamInfo.TryAdd(e.Author.Id, new UserSpamInfo(settings.Sensitivity)))
                    throw new ConcurrentOperationException("Failed to add member to antispam watch list!");
                return;
            }

            if (gSpamInfo.TryGetValue(e.Author.Id, out UserSpamInfo? spamInfo) && !spamInfo.TryDecrementAllowedMessageCount(e.Message.Content)) {
                await this.PunishMemberAsync(e.Guild, member, settings.Action);
                spamInfo.Reset();
            }
        }

        public override void Dispose()
        {
            this.refreshTimer.Dispose();
        }
    }
}
