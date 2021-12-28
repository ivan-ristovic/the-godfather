using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.EventListeners;

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
            emb.WithLocalizedTitle(DiscordEventType.GuildMemberAdded, TranslationKey.evt_gld_bot_add, e.Member);
            AddMemberInfoToEmbed(emb, e.Member);
            await logService.LogAsync(e.Guild, emb);
            return;
        }

        LogExt.Debug(bot.GetId(e.Guild.Id), "Member added: {Member} {Guild}", e.Member, e.Guild);
        emb.WithLocalizedTitle(DiscordEventType.GuildMemberAdded, TranslationKey.evt_gld_mem_add, e.Member);

        GuildConfigService gcs = bot.Services.GetRequiredService<GuildConfigService>();
        GuildConfig gcfg = await gcs.GetConfigAsync(e.Guild.Id);

        await Task.Delay(TimeSpan.FromSeconds(gcfg.AntiInstantLeaveSettings.Cooldown + 1));

        DiscordMember? member = await e.Guild.GetMemberAsync(e.Member.Id);
        if (member is null)     // User left/punished in meantime
            return;

        DiscordChannel? wchn = e.Guild.GetChannel(gcfg.WelcomeChannelId);
        if (wchn is { }) {
            string welcomeStr = string.IsNullOrWhiteSpace(gcfg.WelcomeMessage)
                ? bot.Services.GetRequiredService<LocalizationService>().GetString(e.Guild.Id, TranslationKey.fmt_welcome(e.Guild.Name, e.Member.Mention))
                : gcfg.WelcomeMessage.Replace("%user%", e.Member.Mention);
            await LoggingService.TryExecuteWithReportAsync(
                bot, e.Guild, wchn.EmbedAsync(welcomeStr, Emojis.Wave), TranslationKey.rep_wchn_403, TranslationKey.rep_wchn_404,
                code404action: () => gcs.ModifyConfigAsync(e.Guild.Id, cfg => cfg.WelcomeChannelId = default)
            );
        }

        await bot.Services.GetRequiredService<AutoRoleService>().GrantRolesAsync(bot, e.Guild, e.Member);

        AddMemberInfoToEmbed(emb, e.Member);

        ForbiddenNamesService fns = bot.Services.GetRequiredService<ForbiddenNamesService>();
        if (fns.IsNameForbidden(e.Guild.Id, e.Member.DisplayName, out ForbiddenName? fname)) {
            if ((await bot.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(e.Guild.Id)).ActionHistoryEnabled) {
                LogExt.Debug(bot.GetId(e.Guild.Id), "Adding forbidden name entry to action history: {Member}, {Guild}", e.Member, e.Guild);
                await bot.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
                    Type = ActionHistoryEntry.Action.ForbiddenName,
                    GuildId = e.Guild.Id,
                    Notes = ls.GetString(e.Guild.Id, TranslationKey.rsn_fname_match(fname?.RegexString ?? "?")),
                    Time = DateTimeOffset.Now,
                    UserId = e.Member.Id
                });
            }
            try {
                if (fname?.ActionOverride is { })
                    await fns.PunishMemberAsync(e.Guild, e.Member, fname.ActionOverride.Value);
                else
                    await e.Member.ModifyAsync(m => {
                        m.Nickname = e.Member.Id.ToString();
                        m.AuditLogReason = ls.GetString(e.Guild.Id, TranslationKey.rsn_fname_match(fname?.RegexString ?? "?"));
                    });
                emb.AddLocalizedField(TranslationKey.str_act_taken, TranslationKey.act_fname_match);
                if (!e.Member.IsBot)
                    await e.Member.SendMessageAsync(ls.GetString(null, TranslationKey.dm_fname_match(Formatter.Italic(e.Guild.Name))));
            } catch (UnauthorizedException) {
                emb.AddLocalizedField(TranslationKey.str_err, TranslationKey.err_fname_match);
            }
        }

        if (gcfg.ActionHistoryEnabled) {
            IReadOnlyList<ActionHistoryEntry> history = await bot.Services.GetRequiredService<ActionHistoryService>().GetAllAsync((e.Guild.Id, e.Member.Id));
            if (history.Any()) {
                IEnumerable<ActionHistoryEntry> orderedHistory = history
                        .OrderByDescending(e => e.Type)
                        .ThenByDescending(e => e.Time)
                        .Take(5)
                    ;
                foreach (ActionHistoryEntry ahe in orderedHistory) {
                    TranslationKey title = ahe.Type.ToLocalizedKey();
                    TranslationKey content = TranslationKey.fmt_ah_emb(ls.GetLocalizedTimeString(e.Guild.Id, ahe.Time), ahe.Notes);
                    emb.AddLocalizedField(title, content);
                }
            }
        }

        await logService.LogAsync(e.Guild, emb);


        static LocalizedEmbedBuilder AddMemberInfoToEmbed(LocalizedEmbedBuilder emb, DiscordMember member)
        {
            emb.WithThumbnail(member.AvatarUrl);
            emb.AddLocalizedTimestampField(TranslationKey.str_regtime, member.CreationTimestamp, true);
            emb.AddLocalizedField(TranslationKey.str_ahash, member.AvatarHash, true, false);
            emb.AddLocalizedField(TranslationKey.str_flags, member.Flags?.Humanize(), true, false);
            emb.AddLocalizedField(TranslationKey.str_locale, member.Locale, true, false);
            emb.AddLocalizedField(TranslationKey.str_mfa, member.MfaEnabled, true, false);
            emb.AddLocalizedField(TranslationKey.str_flags_oauth, member.OAuthFlags?.Humanize(LetterCasing.Sentence), true, false);
            emb.AddLocalizedField(TranslationKey.str_verified, member.Verified, true, false);
            emb.AddLocalizedField(TranslationKey.str_premium_type, member.PremiumType?.Humanize(), true, false);
            emb.AddLocalizedTimestampField(TranslationKey.str_premium_since, member.PremiumSince, true);
            emb.AddLocalizedField(TranslationKey.str_email, member.Email, true, false);
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

        await shard.Services.GetRequiredService<ProtectionService>().ReapplyLoggedPunishmentsIfNececaryAsync(e.Guild, e.Member);
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
                    ? bot.Services.GetRequiredService<LocalizationService>().GetString(e.Guild.Id, TranslationKey.fmt_leave(e.Member.Mention))
                    : gcfg.LeaveMessage.Replace("%user%", e.Member.Mention);
                await LoggingService.TryExecuteWithReportAsync(
                    bot, e.Guild, lchn.EmbedAsync(leaveStr, Emojis.Wave), TranslationKey.rep_lchn_403, TranslationKey.rep_lchn_404,
                    code404action: () => gcs.ModifyConfigAsync(e.Guild.Id, cfg => cfg.LeaveChannelId = default)
                );
            }
        }

        if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb) && !gcfg.ActionHistoryEnabled)
            return;

        LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();

        emb.WithLocalizedTitle(DiscordEventType.GuildMemberRemoved, TranslationKey.evt_gld_mem_left, e.Member);

        DiscordAuditLogKickEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogKickEntry>(AuditLogActionType.Kick);
        if (entry?.Target?.Id == e.Member.Id) {
            emb.AddFieldsFromAuditLogEntry(entry, (emb, _) => emb.WithLocalizedTitle(DiscordEventType.GuildMemberRemoved, TranslationKey.evt_gld_kick));
            if ((await bot.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(e.Guild.Id)).ActionHistoryEnabled) {
                LogExt.Debug(bot.GetId(e.Guild.Id), "Adding kick entry to action history: {Member}, {Guild}", e.Member, e.Guild);
                await bot.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
                    Type = ActionHistoryEntry.Action.Kick,
                    GuildId = e.Guild.Id,
                    Notes = ls.GetString(e.Guild.Id, TranslationKey.fmt_ah(entry.UserResponsible.Mention, entry.Reason)),
                    Time = DateTimeOffset.Now,
                    UserId = e.Member.Id
                });
            }
        }
        emb.WithThumbnail(e.Member.AvatarUrl);
        emb.AddLocalizedField(TranslationKey.str_regtime, ls.GetLocalizedTimeString(e.Guild.Id, e.Member.CreationTimestamp), true);
        emb.AddLocalizedField(TranslationKey.str_email, e.Member.Email, unknown: false);

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
                    if (fname.ActionOverride is { })
                        await fns.PunishMemberAsync(e.Guild, e.Member, fname.ActionOverride.Value);
                    else
                        await e.Member.ModifyAsync(m => {
                            m.Nickname = e.Member.Id.ToString();
                            m.AuditLogReason = ls.GetString(e.Guild.Id, TranslationKey.rsn_fname_match(fname.RegexString));
                        });
                    renamed = true;
                    if (!e.Member.IsBot)
                        await e.Member.SendMessageAsync(ls.GetString(null, TranslationKey.dm_fname_match(Formatter.Italic(e.Guild.Name))));
                } catch (UnauthorizedException) {
                    failed = true;
                }

                if ((await bot.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(e.Guild.Id)).ActionHistoryEnabled) {
                    LogExt.Debug(bot.GetId(e.Guild.Id), "Adding forbidden name entry to action history: {Member}, {Guild}", e.Member, e.Guild);
                    await bot.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
                        Type = ActionHistoryEntry.Action.ForbiddenName,
                        GuildId = e.Guild.Id,
                        Notes = ls.GetString(e.Guild.Id, TranslationKey.rsn_fname_match(fname.RegexString)),
                        Time = DateTimeOffset.Now,
                        UserId = e.Member.Id
                    });
                }
            }
        }

        ProtectionService ps = bot.Services.GetRequiredService<ProtectionService>();
        DiscordRole? muteRole = await ps.UnsafeGetMuteRoleAsync(e.Guild);
        if (e.RolesBefore.Contains(muteRole) && !e.RolesAfter.Contains(muteRole))
            await ps.RemoveLoggedPunishmentInCaseOfRejoinAsync(e.Guild, e.Member, Punishment.Action.PermanentMute);
        else if (!e.RolesBefore.Contains(muteRole) && e.RolesAfter.Contains(muteRole))
            await ps.LogPunishmentInCaseOfRejoinAsync(e.Guild, e.Member, Punishment.Action.PermanentMute);

        if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
            return;

        emb.WithLocalizedTitle(DiscordEventType.GuildMemberUpdated, TranslationKey.evt_gld_mem_upd, e.Member);
        emb.WithThumbnail(e.Member.AvatarUrl);

        DiscordAuditLogEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogEntry>();
        switch (entry) {
            case null:
                if (e.NicknameBefore != e.NicknameAfter)
                    AddNicknameChangeField(emb, e.NicknameBefore, e.NicknameAfter);
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_member_screening, e.PendingBefore, e.PendingAfter, true);
                emb.AddLocalizedField(TranslationKey.str_premium_type, e.Member.PremiumType?.Humanize(), true, false);
                emb.AddLocalizedTimestampField(TranslationKey.str_premium_since, e.Member.PremiumSince, true);
                if (!e.RolesBefore.OrderBy(r => r.Id).SequenceEqual(e.RolesAfter.OrderBy(r => r.Id))) {
                    string rolesBefore = e.RolesBefore.Where(r => r.Id != e.Guild.Id).Select(r => r.Mention).Humanize(", ");
                    string rolesAfter = e.RolesAfter.Where(r => r.Id != e.Guild.Id).Select(r => r.Mention).Humanize(", ");
                    string noneStr = ls.GetString(e.Guild.Id, TranslationKey.str_none);
                    emb.AddLocalizedField(TranslationKey.str_roles_bef, string.IsNullOrWhiteSpace(rolesBefore) ? noneStr : rolesBefore, true);
                    emb.AddLocalizedField(TranslationKey.str_roles_aft, string.IsNullOrWhiteSpace(rolesAfter) ? noneStr : rolesAfter, true);
                }
                break;
            case DiscordAuditLogMemberUpdateEntry uentry:
                emb.AddFieldsFromAuditLogEntry(uentry, (emb, ent) => {
                    if (ent.NicknameChange is { })
                        AddNicknameChangeField(emb, ent.NicknameChange.Before, ent.NicknameChange.After);
                    emb.AddLocalizedField(TranslationKey.str_roles_add, ent.AddedRoles?.Select(r => r.Mention).Humanize(", "), true, false);
                    emb.AddLocalizedField(TranslationKey.str_roles_rem, ent.RemovedRoles?.Select(r => r.Mention).Humanize(", "), true, false);
                });
                break;
            case DiscordAuditLogMemberMoveEntry mentry:
                // TODO
                emb.WithLocalizedTitle(DiscordEventType.GuildMemberUpdated, TranslationKey.evt_gld_mem_vc_mv, e.Member);
                Log.Debug("{Member}", e.Member);
                emb.AddFieldsFromAuditLogEntry(mentry, (emb, ent) => {
                    emb.WithDescription(ent.Channel);
                    emb.AddLocalizedField(TranslationKey.str_move_count, ent.UserCount);
                });
                break;
        }

        if (renamed)
            emb.AddLocalizedField(TranslationKey.str_act_taken, TranslationKey.act_fname_match);
        if (failed)
            emb.AddLocalizedField(TranslationKey.str_err, TranslationKey.err_fname_match);

        if (emb.FieldCount > 0)
            await logService.LogAsync(e.Guild, emb);


        static void AddNicknameChangeField(LocalizedEmbedBuilder emb, string? nameBefore, string? nameAfter)
        {
            if (string.IsNullOrWhiteSpace(nameAfter))
                emb.AddLocalizedField(TranslationKey.evt_nick_clear, string.IsNullOrWhiteSpace(nameBefore) ? null: Formatter.InlineCode(nameBefore));
            else
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_name, nameBefore, nameAfter);
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

            emb.WithLocalizedTitle(DiscordEventType.GuildMemberUpdated, TranslationKey.evt_gld_mem_upd, e.User);
            emb.WithThumbnail(e.UserAfter.AvatarUrl);

            emb.AddLocalizedPropertyChangeField(TranslationKey.str_name, e.UserBefore.Username, e.UserAfter.Username);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_discriminator, e.UserBefore.Discriminator, e.UserAfter.Discriminator);
            if (e.UserAfter.AvatarUrl != e.UserBefore.AvatarUrl)
                emb.AddLocalizedField(TranslationKey.str_avatar, Formatter.MaskedUrl(ls.GetString(guild.Id, TranslationKey.str_avatar_old), new Uri(e.UserBefore.AvatarUrl)));
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_email, e.UserBefore.Email, e.UserAfter.Email);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_locale, e.UserBefore.Locale, e.UserAfter.Locale);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_mfa, e.UserBefore.MfaEnabled, e.UserAfter.MfaEnabled);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_flags_oauth, e.UserBefore.OAuthFlags, e.UserAfter.OAuthFlags);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_premium_type, e.UserBefore.PremiumType, e.UserAfter.PremiumType);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_verified, e.UserBefore.Verified, e.UserAfter.Verified);

            // TODO improve
            // emb.AddLocalizedField(TranslationKey.str_activity, e.Activity.ToDetailedString());

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