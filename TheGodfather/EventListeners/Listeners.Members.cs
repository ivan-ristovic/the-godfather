#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.GuildMemberAdded)]
        public static async Task MemberJoinEventHandlerAsync(TheGodfatherShard shard, GuildMemberAddEventArgs e)
        {
            DatabaseGuildConfig gcfg = e.Guild.GetGuildSettings(shard.Database);
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
                            shard.Log(LogLevel.Debug,
                                $"| Failed to assign an automatic role to a new member!\n" +
                                $"| {e.Guild.ToString()}\n" +
                                $"| Exception: {exc.GetType()}\n" +
                                $"| Message: {exc.Message}"
                            );
                        }
                    }
                }
            } catch (Exception exc) {
                shard.SharedData.LogProvider.Log(LogLevel.Debug, exc);
            }

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Member, "Member joined", e.Member.ToString());
            emb.WithThumbnailUrl(e.Member.AvatarUrl);
            emb.AddField("Registration time", e.Member.CreationTimestamp.ToUtcTimestamp(), inline: true);
            if (!string.IsNullOrWhiteSpace(e.Member.Email))
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
                        emb.AddField("Additional actions taken", "Matched forbidden name, but I failed to remove it. Check my permissions");
                    }
                }
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.GuildMemberAdded)]
        public static async Task MemberJoinProtectionEventHandlerAsync(TheGodfatherShard shard, GuildMemberAddEventArgs e)
        {
            if (e.Member is null || e.Member.IsBot)
                return;

            DatabaseGuildConfig gcfg = e.Guild.GetGuildSettings(shard.Database);

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

            DatabaseGuildConfig gcfg = e.Guild.GetGuildSettings(shard.Database);
            bool punished = false;

            if (gcfg.AntiInstantLeaveEnabled)
                punished = await shard.CNext.Services.GetService<AntiInstantLeaveService>().HandleMemberLeaveAsync(e, gcfg.AntiInstantLeaveSettings);

            if (!punished) {
                DiscordChannel lchn = e.Guild.GetChannel(gcfg.LeaveChannelId);
                if (!(lchn is null)) {
                    if (string.IsNullOrWhiteSpace(gcfg.LeaveMessage))
                        await lchn.EmbedAsync($"{Formatter.Bold(e.Member?.Username ?? _unknown)} left the server! Bye!", StaticDiscordEmoji.Wave);
                    else
                        await lchn.EmbedAsync(gcfg.LeaveMessage.Replace("%user%", e.Member?.Username ?? _unknown), StaticDiscordEmoji.Wave);
                }
            }

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Member, "Member left", e.Member.ToString());

            DiscordAuditLogEntry kickEntry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.Kick);
            if (!(kickEntry is null) && kickEntry is DiscordAuditLogKickEntry ke && ke.Target.Id == e.Member.Id) {
                emb.WithTitle("Member kicked");
                emb.AddField("User responsible", ke.UserResponsible.Mention);
                emb.AddField("Reason", ke.Reason ?? "No reason provided.");
            }

            emb.WithThumbnailUrl(e.Member.AvatarUrl);
            emb.AddField("Registration time", e.Member.CreationTimestamp.ToUtcTimestamp(), inline: true);
            if (!string.IsNullOrWhiteSpace(e.Member.Email))
                emb.AddField("Email", e.Member.Email);

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.GuildMemberUpdated)]
        public static async Task MemberUpdateEventHandlerAsync(TheGodfatherShard shard, GuildMemberUpdateEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Member, "Member updated", e.Member.ToString());
            emb.WithThumbnailUrl(e.Member.AvatarUrl);

            DiscordAuditLogEntry entry = null;
            if (e.RolesBefore.Count == e.RolesAfter.Count)
                entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.MemberUpdate);
            else
                entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.MemberRoleUpdate);
            if (!(entry is null) && entry is DiscordAuditLogMemberUpdateEntry mentry) {
                emb.AddField("User responsible", mentry.UserResponsible.Mention, inline: true);
                if (!(mentry.NicknameChange is null))
                    emb.AddField("Nickname change", $"{mentry.NicknameChange.Before} -> {mentry.NicknameChange.After}", inline: true);
                if (!(mentry.AddedRoles is null) && mentry.AddedRoles.Any())
                    emb.AddField("Added roles", string.Join(",", mentry.AddedRoles.Select(r => r.Name)), inline: true);
                if (!(mentry.RemovedRoles is null) && mentry.RemovedRoles.Any())
                    emb.AddField("Removed roles", string.Join(",", mentry.RemovedRoles.Select(r => r.Name)), inline: true);
                if (!string.IsNullOrWhiteSpace(mentry.Reason))
                    emb.AddField("Reason", mentry.Reason);
                emb.WithFooter(mentry.CreationTimestamp.ToUtcTimestamp(), mentry.UserResponsible.AvatarUrl);
            } else {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                emb.AddField("Name before", e.NicknameBefore ?? _unknown, inline: true);
                emb.AddField("Name after", e.NicknameAfter ?? _unknown, inline: true);
                emb.AddField("Roles before", e.RolesBefore?.Count.ToString() ?? _unknown, inline: true);
                emb.AddField("Roles after", e.RolesAfter?.Count.ToString() ?? _unknown, inline: true);
            }

            using (DatabaseContext db = shard.Database.CreateContext()) {
                if (!string.IsNullOrWhiteSpace(e.NicknameAfter) && db.ForbiddenNames.Any(n => n.GuildId == e.Guild.Id && n.Regex.IsMatch(e.NicknameAfter))) {
                    try {
                        await e.Member.ModifyAsync(m => {
                            m.Nickname = e.NicknameBefore;
                            m.AuditLogReason = "_gf: Forbidden name match";
                        });
                        emb.AddField("Additional actions taken", "Removed name due to a match with a forbidden name");
                        if (!e.Member.IsBot)
                            await e.Member.SendMessageAsync($"The nickname you tried to set in the guild {e.Guild.Name} is forbidden by the guild administrator. Please set a different name.");
                    } catch (UnauthorizedException) {
                        emb.AddField("Additional actions taken", "Matched forbidden name, but I failed to remove it. Check my permissions");
                    }
                }
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.PresenceUpdated)]
        public static async Task MemberPresenceUpdateEventHandlerAsync(TheGodfatherShard shard, PresenceUpdateEventArgs e)
        {
            if (e.User.IsBot)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Member, "User updated", e.User.ToString());
            emb.WithTitle("User Updated:");
            emb.WithDescription(e.UserAfter.ToString());
            if (e.UserAfter.Username != e.UserBefore.Username)
                emb.AddField("Changed username", $"{e.UserBefore.Username} to {e.UserAfter.Username}");
            if (e.UserAfter.Discriminator != e.UserBefore.Discriminator)
                emb.AddField("Changed discriminator", $"{e.UserBefore.Discriminator} to {e.UserAfter.Discriminator}");
            if (e.UserAfter.AvatarUrl != e.UserBefore.AvatarUrl)
                emb.AddField("Changed avatar", Formatter.MaskedUrl("Old Avatar (note: might 404 later)", new Uri(e.UserBefore.AvatarUrl)));
            emb.WithThumbnailUrl(e.UserAfter.AvatarUrl);

            if (!emb.Fields.Any())
                return;

            IEnumerable<DiscordGuild> guilds = TheGodfather.ActiveShards
                .SelectMany(s => s.Client?.Guilds)
                .Select(kvp => kvp.Value)
                ?? Enumerable.Empty<DiscordGuild>();
            foreach (DiscordGuild guild in guilds) {
                DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(guild);
                if (logchn is null)
                    continue;
                if (await e.UserAfter.IsMemberOfGuildAsync(guild))
                    await logchn.SendMessageAsync(embed: emb.Build());
            }
        }
    }
}
