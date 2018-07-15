#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Extensions;
using TheGodfather.Services.Database.GuildConfig;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.GuildMemberAdded)]
        public static async Task MemberJoinEventHandlerAsync(TheGodfatherShard shard, GuildMemberAddEventArgs e)
        {
            shard.Log(LogLevel.Info, $"| Member joined: {e.Member.ToString()}\n{e.Guild.ToString()}");

            DiscordChannel wchn = await shard.DatabaseService.GetWelcomeChannelAsync(e.Guild);
            if (wchn != null) {
                string msg = await shard.DatabaseService.GetWelcomeMessageAsync(e.Guild.Id);
                if (string.IsNullOrWhiteSpace(msg))
                    await wchn.InformSuccessAsync($"Welcome to {Formatter.Bold(e.Guild.Name)}, {e.Member.Mention}!", StaticDiscordEmoji.Wave);
                else
                    await wchn.InformSuccessAsync(msg.Replace("%user%", e.Member.Mention), StaticDiscordEmoji.Wave);
            }

            try {
                IReadOnlyList<ulong> rids = await shard.DatabaseService.GetAutomaticRolesForGuildAsync(e.Guild.Id)
                    .ConfigureAwait(false);
                foreach (ulong rid in rids) {
                    try {
                        DiscordRole role = e.Guild.GetRole(rid);
                        if (role != null)
                            await e.Member.GrantRoleAsync(role);
                        else
                            await shard.DatabaseService.RemoveAutomaticRoleAsync(e.Guild.Id, rid);
                    } catch (Exception exc) {
                        shard.Log(LogLevel.Debug,
                            $"| Failed to assign an automatic role to a new member!\n" +
                            $"| {e.Guild.ToString()}\n" +
                            $"| Exception: {exc.GetType()}\n" +
                            $"| Message: {exc.Message}"
                        );
                    }
                }
            } catch (Exception exc) {
                shard.SharedData.LogProvider.LogException(LogLevel.Debug, exc);
            }

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn == null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Member, "Member joined", e.Member.ToString());
            emb.WithThumbnailUrl(e.Member.AvatarUrl);
            emb.AddField("Registration time", BuildUTCString(e.Member.CreationTimestamp), inline: true);
            if (!string.IsNullOrWhiteSpace(e.Member.Email))
                emb.AddField("Email", e.Member.Email);

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.GuildMemberRemoved)]
        public static async Task MemberRemoveEventHandlerAsync(TheGodfatherShard shard, GuildMemberRemoveEventArgs e)
        {
            if (e.Member.Id == e.Client.CurrentUser.Id)
                return;

            shard.Log(LogLevel.Info, $"| Member left: {e.Member.ToString()}\n{e.Guild.ToString()}");

            DiscordChannel lchn = await shard.DatabaseService.GetLeaveChannelAsync(e.Guild);
            if (lchn != null) {
                string msg = await shard.DatabaseService.GetLeaveMessageForGuildAsync(e.Guild.Id);
                if (string.IsNullOrWhiteSpace(msg))
                    await lchn.InformSuccessAsync($"{Formatter.Bold(e.Member?.Username ?? _unknown)} left the server! Bye!", StaticDiscordEmoji.Wave);
                else
                    await lchn.InformSuccessAsync(msg.Replace("%user%", e.Member?.Username ?? _unknown), StaticDiscordEmoji.Wave);
            }

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn == null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Member, "Member left", e.Member.ToString());
            emb.WithThumbnailUrl(e.Member.AvatarUrl);
            emb.AddField("Registration time", BuildUTCString(e.Member.CreationTimestamp), inline: true);
            if (!string.IsNullOrWhiteSpace(e.Member.Email))
                emb.AddField("Email", e.Member.Email);

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.GuildMemberUpdated)]
        public static async Task MemberUpdateEventHandlerAsync(TheGodfatherShard shard, GuildMemberUpdateEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn == null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Member, "Member updated", e.Member.ToString());
            emb.WithThumbnailUrl(e.Member.AvatarUrl);
            
            DiscordAuditLogEntry entry = null;
            if (e.RolesBefore.Count == e.RolesAfter.Count)
                entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.MemberUpdate);
            else
                entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.MemberRoleUpdate);
            if (entry != null && entry is DiscordAuditLogMemberUpdateEntry mentry) {
                emb.AddField("User responsible", mentry.UserResponsible.Mention, inline: true);
                if (mentry.NicknameChange != null)
                    emb.AddField("Nickname change", $"{mentry.NicknameChange.Before} -> {mentry.NicknameChange.After}", inline: true);
                if (mentry.AddedRoles != null && mentry.AddedRoles.Any())
                    emb.AddField("Added roles", string.Join(",", mentry.AddedRoles.Select(r => r.Name)), inline: true);
                if (mentry.RemovedRoles != null && mentry.RemovedRoles.Any())
                    emb.AddField("Removed roles", string.Join(",", mentry.RemovedRoles.Select(r => r.Name)), inline: true);
                if (!string.IsNullOrWhiteSpace(mentry.Reason))
                    emb.AddField("Reason", mentry.Reason);
                emb.WithFooter(BuildUTCString(mentry.CreationTimestamp), mentry.UserResponsible.AvatarUrl);
            } else {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                emb.AddField("Name before", e.NicknameBefore ?? _unknown, inline: true);
                emb.AddField("Name after", e.NicknameAfter ?? _unknown, inline: true);
                emb.AddField("Roles before", e.RolesBefore?.Count.ToString() ?? _unknown, inline: true);
                emb.AddField("Roles after", e.RolesAfter?.Count.ToString() ?? _unknown, inline: true);
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }
    }
}
