using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
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
        [AsyncEventListener(DiscordEventType.ChannelCreated)]
        public static async Task ChannelCreateEventHandlerAsync(TheGodfatherShard shard, ChannelCreateEventArgs e)
        {
            if (shard.Services.GetService<GuildConfigService>().GetLogChannelForGuild(e.Guild) is null)
                return;

            var emb = new DiscordLogEmbedBuilder("Channel created", e.Channel.ToString(), DiscordEventType.ChannelCreated);
            emb.AddField("Channel type", e.Channel?.Type.ToString(), inline: true);

            DiscordAuditLogChannelEntry entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogChannelEntry>(AuditLogActionType.ChannelCreate);
            if (entry is null) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.AddInvocationFields(entry.UserResponsible);
                emb.AddField("Reason", entry.Reason, null);
                emb.WithTimestampFooter(entry.CreationTimestamp, entry.UserResponsible?.AvatarUrl);
            }

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.ChannelDeleted)]
        public static async Task ChannelDeleteEventHandlerAsync(TheGodfatherShard shard, ChannelDeleteEventArgs e)
        {
            if (shard.Services.GetService<GuildConfigService>().GetLogChannelForGuild(e.Guild) is null)
                return;

            var emb = new DiscordLogEmbedBuilder("Channel deleted", e.Channel.ToString(), DiscordEventType.ChannelDeleted);
            emb.AddField("Channel type", e.Channel?.Type.ToString(), inline: true);

            DiscordAuditLogChannelEntry entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogChannelEntry>(AuditLogActionType.ChannelDelete);
            if (entry is null) {
                emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
            } else {
                emb.AddInvocationFields(entry.UserResponsible);
                emb.AddField("Reason", entry.Reason, null);
                emb.WithTimestampFooter(entry.CreationTimestamp, entry.UserResponsible?.AvatarUrl);
            }

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.ChannelPinsUpdated)]
        public static async Task ChannelPinsUpdateEventHandlerAsync(TheGodfatherShard shard, ChannelPinsUpdateEventArgs e)
        {
            GuildConfigService gcs = shard.Services.GetService<GuildConfigService>();
            if (gcs.GetLogChannelForGuild(e.Channel.Guild) is null || gcs.IsChannelExempted(e.Channel.GuildId, e.Channel.Id, e.Channel.ParentId))
                return;

            var emb = new DiscordLogEmbedBuilder("Channel pins updated", null, DiscordEventType.ChannelPinsUpdated);
            emb.AddField("Channel", e.Channel.Mention, inline: true);

            IReadOnlyList<DiscordMessage> pinned = await e.Channel.GetPinnedMessagesAsync();
            if (pinned.Any()) {
                emb.WithDescription(Formatter.MaskedUrl("Jump to top pin", pinned.First().JumpLink));
                string content = string.IsNullOrWhiteSpace(pinned.First().Content) ? "<embedded message>" : pinned.First().Content;
                emb.AddField("Top pin content", Formatter.BlockCode(FormatterExtensions.StripMarkdown(content.Truncate(900))));
            }
            if (!(e.LastPinTimestamp is null))
                emb.AddField("Last pin timestamp", e.LastPinTimestamp.Value.ToUtcTimestamp(), inline: true);

            await shard.Services.GetService<LoggingService>().LogAsync(e.Channel.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.ChannelUpdated)]
        public static async Task ChannelUpdateEventHandlerAsync(TheGodfatherShard shard, ChannelUpdateEventArgs e)
        {
            if (e.ChannelBefore.Position != e.ChannelAfter.Position)
                return;

            GuildConfigService gcs = shard.Services.GetService<GuildConfigService>();
            if (gcs.GetLogChannelForGuild(e.Guild) is null || gcs.IsChannelExempted(e.Guild.Id, e.ChannelBefore.Id, e.ChannelBefore.ParentId))
                return;

            var emb = new DiscordLogEmbedBuilder("Channel updated", null, DiscordEventType.ChannelUpdated);

            DiscordAuditLogChannelEntry entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogChannelEntry>(AuditLogActionType.ChannelUpdate);
            if (entry is null) {    // Overwrite update
                AuditLogActionType type;
                DiscordAuditLogEntry aentry;
                if (e.ChannelBefore.PermissionOverwrites.Count > e.ChannelAfter.PermissionOverwrites.Count) {
                    type = AuditLogActionType.OverwriteCreate;
                    aentry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogOverwriteEntry>(AuditLogActionType.OverwriteCreate);
                } else if (e.ChannelBefore.PermissionOverwrites.Count < e.ChannelAfter.PermissionOverwrites.Count) {
                    type = AuditLogActionType.OverwriteDelete;
                    aentry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogOverwriteEntry>(AuditLogActionType.OverwriteDelete);
                } else {
                    if (e.ChannelBefore.PermissionOverwrites.Zip(e.ChannelAfter.PermissionOverwrites, (o1, o2) => o1.Allowed != o1.Allowed && o2.Denied != o2.Denied).Any()) {
                        type = AuditLogActionType.OverwriteUpdate;
                        aentry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogOverwriteEntry>(AuditLogActionType.OverwriteUpdate);
                    } else {
                        type = AuditLogActionType.ChannelUpdate;
                        aentry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogChannelEntry>(AuditLogActionType.ChannelUpdate);
                    }
                }

                if (!(aentry is null) && aentry is DiscordAuditLogOverwriteEntry owentry) {
                    emb.WithDescription($"{owentry.Channel.ToString()} ({type})");
                    emb.AddInvocationFields(owentry.UserResponsible);

                    DiscordUser member = null;
                    DiscordRole role = null;

                    try {
                        bool isMemberUpdated = owentry.Target.Type.HasFlag(OverwriteType.Member);
                        if (isMemberUpdated)
                            member = await e.Client.GetUserAsync(owentry.Target.Id);
                        else
                            role = e.Guild.GetRole(owentry.Target.Id);
                        emb.AddField("Target", isMemberUpdated ? member.ToString() : role.ToString(), inline: true);
                        if (!(owentry.AllowChange is null))
                            emb.AddField("Allowed", owentry.Target.Allowed.ToPermissionString(), inline: true);
                        if (!(owentry.DenyChange is null))
                            emb.AddField("Denied", owentry.Target.Denied.ToPermissionString(), inline: true);
                    } catch {
                        emb.AddField("Target ID", owentry.Target.Id.ToString(), inline: true);
                    }

                    emb.AddField("Reason", owentry.Reason, null);
                    emb.WithTimestampFooter(owentry.CreationTimestamp, owentry.UserResponsible.AvatarUrl);
                } else {
                    return;
                }
            } else {     // Regular update
                emb.WithDescription(entry.Target.ToString());
                emb.AddInvocationFields(entry.UserResponsible);
                emb.AddField("Bitrate changed to", entry.BitrateChange?.After.ToString(), null, inline: true);
                emb.AddField("Name changed to", entry.NameChange?.After, null, inline: true);
                emb.AddField("NSFW flag changed to", entry.NsfwChange?.After.Value.ToString(), null, inline: true);
                emb.AddField("Type changed to", entry.TypeChange?.After.Value.ToString(), null, inline: true);
                emb.AddField("Per-user rate limit changed to", entry.PerUserRateLimitChange?.After.Value.ToString(), null, inline: true);
                if (!(entry.OverwriteChange is null))
                    emb.AddField("Permissions overwrites changed", $"{entry.OverwriteChange.After.Count} overwrites after changes");
                if (!(entry.TopicChange is null)) {
                    string ptopic = Formatter.BlockCode(FormatterExtensions.StripMarkdown(string.IsNullOrWhiteSpace(entry.TopicChange.Before) ? " " : entry.TopicChange.Before));
                    string ctopic = Formatter.BlockCode(FormatterExtensions.StripMarkdown(string.IsNullOrWhiteSpace(entry.TopicChange.After) ? " " : entry.TopicChange.After));
                    emb.AddField("Topic changed", $"From:{ptopic}\nTo:{ctopic}");
                }
                emb.AddField("Reason", entry.Reason, null);
                emb.WithTimestampFooter(entry.CreationTimestamp, entry.UserResponsible.AvatarUrl);
            }

            await shard.Services.GetService<LoggingService>().LogAsync(e.Guild, emb);
        }
    }
}
