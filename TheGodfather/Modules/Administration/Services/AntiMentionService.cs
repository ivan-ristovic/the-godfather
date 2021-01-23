using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class AntiMentionService : ProtectionService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentHashSet<ExemptedEntity>> guildExempts;


        public AntiMentionService(DbContextBuilder dbb, LoggingService ls, SchedulingService ss, GuildConfigService gcs)
            : base(dbb, ls, ss, gcs, "_gf: Anti-mention")
        {
            this.guildExempts = new ConcurrentDictionary<ulong, ConcurrentHashSet<ExemptedEntity>>();
        }


        public override bool TryAddGuildToWatch(ulong gid)
            => this.guildExempts.TryAdd(gid, new ConcurrentHashSet<ExemptedEntity>());

        public override bool TryRemoveGuildFromWatch(ulong gid)
            => this.guildExempts.TryRemove(gid, out _);

        public async Task<IReadOnlyList<ExemptedSpamEntity>> GetExemptsAsync(ulong gid)
        {
            List<ExemptedSpamEntity> exempts;
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
                db.ExemptsAntispam.Where(ee => ee.GuildIdDb == (long)gid)
            );
        }

        public Task HandleNewMessageAsync(MessageCreateEventArgs e, AntiMentionSettings settings)
        {
            DiscordMember member = e.Author as DiscordMember ?? throw new ConcurrentOperationException("Message sender not part of guild.");
            if (this.guildExempts.TryGetValue(e.Guild.Id, out ConcurrentHashSet<ExemptedEntity>? exempts)) {
                if (exempts.Any(ee => ee.Type == ExemptedEntityType.Channel && (ee.Id == e.Channel.Id || ee.Id == e.Channel.ParentId)))
                    return Task.CompletedTask;
                if (exempts.Any(ee => ee.Type == ExemptedEntityType.Member && ee.Id == e.Author.Id))
                    return Task.CompletedTask;
                if (exempts.Any(ee => ee.Type == ExemptedEntityType.Role && member.Roles.Any(r => r.Id == ee.Id)))
                    return Task.CompletedTask;
            }

            return e.MentionedChannels.Count + e.MentionedRoles.Count + e.MentionedUsers.Count > settings.Sensitivity
                ? this.PunishMemberAsync(e.Guild, member, settings.Action)
                : Task.CompletedTask;
        }

        public override void Dispose()
        {

        }
    }
}
