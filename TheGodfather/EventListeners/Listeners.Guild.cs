using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
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
        [AsyncEventListener(DiscordEventType.GuildBanAdded)]
        public static async Task GuildBanEventHandlerAsync(TheGodfatherShard shard, GuildBanAddEventArgs e)
        {
            LogExt.Debug(shard.Id, "Banned: {Member} {Guild}", e.Member, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildBanAdded, "evt-gld-ban-add");

            DiscordAuditLogBanEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogBanEntry>(AuditLogActionType.Ban);
            emb.WithDescription(e.Member);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildBanRemoved)]
        public static async Task GuildUnbanEventHandlerAsync(TheGodfatherShard shard, GuildBanRemoveEventArgs e)
        {
            LogExt.Debug(shard.Id, "Unanned: {Member} {Guild}", e.Member, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildBanAdded, "evt-gld-ban-del");

            DiscordAuditLogBanEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogBanEntry>(AuditLogActionType.Unban);
            emb.WithDescription(e.Member);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildDeleted)]
        public static Task GuildDeleteEventHandlerAsync(TheGodfatherShard shard, GuildDeleteEventArgs e)
        {
            LogExt.Information(shard.Id, "Guild deleted {Guild}", e.Guild);
            LogExt.Debug(shard.Id, "Guild deleted event args: {@GuildDeleteEventArgs}", e);
            return shard.Services.GetService<GuildConfigService>().UnregisterGuildAsync(e.Guild.Id);
        }

        [AsyncEventListener(DiscordEventType.GuildEmojisUpdated)]
        public static async Task GuildEmojisUpdateEventHandlerAsync(TheGodfatherShard shard, GuildEmojisUpdateEventArgs e)
        {
            LogExt.Information(shard.Id, "Emojis updated {Guild}, ({EmojisBefore} to {EmojisAfter}", e.Guild, e.EmojisBefore, e.EmojisAfter);
            LogExt.Debug(shard.Id, "Emojis updated: {@GuildEmojisUpdateEventArgs}", e);
            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            AuditLogActionType action = e.EmojisAfter.Count > e.EmojisBefore.Count
                ? AuditLogActionType.EmojiCreate
                : e.EmojisAfter.Count < e.EmojisBefore.Count
                    ? AuditLogActionType.EmojiDelete
                    : AuditLogActionType.EmojiUpdate;

            emb.WithLocalizedTitle(DiscordEventType.GuildBanAdded, "evt-gld-emoji-chn", desc: null, action);

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
                    default:
                        break;
                }
            });

            if (entry is null) {
                emb.AddLocalizedTitleField("evt-gld-emoji-bef", e.EmojisBefore?.Count, inline: true);
                emb.AddLocalizedTitleField("evt-gld-emoji-aft", e.EmojisAfter?.Count, inline: true);
            }

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildIntegrationsUpdated)]
        public static async Task GuildIntegrationsUpdateEventHandlerAsync(TheGodfatherShard shard, GuildIntegrationsUpdateEventArgs e)
        {
            LogExt.Debug(shard.Id, "Integrations updated: {Guild}", e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildIntegrationsUpdated, "evt-gld-int-upd");
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleCreated)]
        public static async Task GuildRoleCreateEventHandlerAsync(TheGodfatherShard shard, GuildRoleCreateEventArgs e)
        {
            LogExt.Debug(shard.Id, "Role created: {Role} {Guild}", e.Role, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;
            
            emb.WithLocalizedTitle(DiscordEventType.GuildRoleCreated, "evt-gld-role-add", e.Role);
            emb.AddLocalizedPropertyChangeField("str-managed", false, e.Role?.IsManaged ?? false, inline: true);

            DiscordAuditLogRoleUpdateEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleCreate);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleDeleted)]
        public static async Task GuildRoleDeleteEventHandlerAsync(TheGodfatherShard shard, GuildRoleDeleteEventArgs e)
        {
            LogExt.Debug(shard.Id, "Role deleted: {Role} {Guild}", e.Role, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "evt-gld-role-del", e.Role);

            DiscordAuditLogRoleUpdateEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleDelete);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleUpdated)]
        public static async Task GuildRoleUpdateEventHandlerAsync(TheGodfatherShard shard, GuildRoleUpdateEventArgs e)
        {
            LogExt.Debug(shard.Id, "Role updated: {Role} {Guild}", e.RoleBefore, e.Guild);
            // FIXME Still no solution for Discord role position event spam...
            if (OnlyPositionUpdate(e.RoleBefore, e.RoleAfter))
                return;

            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildRoleUpdated, "evt-gld-role-upd", e.RoleBefore);

            DiscordAuditLogRoleUpdateEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleUpdate);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                emb.AddLocalizedPropertyChangeField("str-name", ent.NameChange);
                emb.AddLocalizedPropertyChangeField("str-color", ent.ColorChange);
                emb.AddLocalizedPropertyChangeField("str-hoist", ent.HoistChange);
                emb.AddLocalizedPropertyChangeField("str-mention", ent.MentionableChange);
                emb.AddLocalizedPropertyChangeField("str-pos", ent.PositionChange);
                // TODO use permissions_new once it is implemented in D#+
                emb.AddLocalizedTitleField("str-perms", ent.PermissionChange.After, inline: false);
            });

            await logService.LogAsync(e.Guild, emb);


            static bool OnlyPositionUpdate(DiscordRole roleBefore, DiscordRole roleAfter)
            {
                return roleBefore.Position != roleAfter.Position
                    && roleBefore.Color.Value == roleAfter.Color.Value
                    && roleBefore.IsHoisted == roleAfter.IsHoisted
                    && roleBefore.IsManaged == roleAfter.IsManaged
                    && roleBefore.IsMentionable == roleAfter.IsMentionable
                    && roleBefore.Name == roleAfter.Name
                    && roleBefore.Permissions == roleAfter.Permissions;
            }
        }

        [AsyncEventListener(DiscordEventType.GuildUpdated)]
        public static async Task GuildUpdateEventHandlerAsync(TheGodfatherShard shard, GuildUpdateEventArgs e)
        {
            LogExt.Debug(shard.Id, "Guild updated: {Guild}", e.GuildBefore);
            if (!LoggingService.IsLogEnabledForGuild(shard, e.GuildBefore.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, "evt-gld-upd");

            DiscordAuditLogGuildEntry? entry = await e.GuildAfter.GetLatestAuditLogEntryAsync<DiscordAuditLogGuildEntry>(AuditLogActionType.GuildUpdate);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                emb.AddLocalizedPropertyChangeField("str-name", ent.NameChange);
                emb.AddLocalizedPropertyChangeField("str-afkchn", ent.AfkChannelChange);
                emb.AddLocalizedPropertyChangeField("str-embchn", ent.EmbedChannelChange);
                emb.AddLocalizedPropertyChangeField("str-nsfw", ent.ExplicitContentFilterChange);
                emb.AddLocalizedPropertyChangeField("str-icon", ent.IconChange);
                emb.AddLocalizedPropertyChangeField("str-mfa", ent.MfaLevelChange);
                emb.AddLocalizedPropertyChangeField("str-notifications", ent.NotificationSettingsChange);
                emb.AddLocalizedPropertyChangeField("str-owner", ent.OwnerChange);
                emb.AddLocalizedPropertyChangeField("str-region", ent.RegionChange);
                emb.AddLocalizedPropertyChangeField("str-splash", ent.SplashChange);
                emb.AddLocalizedPropertyChangeField("str-syschn", ent.SystemChannelChange);
                emb.AddLocalizedPropertyChangeField("str-verlvl", ent.VerificationLevelChange);
            });

            await logService.LogAsync(e.GuildAfter, emb);
        }

        [AsyncEventListener(DiscordEventType.VoiceServerUpdated)]
        public static async Task GuildVoiceServerUpdateEventHandlerAsync(TheGodfatherShard shard, VoiceServerUpdateEventArgs e)
        {
            LogExt.Debug(shard.Id, "Guild voice server updated: {Endpoint} {Guild}", e.Endpoint, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.VoiceServerUpdated, "evt-gld-vs-upd", e.Endpoint);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.InviteCreated)]
        public static async Task InviteCreatedEventHandlerAsync(TheGodfatherShard shard, InviteCreateEventArgs e)
        {
            LogExt.Debug(shard.Id, "Guild invite created: {Invite} {Guild}", e.Invite, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            LocalizationService ls = shard.Services.GetRequiredService<LocalizationService>();
            emb.WithLocalizedTitle(DiscordEventType.InviteCreated, "evt-gld-inv-add", e.Channel);
            emb.AddLocalizedTitleField("str-code", e.Invite.Code, inline: true);
            emb.AddLocalizedTitleField("str-revoked", e.Invite.IsRevoked, inline: true);
            emb.AddLocalizedTitleField("str-temporary", e.Invite.IsTemporary, inline: true);
            emb.AddLocalizedTitleField("str-max-age-s", e.Invite.MaxAge, inline: true);
            emb.AddLocalizedTitleField("str-max-uses", e.Invite.MaxUses, inline: true);
            emb.WithLocalizedFooter("str-created-at", ls.GetLocalizedTime(e.Guild.Id, e.Invite.CreatedAt), e.Invite.Inviter.AvatarUrl);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.InviteDeleted)]
        public static async Task InviteDeletedEventHandlerAsync(TheGodfatherShard shard, InviteDeleteEventArgs e)
        {
            LogExt.Debug(shard.Id, "Guild invite deleted: {Invite} {Guild}", e.Invite, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            LocalizationService ls = shard.Services.GetRequiredService<LocalizationService>();
            emb.WithLocalizedTitle(DiscordEventType.InviteDeleted, "evt-gld-inv-add", e.Channel);
            emb.AddLocalizedTitleField("str-code", e.Invite.Code, inline: true);
            emb.AddLocalizedTitleField("str-revoked", e.Invite.IsRevoked, inline: true);
            emb.AddLocalizedTitleField("str-temporary", e.Invite.IsTemporary, inline: true);
            emb.AddLocalizedTitleField("str-max-age-s", e.Invite.MaxAge, inline: true);
            emb.AddLocalizedTitleField("str-max-uses", e.Invite.MaxUses, inline: true);
            emb.WithLocalizedFooter("str-created-at", ls.GetLocalizedTime(e.Guild.Id, e.Invite.CreatedAt), e.Invite.Inviter.AvatarUrl);
            await logService.LogAsync(e.Guild, emb);
        }
    }
}
