using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration.Services
{
    public abstract class ProtectionServiceBase : ITheGodfatherService, IDisposable
    {
        protected SemaphoreSlim csem = new(1, 1);
        protected string reason;
        protected readonly DbContextBuilder dbb;
        protected readonly SchedulingService ss;
        protected readonly LoggingService ls;
        protected readonly GuildConfigService gcs;

        public bool IsDisabled => false;


        protected ProtectionServiceBase(DbContextBuilder dbb, LoggingService ls, SchedulingService ss, GuildConfigService gcs, string reason)
        {
            this.dbb = dbb;
            this.ss = ss;
            this.ls = ls;
            this.gcs = gcs;
            this.reason = reason;
        }


        public async Task PunishMemberAsync(DiscordGuild guild, DiscordMember member, Punishment.Action type, TimeSpan? cooldown = null, string? reason = null)
        {
            Log.Debug("Punishing {Member} in guild {Guild} with action {ActionType} due to: {Reason}", member, guild, type, reason ?? this.reason);
            try {
                DiscordRole muteRole;
                GuildTask gt;
                switch (type) {
                    case Punishment.Action.Kick:
                        await member.RemoveAsync(reason ?? this.reason);
                        break;
                    case Punishment.Action.PermanentMute:
                        muteRole = await this.GetOrCreateMuteRoleAsync(guild);
                        if (member.Roles.Contains(muteRole))
                            return;
                        await member.GrantRoleAsync(muteRole, reason ?? this.reason);
                        await this.LogPunishmentInCaseOfRejoinAsync(guild, member, type);
                        break;
                    case Punishment.Action.PermanentBan:
                        await member.BanAsync(0, reason: reason ?? this.reason);
                        break;
                    case Punishment.Action.TemporaryBan:
                        await member.BanAsync(0, reason: reason ?? this.reason);
                        gt = new GuildTask {
                            ExecutionTime = DateTimeOffset.Now + (cooldown ?? TimeSpan.FromDays(1)),
                            GuildId = guild.Id,
                            UserId = member.Id,
                            Type = ScheduledTaskType.Unban,
                        };
                        await this.ss.ScheduleAsync(gt);
                        break;
                    case Punishment.Action.TemporaryMute:
                        muteRole = await this.GetOrCreateMuteRoleAsync(guild);
                        if (member.Roles.Contains(muteRole))
                            return;
                        await member.GrantRoleAsync(muteRole, reason ?? this.reason);
                        gt = new GuildTask {
                            ExecutionTime = DateTimeOffset.Now + (cooldown ?? TimeSpan.FromHours(1)),
                            GuildId = guild.Id,
                            RoleId = muteRole.Id,
                            UserId = member.Id,
                            Type = ScheduledTaskType.Unmute,
                        };
                        await this.ss.ScheduleAsync(gt);
                        await this.LogPunishmentInCaseOfRejoinAsync(guild, member, type);
                        break;
                }
            } catch (Exception e) {
                Log.Error(e, "Failed to punish ({PunishmentType}) {Member} in {Guild}", type, member, guild);
                if (this.ls.IsLogEnabledFor(guild.Id, out LocalizedEmbedBuilder emb)) {
                    emb.WithLocalizedTitle("err-punish-failed");
                    emb.WithColor(DiscordColor.Red);
                    emb.AddLocalizedTitleField("str-user", member);
                    emb.AddLocalizedTitleField("str-rsn", reason ?? this.reason);
                    await this.ls.LogAsync(guild, emb.Build());
                }
            }
        }

        public async Task<DiscordRole?> UnsafeGetMuteRoleAsync(DiscordGuild guild)
        {
            DiscordRole? muteRole = null;
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            GuildConfig gcfg = await this.gcs.GetConfigAsync(guild.Id);
            muteRole = guild.GetRole(gcfg.MuteRoleId);
            if (muteRole is null)
                muteRole = guild.Roles.Select(kvp => kvp.Value).FirstOrDefault(r => r.Name.ToLowerInvariant() == "gf_mute");
            return muteRole;
        }

        public async Task<DiscordRole> GetOrCreateMuteRoleAsync(DiscordGuild guild)
        {
            DiscordRole? muteRole = null;

            await this.csem.WaitAsync();
            try {
                using TheGodfatherDbContext db = this.dbb.CreateContext();
                GuildConfig gcfg = await this.gcs.GetConfigAsync(guild.Id);
                muteRole = guild.GetRole(gcfg.MuteRoleId);
                if (muteRole is null)
                    muteRole = guild.Roles.Select(kvp => kvp.Value).FirstOrDefault(r => r.Name.ToLowerInvariant() == "gf_mute");
                if (muteRole is null) {
                    muteRole = await guild.CreateRoleAsync("gf_mute", hoist: false, mentionable: false);

                    IEnumerable<DiscordChannel> overwriteTargets = guild.Channels
                        .Select(kvp => kvp.Value)
                        .Where(c => c.Type == ChannelType.Category
                                 || ((c.Type == ChannelType.Text || c.Type == ChannelType.Voice) && c.Parent is null)
                        );

                    await Task.WhenAll(overwriteTargets.Select(c => AddOverwrite(c, muteRole)));

                    gcfg.MuteRoleId = muteRole.Id;
                    db.Configs.Update(gcfg);
                    await db.SaveChangesAsync();
                }
            } finally {
                this.csem.Release();
            }

            return muteRole;


            static async Task AddOverwrite(DiscordChannel channel, DiscordRole muteRole)
            {
                await channel.AddOverwriteAsync(
                    muteRole,
                    deny: Permissions.SendMessages
                        | Permissions.SendTtsMessages
                        | Permissions.AddReactions
                        | Permissions.Speak
                );
                await Task.Delay(10);
            }
        }

        public async Task LogPunishmentInCaseOfRejoinAsync(DiscordGuild guild, DiscordMember member, Punishment.Action action)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                Punishment? existing = db.Punishments.Find((long)guild.Id, (long)member.Id, (int)action);
                if (existing is null) {
                    db.Punishments.Add(new Punishment {
                        GuildId = guild.Id,
                        UserId = member.Id,
                        Type = action,
                    });
                    await db.SaveChangesAsync();
                }
            }
        }

        public Task RemoveLoggedPunishmentInCaseOfRejoinAsync(DiscordGuild guild, DiscordMember member, Punishment.Action action)
            => this.RemoveLoggedPunishmentInCaseOfRejoinAsync(new Punishment { GuildId = guild.Id, UserId = member.Id, Type = action });

        public async Task RemoveLoggedPunishmentInCaseOfRejoinAsync(Punishment punishment)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                Punishment? existing = db.Punishments.Find(punishment.GuildIdDb, punishment.UserIdDb, (int)punishment.Type);
                if (existing is not null) {
                    db.Punishments.Remove(existing);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<bool> ReapplyLoggedPunishmentsIfNececaryAsync(DiscordGuild guild, DiscordMember member)
        {
            Punishment? punishment;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                List<Punishment> punishments = await db.Punishments
                    .AsQueryable()
                    .Where(p => p.GuildIdDb == (long)guild.Id && p.UserIdDb == (long)member.Id)
                    .ToListAsync();
                punishment = punishments.Any()
                    ? punishments.Aggregate((p1, p2) => p1.Type > p2.Type ? p1 : p2)
                    : null;
            }

            if (punishment is null)
                return false;

            await this.PunishMemberAsync(guild, member, punishment.Type, reason: "_gf: Re-applied punishment due to rejoin");
            return true;
        }

        public abstract bool TryAddGuildToWatch(ulong gid);
        public abstract bool TryRemoveGuildFromWatch(ulong gid);
        public virtual void Dispose() { }
    }
}
