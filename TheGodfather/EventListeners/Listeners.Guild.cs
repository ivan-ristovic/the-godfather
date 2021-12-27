using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Database.Models;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;
using TheGodfather.Translations;

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.GuildBanAdded)]
        public static async Task GuildBanEventHandlerAsync(TheGodfatherBot bot, GuildBanAddEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Banned: {Member} {Guild}", e.Member, e.Guild);

            GuildConfigService gcs = bot.Services.GetRequiredService<GuildConfigService>();
            GuildConfig gcfg = await gcs.GetConfigAsync(e.Guild.Id);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb) && !gcfg.ActionHistoryEnabled)
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildBanAdded, TranslationKey.evt_gld_ban_add);

            DiscordAuditLogBanEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogBanEntry>(AuditLogActionType.Ban);
            emb.WithDescription(e.Member);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);

            // TODO handle temp ban separately
            if ((await bot.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(e.Guild.Id)).ActionHistoryEnabled) {
                LogExt.Debug(bot.GetId(e.Guild.Id), "Adding ban entry to action history: {Member}, {Guild}", e.Member, e.Guild);
                LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
                await bot.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
                    Type = ActionHistoryEntry.Action.PermanentBan,
                    GuildId = e.Guild.Id,
                    Notes = entry is null ? null : ls.GetString(e.Guild.Id, TranslationKey.fmt_ah(entry.UserResponsible.Mention, entry.Reason)),
                    Time = DateTimeOffset.Now,
                    UserId = e.Member.Id,
                });
            }
        }

        [AsyncEventListener(DiscordEventType.GuildBanRemoved)]
        public static async Task GuildUnbanEventHandlerAsync(TheGodfatherBot bot, GuildBanRemoveEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Unanned: {Member} {Guild}", e.Member, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildBanAdded, TranslationKey.evt_gld_ban_del);

            DiscordAuditLogBanEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogBanEntry>(AuditLogActionType.Unban);
            emb.WithDescription(e.Member);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildDeleted)]
        public static Task GuildDeleteEventHandlerAsync(TheGodfatherBot bot, GuildDeleteEventArgs e)
        {
            LogExt.Information(bot.GetId(e.Guild.Id), "Guild deleted {Guild}", e.Guild);
            return bot.Services.GetRequiredService<GuildConfigService>().UnregisterGuildAsync(e.Guild.Id);
        }
        
        [AsyncEventListener(DiscordEventType.GuildEmojisUpdated)]
        public static async Task GuildEmojisUpdateEventHandlerAsync(TheGodfatherBot bot, GuildEmojisUpdateEventArgs e)
        {
            LogExt.Information(bot.GetId(e.Guild.Id), "Emojis updated {Guild}", e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            AuditLogActionType action = e.EmojisAfter.Count > e.EmojisBefore.Count
                ? AuditLogActionType.EmojiCreate
                : e.EmojisAfter.Count < e.EmojisBefore.Count
                    ? AuditLogActionType.EmojiDelete
                    : AuditLogActionType.EmojiUpdate;

            emb.WithLocalizedTitle(DiscordEventType.GuildEmojisUpdated, TranslationKey.evt_gld_emoji_chn(action));

            DiscordAuditLogEmojiEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogEmojiEntry>(action);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                if (ent.Target is null) {
                    emb.WithLocalizedDescription(TranslationKey.evt_gld_emoji_unk);
                    return;
                }
                emb.WithThumbnail(ent.Target.Url);
                switch (action) {
                    case AuditLogActionType.EmojiCreate:
                        emb.AddLocalizedField(TranslationKey.evt_gld_emoji_add, ent.Target.Name, inline: true);
                        break;
                    case AuditLogActionType.EmojiDelete:
                        emb.AddLocalizedField(TranslationKey.evt_gld_emoji_del, ent.NameChange.Before, inline: true);
                        break;
                    case AuditLogActionType.EmojiUpdate:
                        emb.AddLocalizedPropertyChangeField(TranslationKey.str_name, ent.NameChange, inline: true);
                        break;
                }
            });

            if (entry is null) {
                emb.AddLocalizedField(TranslationKey.evt_gld_emoji_bef, e.EmojisBefore?.Count, inline: true);
                emb.AddLocalizedField(TranslationKey.evt_gld_emoji_aft, e.EmojisAfter?.Count, inline: true);
            }

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildStickersUpdated)]
        public static async Task GuildStickersUpdateEventHandlerAsync(TheGodfatherBot bot, GuildStickersUpdateEventArgs e)
        {
            LogExt.Information(bot.GetId(e.Guild.Id), "Stickers updated {Guild}", e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            AuditLogActionType action = e.StickersBefore.Count > e.StickersAfter.Count
                ? AuditLogActionType.StickerCreate
                : e.StickersAfter.Count < e.StickersBefore.Count
                    ? AuditLogActionType.StickerDelete
                    : AuditLogActionType.StickerUpdate;

            emb.WithLocalizedTitle(DiscordEventType.GuildStickersUpdated, TranslationKey.evt_gld_sticker_chn(action));

            DiscordAuditLogStickerEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogStickerEntry>(action);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                if (ent.Target is null) {
                    emb.WithLocalizedDescription(TranslationKey.evt_gld_sticker_unk);
                    return;
                }
                emb.WithThumbnail(ent.Target.StickerUrl);
                switch (action) {
                    case AuditLogActionType.StickerCreate:
                        emb.AddLocalizedField(TranslationKey.evt_gld_sticker_add, ent.Target.Name, inline: true);
                        break;
                    case AuditLogActionType.StickerDelete:
                        emb.AddLocalizedField(TranslationKey.evt_gld_sticker_del, ent.NameChange.Before, inline: true);
                        break;
                    case AuditLogActionType.StickerUpdate:
                        emb.AddLocalizedPropertyChangeField(TranslationKey.str_name, ent.NameChange, inline: true);
                        emb.AddLocalizedPropertyChangeField(TranslationKey.str_tags, ent.TagsChange, inline: true);
                        emb.AddLocalizedPropertyChangeField(TranslationKey.str_available, ent.AvailabilityChange, inline: true);
                        emb.AddLocalizedPropertyChangeField(TranslationKey.str_asset, ent.AssetChange, inline: true);
                        emb.AddLocalizedPropertyChangeField(TranslationKey.str_format, ent.FormatChange, inline: true);
                        emb.AddLocalizedPropertyChangeField(TranslationKey.str_type, ent.TypeChange, inline: true);
                        emb.AddLocalizedPropertyChangeField(TranslationKey.str_desc, ent.DescriptionChange, inline: true);
                        break;
                }
            });

            if (entry is null) {
                emb.AddLocalizedField(TranslationKey.evt_gld_emoji_bef, e.StickersBefore?.Count, inline: true);
                emb.AddLocalizedField(TranslationKey.evt_gld_emoji_aft, e.StickersAfter?.Count, inline: true);
            }

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildIntegrationsUpdated)]
        public static async Task GuildIntegrationsUpdateEventHandlerAsync(TheGodfatherBot bot, GuildIntegrationsUpdateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Integrations updated: {Guild}", e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildIntegrationsUpdated, TranslationKey.evt_gld_int_upd);
            DiscordAuditLogIntegrationEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogIntegrationEntry>(AuditLogActionType.Ban);
            if (entry is not null) {
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_expire_behavior, entry.ExpireBehavior);
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_expire_grace_period, entry.ExpireGracePeriod);
                emb.AddFieldsFromAuditLogEntry(entry);
            }

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleCreated)]
        public static async Task GuildRoleCreateEventHandlerAsync(TheGodfatherBot bot, GuildRoleCreateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Role created: {Role} {Guild}", e.Role, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildRoleCreated, TranslationKey.evt_gld_role_add, e.Role);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_managed, false, e.Role?.IsManaged ?? false, inline: true);

            DiscordAuditLogRoleUpdateEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleCreate);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleDeleted)]
        public static async Task GuildRoleDeleteEventHandlerAsync(TheGodfatherBot bot, GuildRoleDeleteEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Role deleted: {Role} {Guild}", e.Role, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, TranslationKey.evt_gld_role_del, e.Role);

            DiscordAuditLogRoleUpdateEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleDelete);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleUpdated)]
        public static async Task GuildRoleUpdateEventHandlerAsync(TheGodfatherBot bot, GuildRoleUpdateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Role updated: {Role} {Guild}", e.RoleBefore, e.Guild);

            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildRoleUpdated, TranslationKey.evt_gld_role_upd, e.RoleBefore);
            emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_name, e.RoleBefore.Name, e.RoleAfter.Name);
            emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_color, e.RoleBefore.Color, e.RoleAfter.Color);
            emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_hoist, e.RoleBefore.IsHoisted, e.RoleAfter.IsHoisted);
            emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_managed, e.RoleBefore.IsManaged, e.RoleAfter.IsManaged);
            emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_mention, e.RoleBefore.IsMentionable, e.RoleAfter.IsMentionable);
            emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_position, e.RoleBefore.Position, e.RoleAfter.Position);

            DiscordAuditLogRoleUpdateEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleUpdate);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                // TODO use permissions_new once it is implemented in D#+
                emb.AddLocalizedField(TranslationKey.str_perms, ent.PermissionChange?.After, inline: false, unknown: false);
            });

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildUpdated)]
        public static async Task GuildUpdateEventHandlerAsync(TheGodfatherBot bot, GuildUpdateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.GuildBefore.Id), "Guild updated: {Guild}", e.GuildBefore);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.GuildBefore.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;
            
            emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_gld_upd);

            emb.AddLocalizedPropertyChangeField(TranslationKey.str_afktime, e.GuildBefore.AfkTimeout, e.GuildAfter.AfkTimeout);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_banner, e.GuildBefore.BannerUrl, e.GuildAfter.BannerUrl);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_boosters, e.GuildBefore.PremiumSubscriptionCount, e.GuildAfter.PremiumSubscriptionCount);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_notifications, e.GuildBefore.DefaultMessageNotifications, e.GuildAfter.DefaultMessageNotifications);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_desc, e.GuildBefore.Description, e.GuildAfter.Description);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_discovery_url, e.GuildBefore.DiscoverySplashUrl, e.GuildAfter.DiscoverySplashUrl);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_nsfw, e.GuildBefore.ExplicitContentFilter, e.GuildAfter.ExplicitContentFilter);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_icon, e.GuildBefore.IconUrl, e.GuildAfter.IconUrl);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_locale, e.GuildBefore.PreferredLocale, e.GuildAfter.PreferredLocale);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_tier, e.GuildBefore.PremiumTier, e.GuildAfter.PremiumTier);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_vanity_url, e.GuildBefore.VanityUrlCode, e.GuildAfter.VanityUrlCode);
            // FIXME Remove the comment once D#+ issue is fixed
            // emb.AddLocalizedPropertyChangeField(TranslationKey.str_region, e.GuildBefore.VoiceRegion, e.GuildAfter.VoiceRegion);
            DiscordAuditLogGuildEntry? entry = await e.GuildAfter.GetLatestAuditLogEntryAsync<DiscordAuditLogGuildEntry>(AuditLogActionType.GuildUpdate);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_name, ent.NameChange);
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_afkchn, ent.AfkChannelChange);
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_embchn, ent.EmbedChannelChange);
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_mfa, ent.MfaLevelChange);
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_owner, ent.OwnerChange);
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_region, ent.RegionChange);
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_splash, ent.SplashChange);
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_syschn, ent.SystemChannelChange);
                emb.AddLocalizedPropertyChangeField(TranslationKey.str_verlvl, ent.VerificationLevelChange);
            }, errReport: false);

            await logService.LogAsync(e.GuildAfter, emb);
        }

        [AsyncEventListener(DiscordEventType.InviteCreated)]
        public static Task InviteCreatedEventHandlerAsync(TheGodfatherBot bot, InviteCreateEventArgs e)
        {
            if (e.Guild is null)
                return Task.CompletedTask;

            LogExt.Debug(bot.GetId(e.Guild.Id), "Guild invite created: {Invite} {Guild}", e.Invite, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return Task.CompletedTask;

            emb.WithLocalizedTitle(DiscordEventType.InviteCreated, TranslationKey.evt_gld_inv_add, e.Channel);
            emb.AddLocalizedField(TranslationKey.str_code, e.Invite.Code, inline: true);
            emb.AddLocalizedField(TranslationKey.str_revoked, e.Invite.IsRevoked, inline: true);
            emb.AddLocalizedField(TranslationKey.str_temporary, e.Invite.IsTemporary, inline: true);
            emb.AddLocalizedField(TranslationKey.str_max_age_s, e.Invite.MaxAge, inline: true);
            emb.AddLocalizedField(TranslationKey.str_max_uses, e.Invite.MaxUses, inline: true);
            if (e.Invite.Inviter is { })
                emb.AddInvocationFields(e.Invite.Inviter, e.Channel);
            return logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.InviteDeleted)]
        public static Task InviteDeletedEventHandlerAsync(TheGodfatherBot bot, InviteDeleteEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Guild invite deleted: {Invite} {Guild}", e.Invite, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return Task.CompletedTask;

            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
            emb.WithLocalizedTitle(DiscordEventType.InviteDeleted, TranslationKey.evt_gld_inv_del, e.Channel);
            emb.AddLocalizedField(TranslationKey.str_code, e.Invite.Code, inline: true);
            emb.AddLocalizedField(TranslationKey.str_revoked, e.Invite.IsRevoked, inline: true);
            emb.AddLocalizedField(TranslationKey.str_temporary, e.Invite.IsTemporary, inline: true);
            emb.AddLocalizedField(TranslationKey.str_max_age_s, e.Invite.MaxAge, inline: true);
            emb.AddLocalizedField(TranslationKey.str_max_uses, e.Invite.MaxUses, inline: true);
            emb.AddLocalizedField(TranslationKey.str_created_by, e.Invite.Inviter?.Mention, inline: true, unknown: false);
            emb.AddLocalizedField(TranslationKey.str_created_at, ls.GetLocalizedTimeString(e.Guild.Id, e.Invite.CreatedAt));
            return logService.LogAsync(e.Guild, emb);
        }
    }
}
