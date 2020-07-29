using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.GuildBanAdded)]
        public static async Task GuildBanEventHandlerAsync(TheGodfatherShard shard, GuildBanAddEventArgs e)
        {
            if (!IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
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
            if (!IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
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
            LogExt.Information(shard.Id, "Left {Guild}", e.Guild);
            return shard.Services.GetService<GuildConfigService>().UnregisterGuildAsync(e.Guild.Id);
        }

        [AsyncEventListener(DiscordEventType.GuildEmojisUpdated)]
        public static async Task GuildEmojisUpdateEventHandlerAsync(TheGodfatherShard shard, GuildEmojisUpdateEventArgs e)
        {
            if (!IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
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
            if (!IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildIntegrationsUpdated, "evt-gld-int-upd");
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleCreated)]
        public static async Task GuildRoleCreateEventHandlerAsync(TheGodfatherShard shard, GuildRoleCreateEventArgs e)
        {
            if (!IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
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
            if (!IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.GuildRoleDeleted, "evt-gld-role-del", e.Role);

            DiscordAuditLogRoleUpdateEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleDelete);
            emb.AddFieldsFromAuditLogEntry(entry);
            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleUpdated)]
        public static async Task GuildRoleUpdateEventHandlerAsync(TheGodfatherShard shard, GuildRoleUpdateEventArgs e)
        {
            // FIXME Still no solution for Discord role position event spam...
            if (OnlyPositionUpdate(e.RoleBefore, e.RoleAfter))
                return;

            if (!IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
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
                if (roleBefore.Position == roleAfter.Position)
                    return false;

                return roleBefore.Color.Value == roleAfter.Color.Value
                    && roleBefore.IsHoisted == roleAfter.IsHoisted
                    && roleBefore.IsManaged == roleAfter.IsManaged
                    && roleBefore.IsMentionable == roleAfter.IsMentionable
                    && roleBefore.Name == roleAfter.Name
                    && roleBefore.Permissions == roleAfter.Permissions
                    ;
            }
        }

        [AsyncEventListener(DiscordEventType.GuildUpdated)]
        public static async Task GuildUpdateEventHandlerAsync(TheGodfatherShard shard, GuildUpdateEventArgs e)
        {
            if (!IsLogEnabledForGuild(shard, e.GuildBefore.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
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

        [AsyncEventListener(DiscordEventType.WebhooksUpdated)]
        public static async Task WebhooksUpdateEventHandlerAsync(TheGodfatherShard shard, WebhooksUpdateEventArgs e)
        {
            if (!IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.WebhooksUpdated, "evt-gld-wh-upd", e.Channel);

            DiscordAuditLogWebhookEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogWebhookEntry>(AuditLogActionType.WebhookUpdate);
            emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
                emb.WithDescription(ent.Target);
                emb.AddLocalizedPropertyChangeField("str-name", ent.NameChange);
                emb.AddLocalizedPropertyChangeField("str-ahash", ent.AvatarHashChange);
                emb.AddLocalizedPropertyChangeField("str-chn", ent.ChannelChange);
                emb.AddLocalizedPropertyChangeField("str-type", ent.TypeChange);
            });

            await logService.LogAsync(e.Guild, emb);
        }
    }
}
