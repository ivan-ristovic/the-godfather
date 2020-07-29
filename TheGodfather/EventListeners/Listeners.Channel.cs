using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Serilog;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.DmChannelCreated)]
        public static Task DmChannelCreateEventHandlerAsync(TheGodfatherShard shard, DmChannelCreateEventArgs e)
        {
            LogExt.Debug(
                shard.Id,
                new[] { "Create: DM {Channel}, recipients:", "{Recipients}" },
                e.Channel,
                e.Channel.Recipients.Humanize(Environment.NewLine)
            );
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.DmChannelDeleted)]
        public static Task DmChannelDeleteEventHandlerAsync(TheGodfatherShard shard, DmChannelDeleteEventArgs e)
        {
            LogExt.Debug(
                shard.Id,
                new[] { "Delete: DM {Channel}, recipients:", "{Recipients}" },
                e.Channel,
                e.Channel.Recipients.Humanize(Environment.NewLine)
            );
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.ChannelCreated)]
        public static async Task ChannelCreateEventHandlerAsync(TheGodfatherShard shard, ChannelCreateEventArgs e)
        {
            LogExt.Verbose(shard.Id, "Create: {Channel}, {Guild}", e.Channel, e.Guild);
            if (!IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.ChannelCreated, "evt-chn-create-title", e.Channel);
            emb.AddLocalizedTitleField("chn-type", e.Channel?.Type, inline: true);

            DiscordAuditLogChannelEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogChannelEntry>(AuditLogActionType.ChannelCreate);
            emb.AddFieldsFromAuditLogEntry(entry);

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.ChannelDeleted)]
        public static async Task ChannelDeleteEventHandlerAsync(TheGodfatherShard shard, ChannelDeleteEventArgs e)
        {
            LogExt.Verbose(shard.Id, "Delete: {Channel}, {Guild}", e.Channel, e.Guild);
            if (!IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.ChannelDeleted, "evt-chn-delete-title", e.Channel);
            emb.AddLocalizedTitleField("chn-type", e.Channel?.Type, inline: true);

            DiscordAuditLogChannelEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogChannelEntry>(AuditLogActionType.ChannelDelete);
            emb.AddFieldsFromAuditLogEntry(entry);

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.ChannelPinsUpdated)]
        public static async Task ChannelPinsUpdateEventHandlerAsync(TheGodfatherShard shard, ChannelPinsUpdateEventArgs e)
        {
            LogExt.Verbose(shard.Id, "Pins update: {Channel}, {Guild}", e.Channel, e.Guild);

            if (e.Channel.IsPrivate || IsChannelExempted(shard, e.Guild, e.Channel, out _))
                return;

            if (!IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.ChannelPinsUpdated, "evt-chn-pins-update-title");
            emb.AddLocalizedTitleField("msg-chn", e.Channel.Mention);

            IReadOnlyList<DiscordMessage> pinned = await e.Channel.GetPinnedMessagesAsync();
            if (pinned.Any()) {
                emb.WithDescription(Formatter.MaskedUrl("Jumplink", pinned.First().JumpLink));
                string content = string.IsNullOrWhiteSpace(pinned.First().Content) ? "<embed>" : pinned.First().Content;
                emb.AddLocalizedTitleField("msg-top-pin-content", Formatter.BlockCode(FormatterExt.StripMarkdown(content.Truncate(900))));
            }
            emb.AddLocalizedTimestampField("msg-last-pin-timestamp", e.LastPinTimestamp);

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.ChannelUpdated)]
        public static async Task ChannelUpdateEventHandlerAsync(TheGodfatherShard shard, ChannelUpdateEventArgs e)
        {
            LogExt.Verbose(shard.Id, "Channel update: {Channel}, {Guild}", e.ChannelBefore, e.Guild);

            if (e.ChannelBefore.IsPrivate || IsChannelExempted(shard, e.Guild, e.ChannelBefore, out _))
                return;

            if (e.ChannelBefore.Position != e.ChannelAfter.Position)
                return;

            if (!IsLogEnabledForGuild(shard, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.ChannelPinsUpdated, "evt-chn-update-title");

            DiscordAuditLogEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync();
            if (entry is DiscordAuditLogChannelEntry centry) {
                Log.Verbose("Retrieved channel update information from audit log");
                emb.AddFieldsFromAuditLogEntry(centry, (emb, ent) => {
                    emb.WithDescription(ent?.Target?.ToString() ?? e.ChannelBefore.ToString());
                    emb.AddLocalizedPropertyChangeField("evt-chn-update-name", ent?.NameChange, inline: false);
                    emb.AddLocalizedPropertyChangeField("evt-chn-update-nsfw", ent?.NsfwChange);
                    emb.AddLocalizedPropertyChangeField("evt-chn-update-br", ent?.BitrateChange);
                    emb.AddLocalizedPropertyChangeField("evt-chn-update-rl", ent?.PerUserRateLimitChange);
                    emb.AddLocalizedPropertyChangeField("evt-chn-update-type", ent?.TypeChange);
                    if (centry.OverwriteChange is { })
                        emb.AddLocalizedTitleField("evt-chn-ow-change", centry.OverwriteChange.After.Count);
                    if (centry.TopicChange is { }) {
                        string before = Formatter.BlockCode(
                            FormatterExt.StripMarkdown(string.IsNullOrWhiteSpace(centry.TopicChange.Before) ? " " : centry.TopicChange.Before).Truncate(450, "...")
                        );
                        string after = Formatter.BlockCode(
                            FormatterExt.StripMarkdown(string.IsNullOrWhiteSpace(centry.TopicChange.After) ? " " : centry.TopicChange.After).Truncate(450, "...")
                        );
                        emb.AddLocalizedField("evt-chn-topic-change", "msg-from-to-block", false, null, new[] { before, after });
                    }
                });
            } else if (entry is DiscordAuditLogOverwriteEntry owentry) {

                // TODO use `deny_new` and `allow_new` once they are implemented by D#+

                Log.Verbose("Retrieved permission overwrite update information from audit log");
                emb.WithDescription($"{owentry.Channel} (Overwrite {owentry.ActionCategory.Humanize()})");

                try {
                    DiscordMember? member = owentry.Target.Type == OverwriteType.Member ? await owentry.Target.GetMemberAsync() : null;
                    DiscordRole? role = owentry.Target.Type == OverwriteType.Role ? await owentry.Target.GetRoleAsync() : null;
                    emb.AddLocalizedTitleField("evt-invoke-target", member?.Mention ?? role?.Mention, inline: true);
                    if (owentry.AllowChange is { })
                        emb.AddLocalizedTitleField("evt-chn-ow-allowed", owentry.Target.Allowed.ToPermissionString(), inline: true);
                    if (owentry.DenyChange is { })
                        emb.AddLocalizedTitleField("evt-chn-ow-denied", owentry.Target.Denied.ToPermissionString(), inline: true);
                } catch {
                    Log.Verbose("Failed to retrieve permission overwrite target");
                    emb.AddLocalizedTitleField("evt-invoke-target", owentry.Target?.Id, inline: true);
                }
                emb.AddInvocationFields(owentry.UserResponsible);
            } else {
                Log.Warning("Channel update event details could not be found");
            }

            await logService.LogAsync(e.Guild, emb);
        }
    }
}
