#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.ChannelCreated)]
        public static async Task ChannelCreateEventHandlerAsync(TheGodfatherShard shard, ChannelCreateEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn == null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Channel, "Channel created", e.Channel.ToString());

            var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.ChannelCreate);
            if (entry == null || !(entry is DiscordAuditLogChannelEntry centry)) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.AddField("User responsible", centry.UserResponsible?.Mention ?? _unknown, inline: true);
                emb.AddField("Channel type", centry.Target?.Type.ToString() ?? _unknown, inline: true);
                if (!string.IsNullOrWhiteSpace(centry.Reason))
                    emb.AddField("Reason", centry.Reason);
                emb.WithFooter(centry.CreationTimestamp.ToUtcTimestamp(), centry.UserResponsible.AvatarUrl);
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.ChannelDeleted)]
        public static async Task ChannelDeleteEventHandlerAsync(TheGodfatherShard shard, ChannelDeleteEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn == null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Channel, "Channel deleted", e.Channel.ToString());

            emb.AddField("Channel type", e.Channel.Type.ToString() ?? _unknown, inline: true);

            var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.ChannelDelete);
            if (entry == null || !(entry is DiscordAuditLogChannelEntry centry)) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.AddField("User responsible", centry.UserResponsible?.Mention ?? _unknown, inline: true);
                if (!string.IsNullOrWhiteSpace(centry.Reason))
                    emb.AddField("Reason", centry.Reason);
                emb.WithFooter(centry.CreationTimestamp.ToUtcTimestamp(), centry.UserResponsible.AvatarUrl);
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.ChannelPinsUpdated)]
        public static async Task ChannelPinsUpdateEventHandlerAsync(TheGodfatherShard shard, ChannelPinsUpdateEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Channel.Guild);
            if (logchn == null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Channel, "Channel pin added", e.Channel.ToString());

            var pinned = await e.Channel.GetPinnedMessagesAsync();
            if (pinned.Any()) {
                string content = string.IsNullOrWhiteSpace(pinned.First().Content) ? "<embedded message>" : pinned.First().Content;
                emb.AddField("Content", Formatter.BlockCode(Formatter.Sanitize(content)));
            }
            if (e.LastPinTimestamp != null)
                emb.AddField("Last pin timestamp", e.LastPinTimestamp.Value.ToUtcTimestamp());

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.ChannelUpdated)]
        public static async Task ChannelUpdateEventHandlerAsync(TheGodfatherShard shard, ChannelUpdateEventArgs e)
        {
            if (e.ChannelBefore.Position != e.ChannelAfter.Position)
                return;

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn == null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Channel, "Channel updated");
            DiscordAuditLogEntry entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.ChannelUpdate);            
            if (entry != null && entry is DiscordAuditLogChannelEntry centry) {     // Regular update
                emb.WithDescription(centry.Target.ToString());
                emb.AddField("User responsible", centry.UserResponsible?.Mention ?? _unknown, inline: true);
                if (centry.BitrateChange != null)
                    emb.AddField("Bitrate changed to", centry.BitrateChange.After.ToString(), inline: true);
                if (centry.NameChange != null)
                    emb.AddField("Name changed to", centry.NameChange.After, inline: true);
                if (centry.NsfwChange != null)
                    emb.AddField("NSFW flag changed to", centry.NsfwChange.After.Value.ToString(), inline: true);
                if (centry.OverwriteChange != null)
                    emb.AddField("Permissions overwrites changed", $"{centry.OverwriteChange.After.Count} overwrites after changes");
                if (centry.TopicChange != null) {
                    string ptopic = Formatter.BlockCode(Formatter.Sanitize(string.IsNullOrWhiteSpace(centry.TopicChange.Before) ? " " : centry.TopicChange.Before));
                    string ctopic = Formatter.BlockCode(Formatter.Sanitize(string.IsNullOrWhiteSpace(centry.TopicChange.After) ? " " : centry.TopicChange.After));
                    emb.AddField("Topic changed", $"From:{ptopic}\nTo:{ctopic}");
                }
                if (centry.TypeChange != null)
                    emb.AddField("Type changed to", centry.TypeChange.After.Value.ToString());
                if (!string.IsNullOrWhiteSpace(centry.Reason))
                    emb.AddField("Reason", centry.Reason);
                emb.WithFooter($"At {centry.CreationTimestamp.ToUniversalTime().ToString()} UTC", centry.UserResponsible.AvatarUrl);
            } else {    // Overwrite update
                AuditLogActionType type;
                if (e.ChannelBefore.PermissionOverwrites.Count > e.ChannelAfter.PermissionOverwrites.Count) {
                    type = AuditLogActionType.OverwriteCreate;
                    entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.OverwriteCreate);
                } else if (e.ChannelBefore.PermissionOverwrites.Count < e.ChannelAfter.PermissionOverwrites.Count) {
                    type = AuditLogActionType.OverwriteDelete;
                    entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.OverwriteDelete);
                } else {
                    if (e.ChannelBefore.PermissionOverwrites.Zip(e.ChannelAfter.PermissionOverwrites, (o1, o2) => o1.Allowed != o1.Allowed && o2.Denied != o2.Denied).Any()) {
                        type = AuditLogActionType.OverwriteUpdate;
                        entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.OverwriteUpdate);
                    } else {
                        type = AuditLogActionType.ChannelUpdate;
                        entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.ChannelUpdate);
                    }
                }

                if (entry != null && entry is DiscordAuditLogOverwriteEntry owentry) {
                    emb.WithDescription($"{owentry.Channel.ToString()} ({type})");
                    emb.AddField("User responsible", owentry.UserResponsible?.Mention ?? _unknown, inline: true);

                    DiscordUser member = null;
                    DiscordRole role = null;

                    if (owentry.Target != null) {
                        try {
                            bool isMemberUpdated = owentry.Target.Type.HasFlag(OverwriteType.Member);
                            if (isMemberUpdated)
                                member = await e.Client.GetUserAsync(owentry.Target.Id).ConfigureAwait(false);
                            else
                                role = e.Guild.GetRole(owentry.Target.Id);
                            emb.AddField("Target", isMemberUpdated ? member.ToString() : role.ToString(), inline: true);
                            if (owentry.AllowChange != null)
                                emb.AddField("Allowed", $"{owentry.Target.Allowed.ToPermissionString() ?? _unknown}", inline: true);
                            if (owentry.DenyChange != null)
                                emb.AddField("Denied", $"{owentry.Target.Denied.ToPermissionString() ?? _unknown}", inline: true);
                        } catch {

                        }
                    } else {
                        if (owentry.TargetIdChange?.After != null)
                            emb.AddField("Target ID", owentry.TargetIdChange?.After?.ToString() ?? _unknown, inline: true);
                    }

                    if (!string.IsNullOrWhiteSpace(owentry.Reason))
                        emb.AddField("Reason", owentry.Reason);
                    emb.WithFooter(owentry.CreationTimestamp.ToUtcTimestamp(), owentry.UserResponsible.AvatarUrl);
                } else {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Channel", e.ChannelBefore?.ToString() ?? _unknown);
                }
            }

            await logchn.SendMessageAsync(embed: emb.Build());
        }
    }
}
