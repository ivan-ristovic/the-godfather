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

            _ = LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb);
            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();

            if (e.Member.IsBot) {
                LogExt.Debug(bot.GetId(e.Guild.Id), "Bot added to guild: {Member} {Guild}", e.Member, e.Guild);
                emb.WithLocalizedTitle(DiscordEventType.GuildMemberAdded, "evt-gld-bot-add", e.Member);
                AddMemberInfoToEmbed(emb, e.Member);
                await logService.LogAsync(e.Guild, emb);
                return;
            }

            LogExt.Debug(bot.GetId(e.Guild.Id), "Member added: {Member} {Guild}", e.Member, e.Guild);
            emb.WithLocalizedTitle(DiscordEventType.GuildMemberAdded, "evt-gld-mem-add", e.Member);

            GuildConfigService gcs = bot.Services.GetRequiredService<GuildConfigService>();
            GuildConfig gcfg = await gcs.GetConfigAsync(e.Guild.Id);

            await Task.Delay(TimeSpan.FromSeconds(gcfg.AntiInstantLeaveSettings.Cooldown + 1));

            if (e.Member.Guild is null)     // User left in meantime
                return;

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

            AddMemberInfoToEmbed(emb, e.Member);

            if (bot.Services.GetRequiredService<ForbiddenNamesService>().IsNameForbidden(e.Guild.Id, e.Member.DisplayName, out ForbiddenName? fname)) {
                if ((await bot.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(e.Guild.Id)).ActionHistoryEnabled) {
                    LogExt.Debug(bot.GetId(e.Guild.Id), "Adding forbidden name entry to action history: {Member}, {Guild}", e.Member, e.Guild);
                    await bot.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
                        Action = ActionHistoryEntry.ActionType.ForbiddenName,
                        GuildId = e.Guild.Id,
                        Notes = ls.GetString(e.Guild.Id, "rsn-fname-match", fname?.RegexString ?? "?"),
                        Time = DateTimeOffset.Now,
                        UserId = e.Member.Id,
                    });
                }
                try {
                    await e.Member.ModifyAsync(m => {
                        m.Nickname = e.Member.Id.ToString();
                        m.AuditLogReason = ls.GetString(e.Guild.Id, "rsn-fname-match", fname?.RegexString ?? "?");
                    });
                    emb.AddLocalizedTitleField("str-act-taken", "act-fname-match");
                    if (!e.Member.IsBot)
                        await e.Member.SendMessageAsync(ls.GetString(null, "dm-fname-match", Formatter.Italic(e.Guild.Name)));
                } catch (UnauthorizedException) {
                    emb.AddLocalizedField("str-err", "err-fname-match");
                }
            }

            if (gcfg.ActionHistoryEnabled) {
                IReadOnlyList<ActionHistoryEntry> history = await bot.Services.GetRequiredService<ActionHistoryService>().GetAllAsync((e.Guild.Id, e.Member.Id));
                if (history.Any()) {
                    IEnumerable<ActionHistoryEntry> orderedHistory = history
                        .OrderByDescending(e => e.Action)
                        .ThenByDescending(e => e.Time)
                        .Take(5)
                        ;
                    foreach (ActionHistoryEntry ahe in orderedHistory) {
                        string title = ahe.Action.ToLocalizedKey();
                        string content = ls.GetString(e.Guild.Id, "fmt-ah-emb",
                            ls.GetLocalizedTimeString(e.Guild.Id, ahe.Time),
                            ahe.Notes
                        );
                        emb.AddLocalizedTitleField(title, content);
                    }
                }
            }

            await logService.LogAsync(e.Guild, emb);


            static LocalizedEmbedBuilder AddMemberInfoToEmbed(LocalizedEmbedBuilder emb, DiscordMember member)
            {
                emb.WithThumbnail(member.AvatarUrl);
                emb.AddLocalizedTimestampField("str-regtime", member.CreationTimestamp, inline: true);
                emb.AddLocalizedTitleField("str-ahash", member.AvatarHash, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-flags", member.Flags?.Humanize(), inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-locale", member.Locale, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-mfa", member.MfaEnabled, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-flags-oauth", member.OAuthFlags?.Humanize(LetterCasing.Sentence), inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-verified", member.Verified, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-premium-type", member.PremiumType?.Humanize(), inline: true, unknown: false);
                emb.AddLocalizedTimestampField("str-premium-since", member.PremiumSince, inline: true);
                emb.AddLocalizedTitleField("str-email", member.Email, inline: true, unknown: false);
                return emb;
            }
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

            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb) && !gcfg.ActionHistoryEnabled)
                return;

            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();

            emb.WithLocalizedTitle(DiscordEventType.GuildMemberRemoved, "evt-gld-mem-left", e.Member);

            DiscordAuditLogKickEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogKickEntry>(AuditLogActionType.Kick);
            if (entry?.Target?.Id == e.Member.Id) {
                emb.AddFieldsFromAuditLogEntry(entry, (emb, _) => emb.WithLocalizedTitle(DiscordEventType.GuildMemberRemoved, "evt-gld-kick"));
                if ((await bot.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(e.Guild.Id)).ActionHistoryEnabled) {
                    LogExt.Debug(bot.GetId(e.Guild.Id), "Adding kick entry to action history: {Member}, {Guild}", e.Member, e.Guild);
                    await bot.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
                        Action = ActionHistoryEntry.ActionType.Kick,
                        GuildId = e.Guild.Id,
                        Notes = ls.GetString(e.Guild.Id, "fmt-ah", entry.UserResponsible.Mention, entry.Reason),
                        Time = DateTimeOffset.Now,
                        UserId = e.Member.Id,
                    });
                }
            }
            emb.WithThumbnail(e.Member.AvatarUrl);
            emb.AddLocalizedTitleField("str-regtime", ls.GetLocalizedTimeString(e.Guild.Id, e.Member.CreationTimestamp), inline: true);
            emb.AddLocalizedTitleField("str-email", e.Member.Email, unknown: false);

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

                    if ((await bot.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(e.Guild.Id)).ActionHistoryEnabled) {
                        LogExt.Debug(bot.GetId(e.Guild.Id), "Adding forbidden name entry to action history: {Member}, {Guild}", e.Member, e.Guild);
                        await bot.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
                            Action = ActionHistoryEntry.ActionType.ForbiddenName,
                            GuildId = e.Guild.Id,
                            Notes = ls.GetString(e.Guild.Id, "rsn-fname-match", fname.RegexString),
                            Time = DateTimeOffset.Now,
                            UserId = e.Member.Id,
                        });
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
                    if (e.NicknameBefore != e.NicknameAfter)
                        AddNicknameChangeField(emb, e.NicknameBefore, e.NicknameAfter);
                    // TODO add pending membership screening
                    // TODO add nitro notifications
                    if (!e.RolesBefore.SequenceEqual(e.RolesAfter)) {   // FIXME order shouldn't matter
                        string rolesBefore = e.RolesBefore.Where(r => r.Id != e.Guild.Id).Select(r => r.Mention).Humanize(", ");
                        string rolesAfter = e.RolesAfter.Where(r => r.Id != e.Guild.Id).Select(r => r.Mention).Humanize(", ");
                        string noneStr = ls.GetString(e.Guild.Id, "str-none");
                        emb.AddLocalizedTitleField("str-roles-bef", string.IsNullOrWhiteSpace(rolesBefore) ? noneStr : rolesBefore, inline: true);
                        emb.AddLocalizedTitleField("str-roles-aft", string.IsNullOrWhiteSpace(rolesAfter) ? noneStr : rolesAfter, inline: true);
                    }
                    break;
                case DiscordAuditLogMemberUpdateEntry uentry:
                    emb.AddFieldsFromAuditLogEntry(uentry, (emb, ent) => {
                        if (ent.NicknameChange is { })
                            AddNicknameChangeField(emb, ent.NicknameChange.Before, ent.NicknameChange.After);
                        emb.AddLocalizedTitleField("str-roles-add", ent.AddedRoles?.Select(r => r.Mention).Humanize(", "), inline: true, unknown: false);
                        emb.AddLocalizedTitleField("str-roles-rem", ent.RemovedRoles?.Select(r => r.Mention).Humanize(", "), inline: true, unknown: false);
                    });
                    break;
                case DiscordAuditLogMemberMoveEntry mentry:
                    // TODO
                    emb.WithLocalizedTitle(DiscordEventType.GuildMemberUpdated, "evt-gld-mem-vc-mv", e.Member);
                    Log.Debug("{Member}", e.Member);
                    emb.AddFieldsFromAuditLogEntry(mentry, (emb, ent) => {
                        emb.WithDescription(ent.Channel);
                        emb.AddLocalizedTitleField("str-move-count", ent.UserCount);
                    });
                    break;
            }

            if (renamed)
                emb.AddLocalizedTitleField("str-act-taken", "act-fname-match");
            if (failed)
                emb.AddLocalizedField("str-err", "err-fname-match");

            if (emb.FieldCount > 0)
                await logService.LogAsync(e.Guild, emb);


            static void AddNicknameChangeField(LocalizedEmbedBuilder emb, string? nameBefore, string? nameAfter)
            {
                if (string.IsNullOrWhiteSpace(nameAfter))
                    emb.AddLocalizedTitleField("evt-nick-clear", string.IsNullOrWhiteSpace(nameBefore) ? null: Formatter.InlineCode(nameBefore));
                else
                    emb.AddLocalizedPropertyChangeField("str-name", nameBefore, nameAfter);
            }
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
            if (e.User is null || e.User.IsBot)
                return;

            Log.Debug("Presence updated: {User}", e.User);
            if (!IsUpdated(e.UserBefore, e.UserAfter))
                return;

            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
            IEnumerable<DiscordGuild> guilds = bot.Client.ShardClients.Values
                .SelectMany(s => s.Guilds)
                .Select(kvp => kvp.Value)
                ?? Enumerable.Empty<DiscordGuild>();
            Log.Debug("Logging user update: {User}", e.User);
            foreach (DiscordGuild guild in guilds) {
                if (!await e.UserAfter.IsMemberOfAsync(guild))
                    continue;

                if (!LoggingService.IsLogEnabledForGuild(bot, guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                    continue;

                emb.WithLocalizedTitle(DiscordEventType.GuildMemberUpdated, "evt-gld-mem-upd", e.User);
                emb.WithThumbnail(e.UserAfter.AvatarUrl);

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
                // emb.AddLocalizedTitleField("str-activity", e.Activity.ToDetailedString());

                if (emb.FieldCount == 0)
                    return;

                await logService.LogAsync(guild, emb);
            }


            static bool IsUpdated(DiscordUser? before, DiscordUser? after)
            {
                if (before is null || after is null)
                    return false;
                return !Equals(before?.Username, after?.Username)
                    || !Equals(before?.Discriminator, after?.Discriminator)
                    || !Equals(before?.AvatarUrl, after?.AvatarUrl)
                    || !Equals(before?.Email, after?.Email)
                    || !Equals(before?.MfaEnabled, after?.MfaEnabled)
                    || !Equals(before?.OAuthFlags, after?.OAuthFlags)
                    || !Equals(before?.PremiumType, after?.PremiumType)
                    || !Equals(before?.Verified, after?.Verified)
                    ;
            }
        }
    }
}
