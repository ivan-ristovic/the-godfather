using System;
using System.Collections.Generic;
using System.Linq;
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
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.GuildMemberAdded)]
        public static async Task GuildMemberJoinEventHandlerAsync(TheGodfatherBot bot, GuildMemberAddEventArgs e)
        {
            if (e.Guild is null)
                return;

            LogExt.Debug(bot.GetId(e.Guild.Id), "Member added: {Member} {Guild}", e.Member, e.Guild);

            GuildConfigService gcs = bot.Services.GetRequiredService<GuildConfigService>();
            GuildConfig gcfg = await gcs.GetConfigAsync(e.Guild.Id);

            await Task.Delay(TimeSpan.FromSeconds(gcfg.AntiInstantLeaveSettings.Cooldown + 1));

            if (e.Member.Guild is null)     // User left in meantime
                return;

            // TODO move to service
            DiscordChannel? wchn = e.Guild.GetChannel(gcfg.WelcomeChannelId);
            if (wchn is { }) {
                string welcomeStr = string.IsNullOrWhiteSpace(gcfg.WelcomeMessage)
                    ? bot.Services.GetRequiredService<LocalizationService>().GetString(e.Guild.Id, "fmt-welcome", e.Guild.Name, e.Member.Mention)
                    : gcfg.WelcomeMessage.Replace("%user%", e.Member.Mention);
                await LoggingService.TryExecuteWithReportAsync(
                    bot, e.Guild, wchn.EmbedAsync(welcomeStr, Emojis.Wave), "rep-wchn-403", "rep-wchn-404",
                    code404action: () => gcs.ModifyConfigAsync(e.Guild.Id, cfg => cfg.WelcomeChannelId = default)
                );
            }

            await bot.Services.GetRequiredService<AutoRoleService>().GrantRolesAsync(bot, e.Guild, e.Member);

            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;
            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();

            emb.WithLocalizedTitle(DiscordEventType.GuildMemberAdded, "evt-gld-mem-add", e.Member);
            emb.WithThumbnail(e.Member.AvatarUrl);
            emb.AddLocalizedTimestampField("str-regtime", e.Member.CreationTimestamp, inline: true);
            emb.AddLocalizedTitleField("str-ahash", e.Member.AvatarHash, inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-flags", e.Member.Flags.Humanize(), inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-locale", e.Member.Locale, inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-mfa", e.Member.MfaEnabled, inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-flags-oauth", e.Member.OAuthFlags?.Humanize(LetterCasing.Sentence), inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-verified", e.Member.Verified, inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-premium-type", e.Member.PremiumType?.Humanize(), inline: true, unknown: false);
            emb.AddLocalizedTimestampField("str-premium-since", e.Member.PremiumSince, inline: true);
            emb.AddLocalizedTitleField("str-email", e.Member.Email, inline: true, unknown: false);

            // TODO move to service
            using (TheGodfatherDbContext db = bot.Database.CreateContext()) {
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
        public static async Task GuildMemberJoinProtectionEventHandlerAsync(TheGodfatherBot shard, GuildMemberAddEventArgs e)
        {
            if (e.Guild is null || e.Member is null || e.Member.IsBot)
                return;

            GuildConfig gcfg = await shard.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(e.Guild.Id);

            if (gcfg.AntifloodEnabled)
                await shard.Services.GetRequiredService<AntifloodService>().HandleMemberJoinAsync(e, gcfg.AntifloodSettings);

            if (gcfg.AntiInstantLeaveEnabled)
                await shard.Services.GetRequiredService<AntiInstantLeaveService>().HandleMemberJoinAsync(e, gcfg.AntiInstantLeaveSettings);
        }

        [AsyncEventListener(DiscordEventType.GuildMemberRemoved)]
        public static async Task GuildMemberRemoveEventHandlerAsync(TheGodfatherBot bot, GuildMemberRemoveEventArgs e)
        {
            if (e.Guild is null || e.Member is null || e.Member.IsBot)
                return;

            LogExt.Debug(bot.GetId(e.Guild.Id), "Member removed: {Member} {Guild}", e.Member, e.Guild);

            GuildConfigService gcs = bot.Services.GetRequiredService<GuildConfigService>();
            GuildConfig gcfg = await gcs.GetConfigAsync(e.Guild.Id);
            bool punished = false;

            if (gcfg.AntiInstantLeaveEnabled)
                punished = await bot.Services.GetRequiredService<AntiInstantLeaveService>().HandleMemberLeaveAsync(e);

            if (!punished) {
                // TODO move to service
                DiscordChannel? lchn = e.Guild.GetChannel(gcfg.LeaveChannelId);
                if (lchn is { }) {
                    string leaveStr = string.IsNullOrWhiteSpace(gcfg.LeaveMessage)
                        ? bot.Services.GetRequiredService<LocalizationService>().GetString(e.Guild.Id, "fmt-leave", e.Member.Mention)
                        : gcfg.LeaveMessage.Replace("%user%", e.Member.Mention);
                    await LoggingService.TryExecuteWithReportAsync(
                        bot, e.Guild, lchn.EmbedAsync(leaveStr, Emojis.Wave), "rep-lchn-403", "rep-lchn-404",
                        code404action: () => gcs.ModifyConfigAsync(e.Guild.Id, cfg => cfg.LeaveChannelId = default)
                    );
                }
            }

            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;
            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();

            emb.WithLocalizedTitle(DiscordEventType.GuildMemberRemoved, "evt-gld-mem-del", e.Member);

            DiscordAuditLogKickEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogKickEntry>(AuditLogActionType.Kick);
            if (entry?.Target?.Id == e.Member.Id)
                emb.AddFieldsFromAuditLogEntry(entry, (emb, _) => emb.WithLocalizedTitle(DiscordEventType.GuildMemberRemoved, "evt-gld-kick"));
            emb.WithThumbnail(e.Member.AvatarUrl);
            emb.AddLocalizedTitleField("str-regtime", ls.GetLocalizedTimeString(e.Guild.Id, e.Member.CreationTimestamp), inline: true);
            emb.AddLocalizedTitleField("str-email", e.Member.Email);

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildMemberUpdated)]
        public static async Task GuildMemberUpdateEventHandlerAsync(TheGodfatherBot bot, GuildMemberUpdateEventArgs e)
        {
            if (e.Guild is null || e.Member is null || e.Member.IsBot)
                return;

            LogExt.Debug(bot.GetId(e.Guild.Id), "Member updated: {Member} {Guild}", e.Member, e.Guild);

            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();

            bool renamed = false, failed = false;
            if (!string.IsNullOrWhiteSpace(e.NicknameAfter) && e.NicknameAfter != e.Member.Id.ToString()) {
                ForbiddenNamesService fns = bot.Services.GetRequiredService<ForbiddenNamesService>();
                if (fns.IsNameForbidden(e.Guild.Id, e.NicknameAfter, out ForbiddenName? fname) && fname is { }) {
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

            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildMemberUpdated, "evt-gld-mem-upd", e.Member);
            emb.WithThumbnail(e.Member.AvatarUrl);

            DiscordAuditLogEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogEntry>();
            switch (entry) {
                case null:
                    emb.AddLocalizedPropertyChangeField("str-name", e.NicknameBefore, e.NicknameAfter);
                    emb.WithTitle(e.Member.ToDiscriminatorString());
                    emb.WithThumbnail(e.Member.AvatarUrl);
                    emb.AddLocalizedTimestampField("str-regtime", e.Member.CreationTimestamp, inline: true);
                    emb.AddLocalizedTimestampField("str-joined-at", e.Member.JoinedAt, inline: true);
                    emb.AddLocalizedTitleField("str-id", e.Member.Id, inline: true);
                    emb.AddLocalizedTitleField("str-hierarchy", e.Member.Hierarchy, inline: true);
                    emb.AddLocalizedTitleField("str-ahash", e.Member.AvatarHash, inline: true, unknown: false);
                    emb.AddLocalizedTitleField("str-verified", e.Member.Verified, inline: true, unknown: false);
                    emb.AddLocalizedTitleField("str-flags", e.Member.Flags?.Humanize(), inline: true, unknown: false);
                    emb.AddLocalizedTitleField("str-locale", e.Member.Locale, inline: true, unknown: false);
                    emb.AddLocalizedTitleField("str-mfa", e.Member.MfaEnabled, inline: true, unknown: false);
                    emb.AddLocalizedTitleField("str-flags-oauth", e.Member.OAuthFlags?.Humanize(), inline: true, unknown: false);
                    emb.AddLocalizedTitleField("str-premium-type", e.Member.PremiumType?.Humanize(), inline: true, unknown: false);
                    emb.AddLocalizedTimestampField("str-premium-since", e.Member.PremiumSince, inline: true);
                    emb.AddLocalizedTitleField("str-email", e.Member.Email, inline: true, unknown: false);
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
                        emb.AddLocalizedTitleField("str-roles-rem", ent.RemovedRoles?.Select(r => r.Mention).Humanize(", "), inline: true, unknown: false);
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
        public static Task GuildMembersChunkedEventHandlerAsync(TheGodfatherBot bot, GuildMembersChunkEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Guild members chunked: [{Nonce}] {ChunkIndex}/{ChunkCount}", e.Nonce, e.ChunkIndex, e.ChunkCount);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.PresenceUpdated)]
        public static async Task MemberPresenceUpdateEventHandlerAsync(TheGodfatherBot bot, PresenceUpdateEventArgs e)
        {
            if (e.User.IsBot)
                return;

            Log.Debug("Presence updated: {User}", e.User);
            if (e.UserBefore.Presence?.Status != e.UserAfter.Presence?.Status || e.UserBefore.Presence?.Activity != e.UserAfter.Presence?.Activity)
                return;

            GuildConfigService gcs = bot.Services.GetRequiredService<GuildConfigService>();
            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
            IEnumerable<DiscordGuild> guilds = bot.Client.ShardClients.Values
                .SelectMany(s => s.Guilds)
                .Select(kvp => kvp.Value)
                ?? Enumerable.Empty<DiscordGuild>();
            foreach (DiscordGuild guild in guilds) {
                if (await e.UserAfter.IsMemberOfAsync(guild)) {
                    if (!LoggingService.IsLogEnabledForGuild(bot, guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
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
