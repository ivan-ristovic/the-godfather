#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
#endregion

namespace TheGodfather.EventListeners
{
    internal static class GuildListeners
    {
        [AsyncExecuter(EventTypes.GuildBanAdded)]
        public static async Task Client_GuildBanAdded(TheGodfatherShard shard, GuildBanAddEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Member banned",
                    Color = DiscordColor.DarkRed
                };

                var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.Ban)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogBanEntry bentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Member", e.Member?.ToString() ?? "<unknown>");
                } else {
                    emb.WithDescription(bentry.Target.ToString());
                    emb.AddField("User responsible", bentry.UserResponsible.Mention, inline: true);
                    if (!string.IsNullOrWhiteSpace(bentry.Reason))
                        emb.AddField("Reason", bentry.Reason);
                    emb.WithFooter($"At {bentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", bentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.GuildBanRemoved)]
        public static async Task Client_GuildBanRemoved(TheGodfatherShard shard, GuildBanRemoveEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Member unbanned",
                    Color = DiscordColor.DarkRed
                };

                var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.Unban)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogBanEntry bentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Member", e.Member?.ToString() ?? "<unknown>");
                } else {
                    emb.WithDescription(bentry.Target.ToString());
                    emb.AddField("User responsible", bentry.UserResponsible.Mention, inline: true);
                    if (!string.IsNullOrWhiteSpace(bentry.Reason))
                        emb.AddField("Reason", bentry.Reason);
                    emb.WithFooter($"At {bentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", bentry.UserResponsible.AvatarUrl);
                }
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.GuildDeleted)]
        public static async Task Client_GuildDeleted(TheGodfatherShard shard, GuildDeleteEventArgs e)
        {
            shard.Log(LogLevel.Info, $"Left guild: {e.Guild.ToString()}");

            await shard.Database.UnregisterGuildAsync(e.Guild.Id)
                .ConfigureAwait(false);
            shard.Shared.GuildConfigurations.TryRemove(e.Guild.Id, out _);
        }

        [AsyncExecuter(EventTypes.GuildEmojisUpdated)]
        public static async Task Client_GuildEmojisUpdated(TheGodfatherShard shard, GuildEmojisUpdateEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Color = DiscordColor.Gold
                };

                DiscordAuditLogEntry entry = null;
                if (e.EmojisAfter.Count > e.EmojisBefore.Count)
                    entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.EmojiCreate).ConfigureAwait(false);
                else if (e.EmojisAfter.Count < e.EmojisBefore.Count)
                    entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.EmojiDelete).ConfigureAwait(false);
                else
                    entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.EmojiUpdate).ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogEmojiEntry eentry)) {
                    emb.WithTitle($"Guild emojis updated");
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Emojis before", e.EmojisBefore?.Count.ToString() ?? "<unknown>", inline: true);
                    emb.AddField("Emojis after", e.EmojisAfter?.Count.ToString() ?? "<unknown>", inline: true);
                } else {
                    emb.WithTitle($"Guild emoji acton occured: {eentry.ActionCategory.ToString()}");
                    emb.WithDescription(eentry.Target?.ToString() ?? "No description provided");
                    emb.AddField("User responsible", eentry.UserResponsible.Mention, inline: true);
                    if (eentry.NameChange != null)
                        emb.AddField("Name changes", $"{eentry.NameChange.Before ?? "None"} -> {eentry.NameChange.After ?? "None"}", inline: true);
                    if (!string.IsNullOrWhiteSpace(eentry.Reason))
                        emb.AddField("Reason", eentry.Reason);
                    emb.WithFooter($"At {eentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", eentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.GuildIntegrationsUpdated)]
        public static async Task Client_GuildIntegrationsUpdated(TheGodfatherShard shard, GuildIntegrationsUpdateEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Guild integrations updated",
                    Color = DiscordColor.DarkGreen
                };
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.GuildRoleCreated)]
        public static async Task Client_GuildRoleCreated(TheGodfatherShard shard, GuildRoleCreateEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Role created",
                    Description = e.Role.ToString(),
                    Color = DiscordColor.Magenta,
                };

                var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.RoleCreate)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogRoleUpdateEntry rentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                } else {
                    emb.AddField("User responsible", rentry.UserResponsible.Mention, inline: true);
                    if (rentry.NameChange != null)
                        emb.AddField("Name change", $"{rentry.NameChange.Before ?? "unknown"} -> {rentry.NameChange.After ?? "unknown"}", inline: true);
                    if (rentry.ColorChange != null)
                        emb.AddField("Color changed", $"{rentry.ColorChange.Before?.ToString() ?? "unknown"} -> {rentry.ColorChange.After?.ToString() ?? "unknown"}", inline: true);
                    if (rentry.HoistChange != null)
                        emb.AddField("Hoist", rentry.HoistChange.After?.ToString() ?? "unknown", inline: true);
                    if (rentry.MentionableChange != null)
                        emb.AddField("Mentionable", rentry.MentionableChange.After?.ToString() ?? "unknown", inline: true);
                    if (rentry.PermissionChange != null)
                        emb.AddField("Permissions changed to", rentry.PermissionChange.After?.ToPermissionString() ?? "unknown", inline: true);
                    if (rentry.PositionChange != null)
                        emb.AddField("Position changed to", rentry.PositionChange.After?.ToString() ?? "unknown", inline: true);
                    if (!string.IsNullOrWhiteSpace(rentry.Reason))
                        emb.AddField("Reason", rentry.Reason);
                    emb.WithFooter($"At {rentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", rentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.GuildRoleDeleted)]
        public static async Task Client_GuildRoleDeleted(TheGodfatherShard shard, GuildRoleDeleteEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Role deleted",
                    Description = e.Role.ToString(),
                    Color = DiscordColor.Magenta,
                };

                var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.RoleDelete)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogRoleUpdateEntry rentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                } else {
                    emb.AddField("User responsible", rentry.UserResponsible.Mention, inline: true);
                    if (rentry.NameChange != null)
                        emb.AddField("Name change", $"{rentry.NameChange.Before ?? "unknown"} -> {rentry.NameChange.After ?? "unknown"}", inline: true);
                    if (rentry.ColorChange != null)
                        emb.AddField("Color changed", $"{rentry.ColorChange.Before?.ToString() ?? "unknown"} -> {rentry.ColorChange.After?.ToString() ?? "unknown"}", inline: true);
                    if (rentry.HoistChange != null)
                        emb.AddField("Hoist", rentry.HoistChange.After?.ToString() ?? "unknown", inline: true);
                    if (rentry.MentionableChange != null)
                        emb.AddField("Mentionable", rentry.MentionableChange.After?.ToString() ?? "unknown", inline: true);
                    if (rentry.PermissionChange != null)
                        emb.AddField("Permissions changed to", rentry.PermissionChange.After?.ToPermissionString() ?? "unknown", inline: true);
                    if (rentry.PositionChange != null)
                        emb.AddField("Position changed to", rentry.PositionChange.After?.ToString() ?? "unknown", inline: true);
                    if (!string.IsNullOrWhiteSpace(rentry.Reason))
                        emb.AddField("Reason", rentry.Reason);
                    emb.WithFooter($"At {rentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", rentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.GuildRoleUpdated)]
        public static async Task Client_GuildRoleUpdated(TheGodfatherShard shard, GuildRoleUpdateEventArgs e)
        {
            if (e.RoleBefore.Position != e.RoleAfter.Position)
                return;

            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Role updated",
                    Color = DiscordColor.Magenta,
                };

                var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.RoleUpdate)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogRoleUpdateEntry rentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Role", e.RoleBefore?.ToString() ?? "<unknown>");
                } else {
                    emb.WithDescription(rentry.Target.ToString());
                    emb.AddField("User responsible", rentry.UserResponsible.Mention, inline: true);
                    if (rentry.NameChange != null)
                        emb.AddField("Name change", $"{rentry.NameChange.Before ?? "unknown"} -> {rentry.NameChange.After ?? "unknown"}", inline: true);
                    if (rentry.ColorChange != null)
                        emb.AddField("Color changed", $"{rentry.ColorChange.Before?.ToString() ?? "unknown"} -> {rentry.ColorChange.After?.ToString() ?? "unknown"}", inline: true);
                    if (rentry.HoistChange != null)
                        emb.AddField("Hoist", rentry.HoistChange.After?.ToString() ?? "unknown", inline: true);
                    if (rentry.MentionableChange != null)
                        emb.AddField("Mentionable", rentry.MentionableChange.After?.ToString() ?? "unknown", inline: true);
                    if (rentry.PermissionChange != null)
                        emb.AddField("Permissions changed to", rentry.PermissionChange.After?.ToPermissionString() ?? "unknown", inline: true);
                    if (rentry.PositionChange != null)
                        emb.AddField("Position changed to", rentry.PositionChange.After?.ToString() ?? "unknown", inline: true);
                    if (!string.IsNullOrWhiteSpace(rentry.Reason))
                        emb.AddField("Reason", rentry.Reason);
                    emb.WithFooter($"At {rentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", rentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.GuildUpdated)]
        public static async Task Client_GuildUpdated(TheGodfatherShard shard, GuildUpdateEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Guild settings updated",
                    Color = DiscordColor.Magenta,
                };

                var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.GuildUpdate)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogGuildEntry gentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                } else {
                    emb.AddField("User responsible", gentry.UserResponsible.Mention, inline: true);
                    if (gentry.NameChange != null)
                        emb.AddField("Name change", $"{gentry.NameChange.Before ?? "unknown"} -> {gentry.NameChange.After ?? "unknown"}", inline: true);
                    if (gentry.AfkChannelChange != null)
                        emb.AddField("AFK channel changed to", gentry.AfkChannelChange.After?.ToString() ?? "unknown", inline: true);
                    if (gentry.EmbedChannelChange != null)
                        emb.AddField("Embed channel changed to", gentry.EmbedChannelChange.After?.ToString() ?? "unknown", inline: true);
                    if (gentry.IconChange != null)
                        emb.AddField("Icon changed to", gentry.IconChange.After ?? "unknown", inline: true);
                    if (gentry.NotificationSettingsChange != null)
                        emb.AddField("Notifications changed to", gentry.NotificationSettingsChange.After.HasFlag(DefaultMessageNotifications.AllMessages) ? "All messages" : "Mentions only", inline: true);
                    if (gentry.OwnerChange != null)
                        emb.AddField("Owner changed to", gentry.OwnerChange.After?.ToString() ?? "unknown", inline: true);
                    if (!string.IsNullOrWhiteSpace(gentry.Reason))
                        emb.AddField("Reason", gentry.Reason);
                    emb.WithFooter($"At {gentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", gentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.VoiceServerUpdated)]
        public static async Task Client_VoiceServerUpdated(TheGodfatherShard shard, VoiceServerUpdateEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Voice server updated",
                    Color = DiscordColor.DarkGray
                };
                emb.AddField("Endpoint", Formatter.Bold(e.Endpoint));

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.WebhooksUpdated)]
        public static async Task Client_WebhooksUpdated(TheGodfatherShard shard, WebhooksUpdateEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendMessageAsync(embed: new DiscordEmbedBuilder() {
                    Title = "Webhooks updated",
                    Description = $"For {e.Channel.ToString()}",
                    Color = DiscordColor.DarkGray
                }.Build()).ConfigureAwait(false);
            }
        }
    }
}
