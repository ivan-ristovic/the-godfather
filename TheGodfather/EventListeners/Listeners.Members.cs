#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.GuildMemberAdded)]
        public static async Task MemberJoinEventHandlerAsync(TheGodfatherShard shard, GuildMemberAddEventArgs e)
        {
            DatabaseGuildConfig gcfg = await shard.Services.GetService<GuildConfigService>().GetConfigAsync(e.Guild.Id);
            await Task.Delay(TimeSpan.FromSeconds(gcfg.AntiInstantLeaveSettings.Cooldown + 1));

            if (e.Member.Guild is null)
                return;

            DiscordChannel wchn = e.Guild.GetChannel(gcfg.WelcomeChannelId);
            if (!(wchn is null)) {
                if (string.IsNullOrWhiteSpace(gcfg.WelcomeMessage))
                    await wchn.EmbedAsync($"Welcome to {Formatter.Bold(e.Guild.Name)}, {e.Member.Mention}!", StaticDiscordEmoji.Wave);
                else
                    await wchn.EmbedAsync(gcfg.WelcomeMessage.Replace("%user%", e.Member.Mention), StaticDiscordEmoji.Wave);
            }

            try {
                using (DatabaseContext db = shard.Database.CreateContext()) {
                    IQueryable<ulong> rids = db.AutoAssignableRoles
                        .Where(dbr => dbr.GuildId == e.Guild.Id)
                        .Select(dbr => dbr.RoleId);
                    foreach (ulong rid in rids.ToList()) {
                        try {
                            DiscordRole role = e.Guild.GetRole(rid);
                            if (!(role is null))
                                await e.Member.GrantRoleAsync(role);
                            else
                                db.AutoAssignableRoles.Remove(db.AutoAssignableRoles.Single(r => r.GuildId == e.Guild.Id && r.RoleId == rid));
                        } catch (Exception exc) {
                            LogExt.Debug(e.Client.ShardId, exc, new[] { "Failed to assign auto role", "{RoleId}", "{Guild}", "{Member}" }, rid, e.Guild, e.Member);
                        }
                    }
                }
            } catch (Exception exc) {
                LogExt.Warning(e.Client.ShardId, exc, new[] { "Failed to assign auto role(s)", "{Guild}", "{Member}" }, e.Guild, e.Member);
            }

            if (gcfg.LeaveChannelId == 0)
                return;

            var emb = new DiscordLogEmbedBuilder("Member joined", e.Member.ToString(), DiscordEventType.GuildMemberAdded);
            emb.WithThumbnailUrl(e.Member.AvatarUrl);
            emb.AddField("Registration time", e.Member.CreationTimestamp.ToUtcTimestamp(), inline: true);
            emb.AddField("Email", e.Member.Email);

            using (DatabaseContext db = shard.Database.CreateContext()) {
                if (db.ForbiddenNames.Any(n => n.GuildId == e.Guild.Id && n.Regex.IsMatch(e.Member.DisplayName))) {
                    try {
                        await e.Member.ModifyAsync(m => {
                            m.Nickname = "Temporary name";
                            m.AuditLogReason = "_gf: Forbidden name match";
                        });
                        emb.AddField("Additional actions taken", "Removed name due to a match with a forbidden name");
                        if (!e.Member.IsBot)
                            await e.Member.SendMessageAsync($"Your nickname in the guild {e.Guild.Name} is forbidden by the guild administrator. Please set a different name.");
                    } catch (UnauthorizedException) {
                        emb.AddField("Error", "Matched forbidden name, but I failed to remove it. Check my permissions");
                    }
                }
            }

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildMemberAdded)]
        public static async Task MemberJoinProtectionEventHandlerAsync(TheGodfatherShard shard, GuildMemberAddEventArgs e)
        {
            if (e.Member is null || e.Member.IsBot)
                return;

            DatabaseGuildConfig gcfg = await shard.Services.GetService<GuildConfigService>().GetConfigAsync(e.Guild.Id);

            if (gcfg.AntifloodEnabled)
                await shard.CNext.Services.GetService<AntifloodService>().HandleMemberJoinAsync(e, gcfg.AntifloodSettings);

            if (gcfg.AntiInstantLeaveEnabled)
                await shard.CNext.Services.GetService<AntiInstantLeaveService>().HandleMemberJoinAsync(e, gcfg.AntiInstantLeaveSettings);
        }

        [AsyncEventListener(DiscordEventType.GuildMemberRemoved)]
        public static async Task MemberRemoveEventHandlerAsync(TheGodfatherShard shard, GuildMemberRemoveEventArgs e)
        {
            if (e.Member.IsCurrent)
                return;

            DatabaseGuildConfig gcfg = await shard.Services.GetService<GuildConfigService>().GetConfigAsync(e.Guild.Id);
            bool punished = false;

            if (gcfg.AntiInstantLeaveEnabled)
                punished = await shard.CNext.Services.GetService<AntiInstantLeaveService>().HandleMemberLeaveAsync(e, gcfg.AntiInstantLeaveSettings);

            if (!punished) {
                DiscordChannel lchn = e.Guild.GetChannel(gcfg.LeaveChannelId);
                if (!(lchn is null)) {
                    if (string.IsNullOrWhiteSpace(gcfg.LeaveMessage))
                        await lchn.EmbedAsync($"{Formatter.Bold(e.Member?.Username ?? "Member")} left the server! Bye!", StaticDiscordEmoji.Wave);
                    else
                        await lchn.EmbedAsync(gcfg.LeaveMessage.Replace("%user%", e.Member?.Username ?? "Unknown"), StaticDiscordEmoji.Wave);
                }
            }

            if (gcfg.LeaveChannelId == 0)
                return;

            var emb = new DiscordLogEmbedBuilder("Member left", e.Member.ToString(), DiscordEventType.GuildMemberRemoved);

            DiscordAuditLogKickEntry entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogKickEntry>(AuditLogActionType.Kick);
            if (!(entry is null) && entry.Target.Id == e.Member.Id) {
                emb.WithTitle("Member kicked");
                emb.AddInvocationFields(entry.UserResponsible);
                emb.AddField("Reason", entry.Reason, null);
            }

            emb.WithThumbnailUrl(e.Member.AvatarUrl);
            emb.AddField("Registration time", e.Member.CreationTimestamp.ToUtcTimestamp(), inline: true);
            emb.AddField("Email", e.Member.Email);

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildMemberUpdated)]
        public static async Task MemberUpdateEventHandlerAsync(TheGodfatherShard shard, GuildMemberUpdateEventArgs e)
        {
            bool renamed = false, failed = false;

            using (DatabaseContext db = shard.Database.CreateContext()) {
                if (!string.IsNullOrWhiteSpace(e.NicknameAfter) && db.ForbiddenNames.Any(n => n.GuildId == e.Guild.Id && n.Regex.IsMatch(e.NicknameAfter))) {
                    try {
                        await e.Member.ModifyAsync(m => {
                            m.Nickname = e.NicknameBefore;
                            m.AuditLogReason = "_gf: Forbidden name match";
                        });
                        renamed = true;
                        if (!e.Member.IsBot)
                            await e.Member.SendMessageAsync($"The nickname you tried to set in the guild {e.Guild.Name} is forbidden by the guild administrator. Please set a different name.");
                    } catch (UnauthorizedException) {
                        failed = true;
                    }
                }
            }

            if (shard.Services.GetService<GuildConfigService>().GetLogChannelForGuild(e.Guild) is null)
                return;

            var emb = new DiscordLogEmbedBuilder("Member updated", e.Member.ToString(), DiscordEventType.GuildMemberUpdated);
            emb.WithThumbnailUrl(e.Member.AvatarUrl);

            DiscordAuditLogMemberUpdateEntry entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogMemberUpdateEntry>(AuditLogActionType.MemberUpdate);
            if (entry is null) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                emb.AddField("Name before", e.NicknameBefore, inline: true);
                emb.AddField("Name after", e.NicknameAfter, inline: true);
                emb.AddField("Roles before", e.RolesBefore?.Count.ToString(), inline: true);
                emb.AddField("Roles after", e.RolesAfter?.Count.ToString(), inline: true);
            } else {
                emb.AddInvocationFields(entry.UserResponsible);
                emb.AddPropertyChangeField("Nickname change", entry.NicknameChange);
                if (!(entry.AddedRoles is null))
                    emb.AddField("Added roles", entry.AddedRoles.Select(r => r.Name), inline: true);
                if (!(entry.RemovedRoles is null))
                    emb.AddField("Removed roles", entry.RemovedRoles.Select(r => r.Name), inline: true);
                emb.AddField("Reason", entry.Reason, null);
                emb.WithTimestampFooter(entry.CreationTimestamp, entry.UserResponsible.AvatarUrl);
            }

            if (renamed)
                emb.AddField("Additional actions taken", "Removed name due to a match with a forbidden name");
            if (failed)
                emb.AddField("Error", "Matched forbidden name, but I failed to remove it. Check my permissions");

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.PresenceUpdated)]
        public static async Task MemberPresenceUpdateEventHandlerAsync(TheGodfatherShard shard, PresenceUpdateEventArgs e)
        {
            if (e.User.IsBot)
                return;

            var emb = new DiscordLogEmbedBuilder("Presence updated", e.User.ToString(), DiscordEventType.PresenceUpdated);
            emb.AddPropertyChangeField("Username change", e.UserBefore, e.UserAfter);
            emb.AddPropertyChangeField("Discriminator change", e.UserBefore.Discriminator, e.UserAfter.Discriminator);
            if (e.UserAfter.AvatarUrl != e.UserBefore.AvatarUrl)
                emb.AddField("Changed avatar", Formatter.MaskedUrl("Old Avatar (note: might 404 later)", new Uri(e.UserBefore.AvatarUrl)));
            emb.WithThumbnailUrl(e.UserAfter.AvatarUrl);

            if (!emb.Builder.Fields.Any())
                return;

            GuildConfigService gcs = shard.Services.GetService<GuildConfigService>();
            LoggingService ls = shard.Services.GetService<LoggingService>();
            IEnumerable<DiscordGuild> guilds = TheGodfather.ActiveShards
                .SelectMany(s => s.Client?.Guilds)
                .Select(kvp => kvp.Value)
                ?? Enumerable.Empty<DiscordGuild>();
            foreach (DiscordGuild guild in guilds) {
                if (gcs.GetLogChannelForGuild(guild) is null)
                    continue;
                if (await e.UserAfter.IsMemberOfGuildAsync(guild))
                    await ls.LogAsync(guild, emb);
            }
        }
    }
}
