using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.GuildMemberAdded)]
        public static async Task GuildMemberJoinEventHandlerAsync(TheGodfatherShard shard, GuildMemberAddEventArgs e)
        {
            if (e.Guild is null)
                return;
            
            LogExt.Debug(shard.Id, "Member added: {Member} {Guild}", e.Member, e.Guild);

            GuildConfigService gcs = shard.Services.GetRequiredService<GuildConfigService>();
            GuildConfig gcfg = await gcs.GetConfigAsync(e.Guild.Id);

            await Task.Delay(TimeSpan.FromSeconds(gcfg.AntiInstantLeaveSettings.Cooldown + 1));

            if (e.Member.Guild is null)     // User left in meantime
                return;

            // TODO move to service
            DiscordChannel? wchn = e.Guild.GetChannel(gcfg.WelcomeChannelId);
            if (wchn is { }) {
                string welcomeStr = string.IsNullOrWhiteSpace(gcfg.WelcomeMessage)
                    ? shard.Services.GetRequiredService<LocalizationService>().GetString(e.Guild.Id, "fmt-welcome", e.Guild.Name, e.Member.Mention)
                    : gcfg.WelcomeMessage.Replace("%user%", e.Member.Mention);
                await LoggingService.TryExecuteWithReportAsync(
                    shard, e.Guild, wchn.EmbedAsync(welcomeStr, Emojis.Wave), "rep-wchn-403", "rep-wchn-404",
                    code404action: () => gcs.ModifyConfigAsync(e.Guild.Id, cfg => { cfg.WelcomeChannelId = 0; })
                );
            }

            await shard.Services.GetRequiredService<AutoRoleService>().GrantRolesAsync(shard, e.Guild, e.Member);

            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;
            LocalizationService ls = shard.Services.GetRequiredService<LocalizationService>();

            emb.WithLocalizedTitle(DiscordEventType.GuildMemberAdded, "evt-gld-mem-add", e.Member);
            emb.WithThumbnail(e.Member.AvatarUrl);
            emb.AddLocalizedTitleField("str-regtime", ls.GetLocalizedTime(e.Guild.Id, e.Member.CreationTimestamp), inline: true);
            emb.AddLocalizedTitleField("str-ahash", e.Member.AvatarHash, inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-flags", e.Member.Flags.Humanize(), inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-locale", e.Member.Locale, inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-mfa", e.Member.MfaEnabled, inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-flags-oauth", e.Member.OAuthFlags.Humanize(), inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-premium-type", e.Member.PremiumType.Humanize(), inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-premium-since", ls.GetLocalizedTime(e.Guild.Id, e.Member.PremiumSince), inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-email", e.Member.Email, inline: true, unknown: false);

            // TODO move to service
            using (TheGodfatherDbContext db = shard.Database.CreateContext()) {
                ForbiddenName? fname = db.ForbiddenNames
                    .Where(n => n.GuildIdDb == (long)e.Guild.Id)
                    .AsEnumerable()
                    .FirstOrDefault(fn => fn.Regex.IsMatch(e.Member.DisplayName))
                    ;
                if (fname is { }) {
                    try {
                        await e.Member.ModifyAsync(m => {
                            m.Nickname = e.Member.Id.ToString();
                            m.AuditLogReason = ls.GetString(e.Guild.Id, "rsn-fname-match", fname.RegexString);
                        });
                        emb.AddLocalizedTitleField("str-act-taken", "act-fname-match");
                        if (!e.Member.IsBot)
                            await e.Member.SendMessageAsync(ls.GetString(null, "dm-fname-match", Formatter.Italic(e.Guild.Name)));
                    } catch (UnauthorizedException) {
                        emb.AddLocalizedField("str-err", "err-fname-match");
                    }
                }
            }

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildMemberAdded)]
        public static async Task GuildMemberJoinProtectionEventHandlerAsync(TheGodfatherShard shard, GuildMemberAddEventArgs e)
        {
            if (e.Guild is null || e.Member is null || e.Member.IsBot)
                return;
            
            GuildConfig gcfg = await shard.Services.GetService<GuildConfigService>().GetConfigAsync(e.Guild.Id);

            if (gcfg.AntifloodEnabled)
                await shard.Services.GetService<AntifloodService>().HandleMemberJoinAsync(e, gcfg.AntifloodSettings);

            if (gcfg.AntiInstantLeaveEnabled)
                await shard.Services.GetService<AntiInstantLeaveService>().HandleMemberJoinAsync(e, gcfg.AntiInstantLeaveSettings);
        }

        [AsyncEventListener(DiscordEventType.GuildMemberRemoved)]
        public static async Task GuildMemberRemoveEventHandlerAsync(TheGodfatherShard shard, GuildMemberRemoveEventArgs e)
        {
            if (e.Guild is null || e.Member is null || e.Member.IsBot)
                return;
            
            LogExt.Debug(shard.Id, "Member removed: {Member} {Guild}", e.Member, e.Guild);

            GuildConfigService gcs = shard.Services.GetRequiredService<GuildConfigService>();
            GuildConfig gcfg = await gcs.GetConfigAsync(e.Guild.Id);
            bool punished = false;

            if (gcfg.AntiInstantLeaveEnabled)
                punished = await shard.Services.GetService<AntiInstantLeaveService>().HandleMemberLeaveAsync(e, gcfg.AntiInstantLeaveSettings);

            if (!punished) {
                // TODO move to service
                DiscordChannel? lchn = e.Guild.GetChannel(gcfg.LeaveChannelId);
                if (lchn is { }) {
                    string leaveStr = string.IsNullOrWhiteSpace(gcfg.LeaveMessage)
                        ? shard.Services.GetRequiredService<LocalizationService>().GetString(e.Guild.Id, "fmt-leave", e.Member.Mention)
                        : gcfg.LeaveMessage.Replace("%user%", e.Member.Mention);
                    await LoggingService.TryExecuteWithReportAsync(
                        shard, e.Guild, lchn.EmbedAsync(leaveStr, Emojis.Wave), "rep-lchn-403", "rep-lchn-404",
                        code404action: () => gcs.ModifyConfigAsync(e.Guild.Id, cfg => { cfg.LeaveChannelId = 0; })
                    );
                }
            }

            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;
            LocalizationService ls = shard.Services.GetRequiredService<LocalizationService>();

            emb.WithLocalizedTitle(DiscordEventType.GuildMemberRemoved, "evt-gld-mem-del", e.Member);

            DiscordAuditLogKickEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogKickEntry>(AuditLogActionType.Kick);
            if (entry?.Target?.Id == e.Member.Id)
                emb.AddFieldsFromAuditLogEntry(entry, (emb, _) => emb.WithLocalizedTitle(DiscordEventType.GuildMemberRemoved, "evt-gld-kick"));
            emb.WithThumbnail(e.Member.AvatarUrl);
            emb.AddLocalizedTitleField("str-regtime", ls.GetLocalizedTime(e.Guild.Id, e.Member.CreationTimestamp), inline: true);
            emb.AddLocalizedTitleField("str-email", e.Member.Email);

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildMemberUpdated)]
        public static async Task GuildMemberUpdateEventHandlerAsync(TheGodfatherShard shard, GuildMemberUpdateEventArgs e)
        {
            if (e.Guild is null || e.Member is null || e.Member.IsBot)
                return;
            
            LogExt.Debug(shard.Id, "Member updated: {Member} {Guild}", e.Member, e.Guild);

            LocalizationService ls = shard.Services.GetRequiredService<LocalizationService>();

            bool renamed = false, failed = false;
            if (!string.IsNullOrWhiteSpace(e.NicknameAfter)) {
                // TODO move to service
                using TheGodfatherDbContext db = shard.Database.CreateContext();
                ForbiddenName? fname = db.ForbiddenNames
                    .Where(n => n.GuildIdDb == (long)e.Guild.Id)
                    .AsEnumerable()
                    .FirstOrDefault(fn => fn.Regex.IsMatch(e.Member.DisplayName))
                    ;
                if (fname is { }) {
                    try {
                        await e.Member.ModifyAsync(m => {
                            m.Nickname = e.Member.Id.ToString();
                            m.AuditLogReason = ls.GetString(e.Guild.Id, "rsn-fname-match", fname.RegexString);
                        });
                        renamed = true;
                        if (!e.Member.IsBot)
                            await e.Member.SendMessageAsync(ls.GetString(null, "dm-fname-match", Formatter.Italic(e.Guild.Name)));
                    } catch (UnauthorizedException) {
                        failed = true;
                    }
                }
            }

            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildMemberUpdated, "evt-gld-mem-upd", e.Member);
            emb.WithThumbnail(e.Member.AvatarUrl);

            DiscordAuditLogEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogEntry>();
            switch (entry) {
                case null:
                    emb.AddLocalizedPropertyChangeField("str-name", e.NicknameBefore, e.NicknameAfter);
                    if (!e.RolesBefore.SequenceEqual(e.RolesAfter)) {
                        string rolesBefore = e.RolesBefore.Select(r => r.Mention).Humanize(", ");
                        string rolesAfter = e.RolesAfter.Select(r => r.Mention).Humanize(", ");
                        string noneStr = ls.GetString(e.Guild.Id, "str-none");
                        emb.AddLocalizedTitleField("str-roles-bef", string.IsNullOrWhiteSpace(rolesBefore) ? noneStr : rolesBefore, inline: true);
                        emb.AddLocalizedTitleField("str-roles-aft", string.IsNullOrWhiteSpace(rolesAfter) ? noneStr : rolesAfter, inline: true);
                    }
                    break;
                case DiscordAuditLogMemberUpdateEntry uentry:
                    emb.AddFieldsFromAuditLogEntry(uentry, (emb, ent) => {
                        emb.AddLocalizedPropertyChangeField("str-name", ent.NicknameChange);
                        emb.AddLocalizedTitleField("str-roles-add", ent.AddedRoles?.Select(r => r.Mention).Humanize(", "), inline: true, unknown: false);
                        emb.AddLocalizedTitleField("str-roles-add", ent.RemovedRoles?.Select(r => r.Mention).Humanize(", "), unknown: false);
                    });
                    break;
                case DiscordAuditLogMemberMoveEntry mentry:
                    // TODO
                    emb.WithLocalizedTitle(DiscordEventType.GuildMemberUpdated, "evt-gld-mem-vc-mv", e.Member);
                    Log.Debug("{@Member}", e.Member);
                    emb.AddFieldsFromAuditLogEntry(mentry, (emb, ent) => {
                        emb.WithDescription(ent.Channel);
                        emb.AddLocalizedTitleField("str-move-count", ent.UserCount);
                    });
                    break;
                default:
                    break;
            }

            if (renamed)
                emb.AddLocalizedTitleField("str-act-taken", "act-fname-match");
            if (failed)
                emb.AddLocalizedField("str-err", "err-fname-match");

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildMembersChunked)]
        public static Task GuildMembersChunkedEventHandlerAsync(TheGodfatherShard shard, GuildMembersChunkEventArgs e)
        {
            LogExt.Debug(shard.Id, "Guild members chunked: [{Nonce}] {ChunkIndex}/{ChunkCount}", e.Nonce, e.ChunkIndex, e.ChunkCount);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.PresenceUpdated)]
        public static async Task MemberPresenceUpdateEventHandlerAsync(TheGodfatherShard shard, PresenceUpdateEventArgs e)
        {
            if (e.User.IsBot)
                return;
            
            LogExt.Debug(shard.Id, "Presence updated: {User}", e.User);

            GuildConfigService gcs = shard.Services.GetRequiredService<GuildConfigService>();
            LocalizationService ls = shard.Services.GetRequiredService<LocalizationService>();
            IEnumerable<DiscordGuild> guilds = TheGodfather.ActiveShards
                .SelectMany(s => s.Client?.Guilds)
                .Select(kvp => kvp.Value)
                ?? Enumerable.Empty<DiscordGuild>();
            foreach (DiscordGuild guild in guilds) {
                if (await e.UserAfter.IsMemberOfAsync(guild)) {
                    if (!LoggingService.IsLogEnabledForGuild(shard, guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                        return;

                    emb.WithLocalizedTitle(DiscordEventType.PresenceUpdated, "evt-presence-upd", e.User);
                    emb.WithThumbnail(e.UserAfter.AvatarUrl);

                    emb.AddLocalizedTitleField("str-flags", e.UserAfter.Flags);
                    emb.AddLocalizedPropertyChangeField("str-name", e.UserBefore.Username, e.UserAfter.Username);
                    emb.AddLocalizedPropertyChangeField("str-discriminator", e.UserBefore.Discriminator, e.UserAfter.Discriminator);
                    if (e.UserAfter.AvatarUrl != e.UserBefore.AvatarUrl)
                        emb.AddLocalizedTitleField("str-avatar", Formatter.MaskedUrl(ls.GetString(guild.Id, "str-avatar-old"), new Uri(e.UserBefore.AvatarUrl)));
                    emb.AddLocalizedPropertyChangeField("str-email", e.UserBefore.Email, e.UserAfter.Email);
                    emb.AddLocalizedPropertyChangeField("str-locale", e.UserBefore.Locale, e.UserAfter.Locale);
                    emb.AddLocalizedPropertyChangeField("str-mfa", e.UserBefore.MfaEnabled, e.UserAfter.MfaEnabled);
                    emb.AddLocalizedPropertyChangeField("str-flags-oauth", e.UserBefore.OAuthFlags, e.UserAfter.OAuthFlags);
                    emb.AddLocalizedPropertyChangeField("str-premium-type", e.UserBefore.PremiumType, e.UserAfter.PremiumType);
                    emb.AddLocalizedPropertyChangeField("str-verified", e.UserBefore.Verified, e.UserAfter.Verified);
                    
                    // TODO improve
                    emb.AddLocalizedTitleField("str-activity", e.Activity.ToDetailedString());

                    await logService.LogAsync(guild, emb);
                }
            }
        }
    }
}
