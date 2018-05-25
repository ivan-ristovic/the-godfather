#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Listeners
{
    internal static class ChannelListeners
    {
        [AsyncEventListener(EventTypes.ChannelCreated)]
        public static async Task Client_ChannelCreated(TheGodfatherShard shard, ChannelCreateEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Channel created",
                    Description = e.Channel.ToString(),
                    Color = DiscordColor.Aquamarine
                };

                var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.ChannelCreate)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogChannelEntry centry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                } else {
                    emb.AddField("User responsible", centry.UserResponsible.Mention, inline: true);
                    emb.AddField("Channel type", centry.Target.Type.ToString(), inline: true);
                    if (!string.IsNullOrWhiteSpace(centry.Reason))
                        emb.AddField("Reason", centry.Reason);
                    emb.WithFooter($"At {centry.CreationTimestamp.ToUniversalTime().ToString()} UTC", centry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncEventListener(EventTypes.ChannelDeleted)]
        public static async Task Client_ChannelDeleted(TheGodfatherShard shard, ChannelDeleteEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Channel deleted",
                    Description = e.Channel.ToString(),
                    Color = DiscordColor.Aquamarine
                };

                var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.ChannelCreate)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogChannelEntry centry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                } else {
                    emb.AddField("User responsible", centry.UserResponsible.Mention, inline: true);
                    emb.AddField("Channel type", centry.Target.Type.ToString(), inline: true);
                    if (!string.IsNullOrWhiteSpace(centry.Reason))
                        emb.AddField("Reason", centry.Reason);
                    emb.WithFooter($"At {centry.CreationTimestamp.ToUniversalTime().ToString()} UTC", centry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncEventListener(EventTypes.ChannelPinsUpdated)]
        public static async Task Client_ChannelPinsUpdated(TheGodfatherShard shard, ChannelPinsUpdateEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Channel.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Channel pins updated",
                    Description = e.Channel.ToString(),
                    Color = DiscordColor.Aquamarine
                };
                emb.AddField("Last pin timestamp", e.LastPinTimestamp.ToUniversalTime().ToString(), inline: true);
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncEventListener(EventTypes.ChannelUpdated)]
        public static async Task Client_ChannelUpdated(TheGodfatherShard shard, ChannelUpdateEventArgs e)
        {
            if (e.ChannelBefore.Position != e.ChannelAfter.Position)
                return;

            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Channel updated",
                    Color = DiscordColor.Aquamarine
                };

                var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.ChannelUpdate)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogChannelEntry centry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Channel", e.ChannelBefore?.ToString() ?? "<unknown>");
                } else {
                    emb.WithDescription(centry.Target.ToString());
                    emb.AddField("User responsible", centry.UserResponsible.Mention, inline: true);
                    if (centry.BitrateChange != null)
                        emb.AddField("Bitrate changed to", centry.BitrateChange.After.Value.ToString(), inline: true);
                    if (centry.NameChange != null)
                        emb.AddField("Name changed to", centry.NameChange.After, inline: true);
                    if (centry.NsfwChange != null)
                        emb.AddField("NSFW flag changed to", centry.NsfwChange.After.Value.ToString(), inline: true);
                    if (centry.OverwriteChange != null)
                        emb.AddField("Permissions overwrites changed", $"{centry.OverwriteChange.After.Count} overwrites after changes");
                    if (centry.TopicChange != null)
                        emb.AddField("Topic changed to", centry.TopicChange.After);
                    if (centry.TypeChange != null)
                        emb.AddField("Type changed to", centry.TypeChange.After.Value.ToString());
                    if (!string.IsNullOrWhiteSpace(centry.Reason))
                        emb.AddField("Reason", centry.Reason);
                    emb.WithFooter($"At {centry.CreationTimestamp.ToUniversalTime().ToString()} UTC", centry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }
    }
}
