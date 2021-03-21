using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class AntiMentionService : ProtectionService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentHashSet<ExemptedEntity>> guildExempts;
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserMentionInfo>> guildMentionInfo;
        private readonly Timer refreshTimer;


        private static void RefreshCallback(object? _)
        {
            AntiMentionService service = _ as AntiMentionService ?? throw new ArgumentException("Failed to cast provided argument in timer callback");

            foreach (ulong gid in service.guildMentionInfo.Keys) {
                IEnumerable<ulong> toRemove = service.guildMentionInfo[gid]
                    .Where(kvp => !kvp.Value.IsActive)
                    .Select(kvp => kvp.Key);

                foreach (ulong uid in toRemove)
                    service.guildMentionInfo[gid].TryRemove(uid, out UserMentionInfo _);
            }

            Log.Debug("Cleared outdated anti-mention information");
        }


        public AntiMentionService(DbContextBuilder dbb, LoggingService ls, SchedulingService ss, GuildConfigService gcs)
            : base(dbb, ls, ss, gcs, "_gf: Anti-mention")
        {
            this.guildExempts = new ConcurrentDictionary<ulong, ConcurrentHashSet<ExemptedEntity>>();
            this.guildMentionInfo = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, UserMentionInfo>>();
            this.refreshTimer = new Timer(RefreshCallback, this, TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
        }


        public override bool TryAddGuildToWatch(ulong gid)
            => this.guildMentionInfo.TryAdd(gid, new ConcurrentDictionary<ulong, UserMentionInfo>());

        public override bool TryRemoveGuildFromWatch(ulong gid)
        {
            bool success = true;
            success &= this.guildExempts.TryRemove(gid, out _);
            success &= this.guildMentionInfo.TryRemove(gid, out _);
            return success;
        }

        public async Task<IReadOnlyList<ExemptedMentionEntity>> GetExemptsAsync(ulong gid)
        {
            List<ExemptedMentionEntity> exempts;
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            exempts = await db.ExemptsMention.AsQueryable().Where(ex => ex.GuildIdDb == (long)gid).ToListAsync();
            return exempts.AsReadOnly();
        }

        public async Task ExemptAsync(ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            db.ExemptsMention.AddExemptions(gid, type, ids);
            await db.SaveChangesAsync();
            this.UpdateExemptsForGuildAsync(gid);
        }

        public async Task UnexemptAsync(ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            db.ExemptsMention.RemoveRange(
                db.ExemptsMention.AsQueryable().Where(ex => ex.GuildId == gid && ex.Type == type && ids.Any(id => id == ex.Id))
            );
            await db.SaveChangesAsync();
            this.UpdateExemptsForGuildAsync(gid);
        }

        public void UpdateExemptsForGuildAsync(ulong gid)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            this.guildExempts[gid] = new ConcurrentHashSet<ExemptedEntity>(
                db.ExemptsMention.AsQueryable().Where(ee => ee.GuildIdDb == (long)gid)
            );
        }

        public async Task HandleNewMessageAsync(MessageCreateEventArgs e, AntiMentionSettings settings)
        {
            if (!this.guildMentionInfo.ContainsKey(e.Guild.Id)) {
                if (!this.TryAddGuildToWatch(e.Guild.Id))
                    throw new ConcurrentOperationException("Failed to add guild to anti-mention watch list!");
                this.UpdateExemptsForGuildAsync(e.Guild.Id);
            }

            DiscordMember member = e.Author as DiscordMember ?? throw new ConcurrentOperationException("Message sender not part of guild.");
            if (this.guildExempts.TryGetValue(e.Guild.Id, out ConcurrentHashSet<ExemptedEntity>? exempts) && exempts.AnyAppliesTo(e))
                return;

            ConcurrentDictionary<ulong, UserMentionInfo> gMentionInfo = this.guildMentionInfo[e.Guild.Id];
            UserMentionInfo mentionInfo = gMentionInfo.GetOrAdd(e.Author.Id, new UserMentionInfo(settings.Sensitivity));
            int count = (e.Message.MentionEveryone ? 1 : 0) + e.MentionedChannels.Count + e.MentionedRoles.Count + e.MentionedUsers.Count;
            if (!mentionInfo.TryDecrementAllowedMentionCount(count)) {
                await this.PunishMemberAsync(e.Guild, member, settings.Action);
                mentionInfo.Reset();
            }
        }

        public override void Dispose()
        {
            this.refreshTimer.Dispose();
        }
    }
}
