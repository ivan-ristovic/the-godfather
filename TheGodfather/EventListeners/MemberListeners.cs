#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.EventListeners
{
    internal static class MemberListeners
    {
        [AsyncExecuter(EventTypes.GuildMemberAdded)]
        public static async Task Client_GuildMemberAdded(TheGodfatherShard shard, GuildMemberAddEventArgs e)
        {
            if (!TheGodfather.Listening)
                return;

            shard.Log(LogLevel.Info, $"Member joined: {e.Member.ToString()}<br>{e.Guild.ToString()}");

            var wchn = await shard.Database.GetWelcomeChannelAsync(e.Guild)
                .ConfigureAwait(false);
            if (wchn != null) {
                var msg = await shard.Database.GetWelcomeMessageAsync(e.Guild.Id)
                    .ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(msg)) {
                    await wchn.SendIconEmbedAsync($"Welcome to {Formatter.Bold(e.Guild.Name)}, {e.Member.Mention}!", DiscordEmoji.FromName(shard.Client, ":wave:"))
                        .ConfigureAwait(false);
                } else {
                    await wchn.SendIconEmbedAsync(msg.Replace("%user%", e.Member.Mention), DiscordEmoji.FromName(shard.Client, ":wave:"))
                        .ConfigureAwait(false);
                }
            }

            try {
                var rids = await shard.Database.GetAutomaticRolesForGuildAsync(e.Guild.Id)
                    .ConfigureAwait(false);
                foreach (var rid in rids) {
                    try {
                        var role = e.Guild.GetRole(rid);
                        if (role == null) {
                            await shard.Database.RemoveAutomaticRoleAsync(e.Guild.Id, rid)
                                .ConfigureAwait(false);
                        } else {
                            await e.Member.GrantRoleAsync(role)
                                .ConfigureAwait(false);
                        }
                    } catch (Exception exc) {
                        shard.Log(LogLevel.Debug,
                            $"Failed to assign an automatic role to a new member!<br>" +
                            $"{e.Guild.ToString()}<br>" +
                            $"Exception: {exc.GetType()}<br>" +
                            $"Message: {exc.Message}"
                        );
                    }
                }
            } catch (Exception exc) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, exc);
            }

            var logchn = shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Member joined",
                    Description = e.Member.ToString(),
                    Color = DiscordColor.White,
                    ThumbnailUrl = e.Member.AvatarUrl
                };
                emb.AddField("Registered at", $"{e.Member.CreationTimestamp.ToUniversalTime().ToString()} UTC", inline: true);
                if (!string.IsNullOrWhiteSpace(e.Member.Email))
                    emb.AddField("Email", e.Member.Email);

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.GuildMemberRemoved)]
        public static async Task Client_GuildMemberRemoved(TheGodfatherShard shard, GuildMemberRemoveEventArgs e)
        {
            if (!TheGodfather.Listening || e.Member.Id == e.Client.CurrentUser.Id)
                return;

            shard.Log(LogLevel.Info, $"Member left: {e.Member.ToString()}<br>{e.Guild.ToString()}");

            var lchn = await shard.Database.GetLeaveChannelAsync(e.Guild)
                .ConfigureAwait(false);
            if (lchn != null) {
                var msg = await shard.Database.GetLeaveMessageAsync(e.Guild.Id)
                    .ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(msg)) {
                    await lchn.SendIconEmbedAsync($"{Formatter.Bold(e.Member?.Username ?? "<unknown user>")} left the server! Bye!", StaticDiscordEmoji.Wave)
                        .ConfigureAwait(false);
                } else {
                    await lchn.SendIconEmbedAsync(msg.Replace("%user%", e.Member?.Username ?? "<unknown user>"), DiscordEmoji.FromName(shard.Client, ":wave:"))
                        .ConfigureAwait(false);
                }
            }

            var logchn = shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Member left",
                    Description = e.Member.ToString(),
                    Color = DiscordColor.White,
                    ThumbnailUrl = e.Member.AvatarUrl
                };
                emb.AddField("Registered at", $"{e.Member.CreationTimestamp.ToUniversalTime().ToString()} UTC", inline: true);
                if (!string.IsNullOrWhiteSpace(e.Member.Email))
                    emb.AddField("Email", e.Member.Email);

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.GuildMemberUpdated)]
        public static async Task Client_GuildMemberUpdated(TheGodfatherShard shard, GuildMemberUpdateEventArgs e)
        {
            var logchn = shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Member updated",
                    Description = e.Member.ToString(),
                    Color = DiscordColor.White,
                    ThumbnailUrl = e.Member.AvatarUrl
                };

                DiscordAuditLogEntry entry = null;
                if (e.RolesBefore.Count == e.RolesAfter.Count)
                    entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.MemberUpdate).ConfigureAwait(false);
                else
                    entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.MemberRoleUpdate).ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogMemberUpdateEntry mentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Name before", e.NicknameBefore ?? "<unknown>", inline: true);
                    emb.AddField("Name after", e.NicknameAfter ?? "<unknown>", inline: true);
                    emb.AddField("Roles before", e.RolesBefore?.Count.ToString() ?? "<unknown>", inline: true);
                    emb.AddField("Roles after", e.RolesAfter?.Count.ToString() ?? "<unknown>", inline: true);
                } else {
                    emb.AddField("User responsible", mentry.UserResponsible.Mention, inline: true);
                    if (mentry.NicknameChange != null)
                        emb.AddField("Nickname change", $"{mentry.NicknameChange.Before} -> {mentry.NicknameChange.After}", inline: true);
                    if (mentry.AddedRoles != null && mentry.AddedRoles.Any())
                        emb.AddField("Added roles", string.Join(",", mentry.AddedRoles.Select(r => r.Name)), inline: true);
                    if (mentry.RemovedRoles != null && mentry.RemovedRoles.Any())
                        emb.AddField("Removed roles", string.Join(",", mentry.RemovedRoles.Select(r => r.Name)), inline: true);
                    if (!string.IsNullOrWhiteSpace(mentry.Reason))
                        emb.AddField("Reason", mentry.Reason);
                    emb.WithFooter($"At {mentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", mentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }
    }
}
