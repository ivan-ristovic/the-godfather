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
            if (shard.Services.GetService<GuildConfigService>().GetLogChannelForGuild(e.Guild) is null)
                return;

            var emb = new DiscordLogEmbedBuilder("Member BANNED", null, DiscordEventType.GuildBanAdded);

            DiscordAuditLogBanEntry entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogBanEntry>(AuditLogActionType.Ban);
            if (entry is null) {
                emb.WithDescription(e.Member?.ToString());
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.WithDescription(entry.Target.ToString());
                emb.AddInvocationFields(entry.UserResponsible);
                emb.AddField("Reason", entry.Reason, null);
                emb.WithTimestampFooter(entry.CreationTimestamp, entry.UserResponsible.AvatarUrl);
            }

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildBanRemoved)]
        public static async Task GuildUnbanEventHandlerAsync(TheGodfatherShard shard, GuildBanRemoveEventArgs e)
        {
            if (shard.Services.GetService<GuildConfigService>().GetLogChannelForGuild(e.Guild) is null)
                return;

            var emb = new DiscordLogEmbedBuilder("Member unbanned", null, DiscordEventType.GuildBanRemoved);

            DiscordAuditLogBanEntry entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogBanEntry>(AuditLogActionType.Unban);
            if (entry is null) {
                emb.WithDescription(e.Member?.ToString());
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.WithDescription(entry.Target.ToString());
                emb.AddInvocationFields(entry.UserResponsible);
                emb.AddField("Reason", entry.Reason, null);
                emb.WithTimestampFooter(entry.CreationTimestamp, entry.UserResponsible.AvatarUrl);
            }

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
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
            if (shard.Services.GetService<GuildConfigService>().GetLogChannelForGuild(e.Guild) is null)
                return;

            var emb = new DiscordLogEmbedBuilder("Emojis updated", null, DiscordEventType.GuildEmojisUpdated);

            AuditLogActionType action;
            if (e.EmojisAfter.Count > e.EmojisBefore.Count)
                action = AuditLogActionType.EmojiCreate;
            else if (e.EmojisAfter.Count < e.EmojisBefore.Count)
                action = AuditLogActionType.EmojiDelete;
            else
                action = AuditLogActionType.EmojiUpdate;
            DiscordAuditLogEmojiEntry entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogEmojiEntry>(action);

            emb.WithDescription($"Action: {action.ToString()}");
            if (entry is null) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                emb.AddField("Emojis before", e.EmojisBefore?.Count.ToString(), inline: true);
                emb.AddField("Emojis after", e.EmojisAfter?.Count.ToString(), inline: true);
            } else {
                switch (action) {
                    case AuditLogActionType.EmojiCreate:
                        emb.AddField("Created", entry.Target.Name);
                        emb.WithThumbnail(entry.Target.Url);
                        break;
                    case AuditLogActionType.EmojiDelete:
                        emb.AddField("Deleted", entry.NameChange.Before);
                        break;
                    case AuditLogActionType.EmojiUpdate:
                        emb.AddField("Renamed", entry.Target.Name);
                        emb.AddPropertyChangeField("Name changed", entry.NameChange);
                        break;
                    default:
                        break;
                }
                emb.AddInvocationFields(entry.UserResponsible);
                emb.AddField("Reason", entry.Reason, null);
                emb.WithTimestampFooter(entry.CreationTimestamp, entry.UserResponsible.AvatarUrl);
            }

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildIntegrationsUpdated)]
        public static async Task GuildIntegrationsUpdateEventHandlerAsync(TheGodfatherShard shard, GuildIntegrationsUpdateEventArgs e)
        {
            if (shard.Services.GetService<GuildConfigService>().GetLogChannelForGuild(e.Guild) is null)
                return;

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, new DiscordLogEmbedBuilder("Integrations updated", null, DiscordEventType.GuildIntegrationsUpdated));
        }

        [AsyncEventListener(DiscordEventType.GuildRoleCreated)]
        public static async Task GuildRoleCreateEventHandlerAsync(TheGodfatherShard shard, GuildRoleCreateEventArgs e)
        {
            if (shard.Services.GetService<GuildConfigService>().GetLogChannelForGuild(e.Guild) is null)
                return;

            var emb = new DiscordLogEmbedBuilder("Role created", e.Role?.ToString(), DiscordEventType.GuildRoleCreated);

            DiscordAuditLogRoleUpdateEntry entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleCreate);
            if (entry is null) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.AddInvocationFields(entry.UserResponsible);
                emb.AddField("User responsible", entry.UserResponsible.Mention, inline: true);
                emb.AddPropertyChangeField("Name changed", entry.NameChange);
                emb.AddPropertyChangeField("Color changed", entry.ColorChange);
                emb.AddField("Hoist changed to", entry.HoistChange.After?.ToString(), null, inline: true);
                emb.AddField("Mentionable changed to", entry.MentionableChange.After?.ToString(), null, inline: true);
                emb.AddField("Permissions changed to", entry.PermissionChange.After?.ToPermissionString(), null, inline: true);
                emb.AddField("Position changed to", entry.PositionChange.After?.ToString(), null, inline: true);
                emb.AddField("Reason", entry.Reason, null);
                emb.WithTimestampFooter(entry.CreationTimestamp, entry.UserResponsible.AvatarUrl);
            }

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleDeleted)]
        public static async Task GuildRoleDeleteEventHandlerAsync(TheGodfatherShard shard, GuildRoleDeleteEventArgs e)
        {
            if (shard.Services.GetService<GuildConfigService>().GetLogChannelForGuild(e.Guild) is null)
                return;

            var emb = new DiscordLogEmbedBuilder("Role deleted", e.Role?.ToString(), DiscordEventType.GuildRoleDeleted);

            DiscordAuditLogRoleUpdateEntry entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleDelete);
            if (entry is null) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.AddInvocationFields(entry.UserResponsible);
                emb.AddField("Reason", entry.Reason, null);
                emb.WithTimestampFooter(entry.CreationTimestamp, entry.UserResponsible.AvatarUrl);
            }

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildRoleUpdated)]
        public static async Task GuildRoleUpdateEventHandlerAsync(TheGodfatherShard shard, GuildRoleUpdateEventArgs e)
        {
            if (e.RoleBefore.Position != e.RoleAfter.Position)
                return;

            if (shard.Services.GetService<GuildConfigService>().GetLogChannelForGuild(e.Guild) is null)
                return;

            var emb = new DiscordLogEmbedBuilder("Role updated", e.RoleBefore?.ToString(), DiscordEventType.GuildRoleUpdated);

            DiscordAuditLogRoleUpdateEntry entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogRoleUpdateEntry>(AuditLogActionType.RoleUpdate);
            if (entry is null) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.AddInvocationFields(entry.UserResponsible);
                emb.AddPropertyChangeField("Name changed", entry.NameChange);
                emb.AddPropertyChangeField("Color changed", entry.ColorChange);
                emb.AddField("Hoist changed to", entry.HoistChange.After?.ToString(), null, inline: true);
                emb.AddField("Mentionable changed to", entry.MentionableChange.After?.ToString(), null, inline: true);
                emb.AddField("Permissions changed to", entry.PermissionChange.After?.ToPermissionString(), null, inline: true);
                emb.AddField("Position changed to", entry.PositionChange.After?.ToString(), null, inline: true);
                emb.AddField("Reason", entry.Reason, null);
                emb.WithTimestampFooter(entry.CreationTimestamp, entry.UserResponsible.AvatarUrl);
            }

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.GuildUpdated)]
        public static async Task GuildUpdateEventHandlerAsync(TheGodfatherShard shard, GuildUpdateEventArgs e)
        {
            if (shard.Services.GetService<GuildConfigService>().GetLogChannelForGuild(e.GuildAfter) is null)
                return;

            var emb = new DiscordLogEmbedBuilder("Guild updated", null, DiscordEventType.GuildRoleCreated);

            DiscordAuditLogGuildEntry entry = await e.GuildAfter.GetLatestAuditLogEntryAsync<DiscordAuditLogGuildEntry>(AuditLogActionType.GuildUpdate);
            if (entry is null) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.AddField("User responsible", entry.UserResponsible.Mention, inline: true);
                emb.AddPropertyChangeField("Name changed", entry.NameChange);
                emb.AddPropertyChangeField("AFK channel changed", entry.AfkChannelChange);
                emb.AddPropertyChangeField("Embed channel changed", entry.EmbedChannelChange);
                emb.AddPropertyChangeField("NSFW filter changed", entry.ExplicitContentFilterChange);
                emb.AddPropertyChangeField("Icon changed", entry.IconChange);
                emb.AddPropertyChangeField("MFA level changed", entry.MfaLevelChange);
                emb.AddPropertyChangeField("Notifications changed", entry.NotificationSettingsChange);
                emb.AddPropertyChangeField("Owner changed", entry.OwnerChange);
                emb.AddPropertyChangeField("Splash changed", entry.SplashChange);
                emb.AddPropertyChangeField("System channel changed", entry.SystemChannelChange);
                emb.AddField("Reason", entry.Reason, null);
                emb.WithTimestampFooter(entry.CreationTimestamp, entry.UserResponsible.AvatarUrl);
            }

            await shard.Services.GetService<LoggingService>().LogAsync(e.GuildAfter, emb);
        }

        [AsyncEventListener(DiscordEventType.WebhooksUpdated)]
        public static async Task WebhooksUpdateEventHandlerAsync(TheGodfatherShard shard, WebhooksUpdateEventArgs e)
        {
            if (shard.Services.GetService<GuildConfigService>().GetLogChannelForGuild(e.Guild) is null)
                return;

            var emb = new DiscordLogEmbedBuilder("Webhooks updated", e.Channel.ToString(), DiscordEventType.WebhooksUpdated);

            // TODO get webhook audit log entry

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
        }
    }
}
