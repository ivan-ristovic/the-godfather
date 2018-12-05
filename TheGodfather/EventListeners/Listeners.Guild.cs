#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.GuildBanAdded)]
        public static async Task GuildBanEventHandlerAsync(TheGodfatherShard shard, GuildBanAddEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.KickOrBan, "Member banned");

            var entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.Ban);
            if (entry is null || !(entry is DiscordAuditLogBanEntry bentry)) {
                emb.WithDescription(e.Member?.ToString() ?? _unknown);
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.WithDescription(bentry.Target.ToString());
                emb.AddField("User responsible", bentry.UserResponsible.Mention, inline: true);
                if (!string.IsNullOrWhiteSpace(bentry.Reason))
                    emb.AddField("Reason", bentry.Reason);
                emb.WithFooter(bentry.CreationTimestamp.ToUtcTimestamp(), bentry.UserResponsible.AvatarUrl);
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.GuildBanRemoved)]
        public static async Task GuildUnbanEventHandlerAsync(TheGodfatherShard shard, GuildBanRemoveEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.KickOrBan, "Member unbanned");

            var entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.Unban);
            if (entry is null || !(entry is DiscordAuditLogBanEntry bentry)) {
                emb.WithDescription(e.Member?.ToString() ?? _unknown);
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.WithDescription(bentry.Target.ToString());
                emb.AddField("User responsible", bentry.UserResponsible.Mention, inline: true);
                if (!string.IsNullOrWhiteSpace(bentry.Reason))
                    emb.AddField("Reason", bentry.Reason);
                emb.WithFooter(bentry.CreationTimestamp.ToUtcTimestamp(), bentry.UserResponsible.AvatarUrl);
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.GuildDeleted)]
        public static async Task GuildDeleteEventHandlerAsync(TheGodfatherShard shard, GuildDeleteEventArgs e)
        {
            shard.Log(LogLevel.Info, $"| Left guild: {e.Guild.ToString()}");

            shard.SharedData.GuildConfigurations.TryRemove(e.Guild.Id, out _);
            using (DatabaseContext db = shard.Database.CreateContext()) {
                db.GuildConfig.Remove(db.GuildConfig.Where(gcfg => gcfg.GuildId == e.Guild.Id).Single());
                await db.SaveChangesAsync();
            }
        }

        [AsyncEventListener(DiscordEventType.GuildEmojisUpdated)]
        public static async Task GuildEmojisUpdateEventHandlerAsync(TheGodfatherShard shard, GuildEmojisUpdateEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Emoji, "Guild emojis updated");
            
            AuditLogActionType action;
            if (e.EmojisAfter.Count > e.EmojisBefore.Count)
                action = AuditLogActionType.EmojiCreate;
            else if (e.EmojisAfter.Count < e.EmojisBefore.Count)
                action = AuditLogActionType.EmojiDelete;
            else
                action = AuditLogActionType.EmojiUpdate;
            DiscordAuditLogEntry entry = await e.Guild.GetLatestAuditLogEntryAsync(action);

            emb.WithTitle($"Guild emoji action occured: {action.ToString()}");
            if (entry is null || !(entry is DiscordAuditLogEmojiEntry eentry)) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                emb.AddField("Emojis before", e.EmojisBefore?.Count.ToString() ?? _unknown, inline: true);
                emb.AddField("Emojis after", e.EmojisAfter?.Count.ToString() ?? _unknown, inline: true);
            } else {
                switch (action) {
                    case AuditLogActionType.EmojiCreate:
                        emb.WithDescription(eentry.Target.Name ?? _unknown);
                        emb.WithThumbnailUrl(eentry.Target.Url);
                        break;
                    case AuditLogActionType.EmojiDelete:
                        emb.WithDescription(eentry.NameChange.Before ?? _unknown);
                        break;
                    case AuditLogActionType.EmojiUpdate:
                        emb.WithDescription(eentry.Target.Name ?? _unknown);
                        if (!(eentry.NameChange is null))
                            emb.AddField("Name changes", $"{Formatter.InlineCode(eentry.NameChange.Before ?? "None")} -> {Formatter.InlineCode(eentry.NameChange.After ?? "None")}", inline: true);
                        break;
                    default:
                        break;
                }
                emb.AddField("User responsible", eentry.UserResponsible.Mention, inline: true);
                if (!string.IsNullOrWhiteSpace(eentry.Reason))
                    emb.AddField("Reason", eentry.Reason);
                emb.WithFooter(eentry.CreationTimestamp.ToUtcTimestamp(), eentry.UserResponsible.AvatarUrl);
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.GuildIntegrationsUpdated)]
        public static async Task GuildIntegrationsUpdateEventHandlerAsync(TheGodfatherShard shard, GuildIntegrationsUpdateEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Guild, "Guild integrations updated");

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.GuildRoleCreated)]
        public static async Task GuildRoleCreateEventHandlerAsync(TheGodfatherShard shard, GuildRoleCreateEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Role, "Role created", e.Role.ToString());

            var entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.RoleCreate);
            if (!(entry is null) && entry is DiscordAuditLogRoleUpdateEntry rentry) {
                emb.AddField("User responsible", rentry.UserResponsible.Mention, inline: true);
                if (!(rentry.NameChange is null))
                    emb.AddField("Name change", $"{rentry.NameChange.Before ?? _unknown} -> {rentry.NameChange.After ?? _unknown}", inline: true);
                if (!(rentry.ColorChange is null))
                    emb.AddField("Color change", $"{rentry.ColorChange.Before?.ToString() ?? _unknown} -> {rentry.ColorChange.After?.ToString() ?? _unknown}", inline: true);
                if (!(rentry.HoistChange is null))
                    emb.AddField("Hoist changed to", rentry.HoistChange.After?.ToString() ?? _unknown, inline: true);
                if (!(rentry.MentionableChange is null))
                    emb.AddField("Mentionable changed to", rentry.MentionableChange.After?.ToString() ?? _unknown, inline: true);
                if (!(rentry.PermissionChange is null))
                    emb.AddField("Permissions changed to", rentry.PermissionChange.After?.ToPermissionString() ?? _unknown, inline: true);
                if (!(rentry.PositionChange is null))
                    emb.AddField("Position changed to", rentry.PositionChange.After?.ToString() ?? _unknown, inline: true);
                if (!string.IsNullOrWhiteSpace(rentry.Reason))
                    emb.AddField("Reason", rentry.Reason);
                emb.WithFooter(rentry.CreationTimestamp.ToUtcTimestamp(), rentry.UserResponsible.AvatarUrl);
            } else {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.GuildRoleDeleted)]
        public static async Task GuildRoleDeleteEventHandlerAsync(TheGodfatherShard shard, GuildRoleDeleteEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Role, "Role deleted", e.Role.ToString());

            var entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.RoleDelete);
            if (!(entry is null) && entry is DiscordAuditLogRoleUpdateEntry rentry) {
                emb.AddField("User responsible", rentry.UserResponsible.Mention, inline: true); if (!string.IsNullOrWhiteSpace(rentry.Reason))
                    emb.AddField("Reason", rentry.Reason);
                emb.WithFooter(rentry.CreationTimestamp.ToUtcTimestamp(), rentry.UserResponsible.AvatarUrl);
            } else {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.GuildRoleUpdated)]
        public static async Task GuildRoleUpdateEventHandlerAsync(TheGodfatherShard shard, GuildRoleUpdateEventArgs e)
        {
            if (e.RoleBefore.Position != e.RoleAfter.Position)
                return;

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Role, "Role updated");
            var entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.RoleUpdate);
            if (!(entry is null) && entry is DiscordAuditLogRoleUpdateEntry rentry) {
                emb.WithDescription(rentry.Target.Id.ToString());
                emb.AddField("User responsible", rentry.UserResponsible.Mention, inline: true);
                if (!(rentry.NameChange is null))
                    emb.AddField("Name change", $"{rentry.NameChange.Before ?? _unknown} -> {rentry.NameChange.After ?? _unknown}", inline: true);
                if (!(rentry.ColorChange is null))
                    emb.AddField("Color changed", $"{rentry.ColorChange.Before?.ToString() ?? _unknown} -> {rentry.ColorChange.After?.ToString() ?? _unknown}", inline: true);
                if (!(rentry.HoistChange is null))
                    emb.AddField("Hoist", rentry.HoistChange.After?.ToString() ?? _unknown, inline: true);
                if (!(rentry.MentionableChange is null))
                    emb.AddField("Mentionable", rentry.MentionableChange.After?.ToString() ?? _unknown, inline: true);
                if (!(rentry.PermissionChange is null))
                    emb.AddField("Permissions changed to", rentry.PermissionChange.After?.ToPermissionString() ?? _unknown, inline: true);
                if (!(rentry.PositionChange is null))
                    emb.AddField("Position changed to", rentry.PositionChange.After?.ToString() ?? _unknown, inline: true);
                if (!string.IsNullOrWhiteSpace(rentry.Reason))
                    emb.AddField("Reason", rentry.Reason);
                emb.WithFooter(rentry.CreationTimestamp.ToUtcTimestamp(), rentry.UserResponsible.AvatarUrl);
            } else {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                emb.AddField("Role", e.RoleBefore?.ToString() ?? _unknown);
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.GuildUpdated)]
        public static async Task GuildUpdateEventHandlerAsync(TheGodfatherShard shard, GuildUpdateEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.GuildAfter);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Role, "Guild settings updated");

            var entry = await e.GuildAfter.GetLatestAuditLogEntryAsync(AuditLogActionType.GuildUpdate);
            if (!(entry is null) && entry is DiscordAuditLogGuildEntry gentry) {
                emb.AddField("User responsible", gentry.UserResponsible.Mention, inline: true);
                if (!(gentry.NameChange is null))
                    emb.AddField("Name change", $"{gentry.NameChange.Before ?? _unknown} -> {gentry.NameChange.After ?? _unknown}", inline: true);
                if (!(gentry.AfkChannelChange is null))
                    emb.AddField("AFK channel changed to", gentry.AfkChannelChange.After?.ToString() ?? _unknown, inline: true);
                if (!(gentry.EmbedChannelChange is null))
                    emb.AddField("Embed channel changed to", gentry.EmbedChannelChange.After?.ToString() ?? _unknown, inline: true);
                if (!(gentry.IconChange is null))
                    emb.AddField("Icon changed to", gentry.IconChange.After ?? _unknown, inline: true);
                if (!(gentry.NotificationSettingsChange is null))
                    emb.AddField("Notifications changed to", gentry.NotificationSettingsChange.After.HasFlag(DefaultMessageNotifications.AllMessages) ? "All messages" : "Mentions only", inline: true);
                if (!(gentry.OwnerChange is null))
                    emb.AddField("Owner changed to", gentry.OwnerChange.After?.ToString() ?? _unknown, inline: true);
                if (!string.IsNullOrWhiteSpace(gentry.Reason))
                    emb.AddField("Reason", gentry.Reason);
                emb.WithFooter(gentry.CreationTimestamp.ToUtcTimestamp(), gentry.UserResponsible.AvatarUrl);
            } else {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.WebhooksUpdated)]
        public static async Task WebhooksUpdateEventHandlerAsync(TheGodfatherShard shard, WebhooksUpdateEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Guild, "Webhooks updated", $"For {e.Channel.ToString()}");

            await logchn.SendMessageAsync(embed: emb.Build());
        }
    }
}
