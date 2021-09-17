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

            emb.WithLocalizedTitle(DiscordEventType.GuildBanAdded, "evt-gld-ban-add");

            DiscordAuditLogBanEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogBanEntry>(AuditLogActionType.Ban);
            emb.WithDescription(e.Member);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);

            // TODO handle temp ban separately
            if ((await bot.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(e.Guild.Id)).ActionHistoryEnabled) {
                LogExt.Debug(bot.GetId(e.Guild.Id), "Adding ban entry to action history: {Member}, {Guild}", e.Member, e.Guild);
                LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
                await bot.Services.GetRequiredService<ActionHistoryService>().LimitedAddAsync(new ActionHistoryEntry {
                    Action = ActionHistoryEntry.ActionType.PermanentBan,
                    GuildId = e.Guild.Id,
                    Notes = entry is null ? null : ls.GetString(e.Guild.Id, "fmt-ah", entry.UserResponsible.Mention, entry.Reason),
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

            emb.WithLocalizedTitle(DiscordEventType.GuildBanAdded, "evt-gld-ban-del");

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

            emb.WithLocalizedTitle(DiscordEventType.GuildEmojisUpdated, "evt-gld-emoji-chn", desc: null, action);

            DiscordAuditLogEmojiEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogEmojiEntry>(action);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                if (ent.Target is null) {
                    emb.WithLocalizedDescription("evt-gld-emoji-unk");
                    return;
                }
                emb.WithThumbnail(ent.Target.Url);
                switch (action) {
                    case AuditLogActionType.EmojiCreate:
                        emb.AddLocalizedTitleField("evt-gld-emoji-add", ent.Target.Name, inline: true);
                        break;
                    case AuditLogActionType.EmojiDelete:
                        emb.AddLocalizedTitleField("evt-gld-emoji-del", ent.NameChange.Before, inline: true);
                        break;
                    case AuditLogActionType.EmojiUpdate:
                        emb.AddLocalizedPropertyChangeField("str-name", ent.NameChange, inline: true);
                        break;
                }
            });

            if (entry is null) {
                emb.AddLocalizedTitleField("evt-gld-emoji-bef", e.EmojisBefore?.Count, inline: true);
                emb.AddLocalizedTitleField("evt-gld-emoji-aft", e.EmojisAfter?.Count, inline: true);
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

            emb.WithLocalizedTitle(DiscordEventType.GuildStickersUpdated, "evt-gld-sticker-chn", desc: null, action);

            DiscordAuditLogStickerEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogStickerEntry>(action);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                if (ent.Target is null) {
                    emb.WithLocalizedDescription("evt-gld-sticker-unk");
                    return;
                }
                emb.WithThumbnail(ent.Target.StickerUrl);
                switch (action) {
                    case AuditLogActionType.StickerCreate:
                        emb.AddLocalizedTitleField("evt-gld-sticker-add", ent.Target.Name, inline: true);
                        break;
                    case AuditLogActionType.StickerDelete:
                        emb.AddLocalizedTitleField("evt-gld-sticker-del", ent.NameChange.Before, inline: true);
                        break;
                    case AuditLogActionType.StickerUpdate:
                        emb.AddLocalizedPropertyChangeField("str-name", ent.NameChange, inline: true);
                        emb.AddLocalizedPropertyChangeField("str-tags", ent.TagsChange, inline: true);
                        emb.AddLocalizedPropertyChangeField("str-available", ent.AvailabilityChange, inline: true);
                        emb.AddLocalizedPropertyChangeField("str-asset", ent.AssetChange, inline: true);
                        emb.AddLocalizedPropertyChangeField("str-format", ent.FormatChange, inline: true);
                        emb.AddLocalizedPropertyChangeField("str-type", ent.TypeChange, inline: true);
                        emb.AddLocalizedPropertyChangeField("str-desc", ent.DescriptionChange, inline: true);
                        break;
                }
            });

            if (entry is null) {
                emb.AddLocalizedTitleField("evt-gld-emoji-bef", e.StickersBefore?.Count, inline: true);
                emb.AddLocalizedTitleField("evt-gld-emoji-aft", e.StickersAfter?.Count, inline: true);
            }

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildIntegrationsUpdated)]
        public static async Task GuildIntegrationsUpdateEventHandlerAsync(TheGodfatherBot bot, GuildIntegrationsUpdateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Integrations updated: {Guild}", e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildIntegrationsUpdated, "evt-gld-int-upd");
            DiscordAuditLogIntegrationEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogIntegrationEntry>(AuditLogActionType.Ban);
            if (entry is not null) {
                emb.AddLocalizedPropertyChangeField("str-expire-behavior", entry.ExpireBehavior);
                emb.AddLocalizedPropertyChangeField("str-expire-grace-period", entry.ExpireGracePeriod);
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

            emb.WithLocalizedTitle(DiscordEventType.GuildRoleCreated, "evt-gld-role-add", e.Role);
            emb.AddLocalizedPropertyChangeField("str-managed", false, e.Role?.IsManaged ?? false, inline: true);

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

            emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "evt-gld-role-del", e.Role);

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

            emb.WithLocalizedTitle(DiscordEventType.GuildRoleUpdated, "evt-gld-role-upd", e.RoleBefore);
            emb.AddLocalizedPropertyChangeField("evt-upd-name", e.RoleBefore.Name, e.RoleAfter.Name);
            emb.AddLocalizedPropertyChangeField("evt-upd-color", e.RoleBefore.Color, e.RoleAfter.Color);
            emb.AddLocalizedPropertyChangeField("evt-upd-hoist", e.RoleBefore.IsHoisted, e.RoleAfter.IsHoisted);
            emb.AddLocalizedPropertyChangeField("evt-upd-managed", e.RoleBefore.IsManaged, e.RoleAfter.IsManaged);
            emb.AddLocalizedPropertyChangeField("evt-upd-mention", e.RoleBefore.IsMentionable, e.RoleAfter.IsMentionable);
            emb.AddLocalizedPropertyChangeField("evt-upd-position", e.RoleBefore.Position, e.RoleAfter.Position);

            DiscordAuditLogRoleUpdateEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleUpdate);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                // TODO use permissions_new once it is implemented in D#+
                emb.AddLocalizedTitleField("str-perms", ent.PermissionChange?.After, inline: false, unknown: false);
            });

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildUpdated)]
        public static async Task GuildUpdateEventHandlerAsync(TheGodfatherBot bot, GuildUpdateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.GuildBefore.Id), "Guild updated: {Guild}", e.GuildBefore);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.GuildBefore.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;
            
            emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, "evt-gld-upd");

            emb.AddLocalizedPropertyChangeField("str-afktime", e.GuildBefore.AfkTimeout, e.GuildAfter.AfkTimeout);
            emb.AddLocalizedPropertyChangeField("str-banner", e.GuildBefore.BannerUrl, e.GuildAfter.BannerUrl);
            emb.AddLocalizedPropertyChangeField("str-boosters", e.GuildBefore.PremiumSubscriptionCount, e.GuildAfter.PremiumSubscriptionCount);
            emb.AddLocalizedPropertyChangeField("str-notifications", e.GuildBefore.DefaultMessageNotifications, e.GuildAfter.DefaultMessageNotifications);
            emb.AddLocalizedPropertyChangeField("str-desc", e.GuildBefore.Description, e.GuildAfter.Description);
            emb.AddLocalizedPropertyChangeField("str-discovery-url", e.GuildBefore.DiscoverySplashUrl, e.GuildAfter.DiscoverySplashUrl);
            emb.AddLocalizedPropertyChangeField("str-nsfw", e.GuildBefore.ExplicitContentFilter, e.GuildAfter.ExplicitContentFilter);
            emb.AddLocalizedPropertyChangeField("str-icon", e.GuildBefore.IconUrl, e.GuildAfter.IconUrl);
            emb.AddLocalizedPropertyChangeField("str-locale", e.GuildBefore.PreferredLocale, e.GuildAfter.PreferredLocale);
            emb.AddLocalizedPropertyChangeField("str-tier", e.GuildBefore.PremiumTier, e.GuildAfter.PremiumTier);
            emb.AddLocalizedPropertyChangeField("str-vanity-url", e.GuildBefore.VanityUrlCode, e.GuildAfter.VanityUrlCode);
            // FIXME Remove the comment once D#+ issue is fixed
            // emb.AddLocalizedPropertyChangeField("str-region", e.GuildBefore.VoiceRegion, e.GuildAfter.VoiceRegion);
            DiscordAuditLogGuildEntry? entry = await e.GuildAfter.GetLatestAuditLogEntryAsync<DiscordAuditLogGuildEntry>(AuditLogActionType.GuildUpdate);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                emb.AddLocalizedPropertyChangeField("str-name", ent.NameChange);
                emb.AddLocalizedPropertyChangeField("str-afkchn", ent.AfkChannelChange);
                emb.AddLocalizedPropertyChangeField("str-embchn", ent.EmbedChannelChange);
                emb.AddLocalizedPropertyChangeField("str-mfa", ent.MfaLevelChange);
                emb.AddLocalizedPropertyChangeField("str-owner", ent.OwnerChange);
                emb.AddLocalizedPropertyChangeField("str-region", ent.RegionChange);
                emb.AddLocalizedPropertyChangeField("str-splash", ent.SplashChange);
                emb.AddLocalizedPropertyChangeField("str-syschn", ent.SystemChannelChange);
                emb.AddLocalizedPropertyChangeField("str-verlvl", ent.VerificationLevelChange);
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

            emb.WithLocalizedTitle(DiscordEventType.InviteCreated, "evt-gld-inv-add", e.Channel);
            emb.AddLocalizedTitleField("str-code", e.Invite.Code, inline: true);
            emb.AddLocalizedTitleField("str-revoked", e.Invite.IsRevoked, inline: true);
            emb.AddLocalizedTitleField("str-temporary", e.Invite.IsTemporary, inline: true);
            emb.AddLocalizedTitleField("str-max-age-s", e.Invite.MaxAge, inline: true);
            emb.AddLocalizedTitleField("str-max-uses", e.Invite.MaxUses, inline: true);
            if (e.Invite.Inviter is { })
                emb.AddInvocationFields(e.Invite.Inviter, e.Channel);
            return logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.InviteDeleted)]
        public static async Task InviteDeletedEventHandlerAsync(TheGodfatherBot bot, InviteDeleteEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Guild invite deleted: {Invite} {Guild}", e.Invite, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
            emb.WithLocalizedTitle(DiscordEventType.InviteDeleted, "evt-gld-inv-add", e.Channel);
            emb.AddLocalizedTitleField("str-code", e.Invite.Code, inline: true);
            emb.AddLocalizedTitleField("str-revoked", e.Invite.IsRevoked, inline: true);
            emb.AddLocalizedTitleField("str-temporary", e.Invite.IsTemporary, inline: true);
            emb.AddLocalizedTitleField("str-max-age-s", e.Invite.MaxAge, inline: true);
            emb.AddLocalizedTitleField("str-max-uses", e.Invite.MaxUses, inline: true);
            emb.AddLocalizedTitleField("str-created-by", e.Invite.Inviter?.Mention, inline: true, unknown: false);
            emb.AddLocalizedTitleField("str-created-at", ls.GetLocalizedTimeString(e.Guild.Id, e.Invite.CreatedAt));
            await logService.LogAsync(e.Guild, emb);
        }
    }
}
