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

namespace TheGodfather.EventListeners
{
    internal static class ChannelListeners
    {
        [AsyncExecuter(EventTypes.ChannelCreated)]
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
                    emb.AddField("User responsible", centry.UserResponsible?.Mention ?? "<unknown>", inline: true);
                    emb.AddField("Channel type", centry.Target?.Type.ToString() ?? "<unknown>", inline: true);
                    if (!string.IsNullOrWhiteSpace(centry.Reason))
                        emb.AddField("Reason", centry.Reason);
                    emb.WithFooter($"At {centry.CreationTimestamp.ToUniversalTime().ToString()} UTC", centry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.ChannelDeleted)]
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

                emb.AddField("Channel type", e.Channel.Type.ToString() ?? "<unknown>", inline: true);

                var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.ChannelDelete)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogChannelEntry centry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                } else {
                    emb.AddField("User responsible", centry.UserResponsible?.Mention ?? "<unknown>", inline: true);
                    if (!string.IsNullOrWhiteSpace(centry.Reason))
                        emb.AddField("Reason", centry.Reason);
                    emb.WithFooter($"At {centry.CreationTimestamp.ToUniversalTime().ToString()} UTC", centry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.ChannelPinsUpdated)]
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

        [AsyncExecuter(EventTypes.ChannelUpdated)]
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

                DiscordAuditLogEntry entry = null;
                AuditLogActionType type;
                if (e.ChannelBefore.PermissionOverwrites.Count > e.ChannelAfter.PermissionOverwrites.Count) {
                    type = AuditLogActionType.OverwriteCreate;
                    entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.OverwriteCreate).ConfigureAwait(false);
                } else if (e.ChannelBefore.PermissionOverwrites.Count < e.ChannelAfter.PermissionOverwrites.Count) {
                    type = AuditLogActionType.OverwriteDelete;
                    entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.OverwriteDelete).ConfigureAwait(false);
                } else {
                    if (e.ChannelBefore.PermissionOverwrites.Zip(e.ChannelAfter.PermissionOverwrites, (o1, o2) => o1.Allowed != o1.Allowed && o2.Denied != o2.Denied).Any()) {
                        type = AuditLogActionType.OverwriteUpdate;
                        entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.OverwriteUpdate).ConfigureAwait(false);
                    } else {
                        type = AuditLogActionType.ChannelUpdate;
                        entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.ChannelUpdate).ConfigureAwait(false);
                    }
                }

                if (entry != null) {
                    if (entry is DiscordAuditLogChannelEntry centry) {
                        emb.WithDescription(centry.Target.ToString());
                        emb.AddField("User responsible", centry.UserResponsible?.Mention ?? "<unknown>", inline: true);
                        if (centry.BitrateChange != null)
                            emb.AddField("Bitrate changed to", centry.BitrateChange.After.ToString(), inline: true);
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
                    } else if (entry is DiscordAuditLogOverwriteEntry owentry) {
                        emb.WithDescription($"{owentry.Channel.ToString()} ({type})");
                        emb.AddField("User responsible", owentry.UserResponsible?.Mention ?? "<unknown>", inline: true);

                        DiscordUser member = null;
                        DiscordRole role = null;

                        if (owentry.Target != null) {
                            try {
                                var isMemberUpdated = owentry.Target.Type.HasFlag(OverwriteType.Member);
                                if (isMemberUpdated)
                                    member = await e.Client.GetUserAsync(owentry.Target.Id).ConfigureAwait(false);
                                else
                                    role = e.Guild.GetRole(owentry.Target.Id);
                                emb.AddField("Target", isMemberUpdated ? member.ToString() : role.ToString(), inline: true);
                                if (owentry.AllowChange != null)
                                    emb.AddField("Allowed", $"{owentry.Target.Allowed.ToPermissionString() ?? " <unknown>"}", inline: true);
                                if (owentry.DenyChange != null)
                                    emb.AddField("Denied", $"{owentry.Target.Denied.ToPermissionString() ?? "<unknown>"}", inline: true);
                            } catch {

                            }
                        } else {
                            if (owentry.TargetIdChange?.After != null)
                                emb.AddField("Target ID", owentry.TargetIdChange?.After?.ToString() ?? "<unknown>", inline: true);
                        }
                        
                        if (!string.IsNullOrWhiteSpace(owentry.Reason))
                            emb.AddField("Reason", owentry.Reason);
                        emb.WithFooter($"At {owentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", owentry.UserResponsible.AvatarUrl);
                    }
                } else {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Channel", e.ChannelBefore?.ToString() ?? "<unknown>");
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }
    }
}
