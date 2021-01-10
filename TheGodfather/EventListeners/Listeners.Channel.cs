using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.DmChannelCreated)]
        public static Task DmChannelCreateEventHandlerAsync(TheGodfatherBot bot, DmChannelCreateEventArgs e)
        {
            LogExt.Debug(
                bot.GetId(null),
                new[] { "Create: DM {Channel}, recipients:", "{Recipients}" },
                e.Channel,
                e.Channel.Recipients.Humanize(Environment.NewLine)
            );
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.DmChannelDeleted)]
        public static Task DmChannelDeleteEventHandlerAsync(TheGodfatherBot bot, DmChannelDeleteEventArgs e)
        {
            LogExt.Debug(
                bot.GetId(null),
                new[] { "Delete: DM {Channel}, recipients:", "{Recipients}" },
                e.Channel,
                e.Channel.Recipients.Humanize(Environment.NewLine)
            );
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.ChannelCreated)]
        public static async Task ChannelCreateEventHandlerAsync(TheGodfatherBot bot, ChannelCreateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Create: {Channel}, {Guild}", e.Channel, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.ChannelCreated, "evt-chn-create", e.Channel);
            emb.AddLocalizedTitleField("str-chn-type", e.Channel?.Type, inline: true);

            DiscordAuditLogChannelEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogChannelEntry>(AuditLogActionType.ChannelCreate);
            emb.AddFieldsFromAuditLogEntry(entry);

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.ChannelCreated)]
        public static Task ChannelCreateBackupEventHandlerAsync(TheGodfatherBot bot, ChannelCreateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Adding newly created channel to backup service: {Channel}, {Guild}", e.Channel, e.Guild);
            return bot.Services.GetRequiredService<BackupService>().AddChannel(e.Channel.GuildId, e.Channel.Id);
        }

        [AsyncEventListener(DiscordEventType.ChannelDeleted)]
        public static async Task ChannelDeleteEventHandlerAsync(TheGodfatherBot bot, ChannelDeleteEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Delete: {Channel}, {Guild}", e.Channel, e.Guild);
            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.ChannelDeleted, "evt-chn-delete", e.Channel);
            emb.AddLocalizedTitleField("str-chn-type", e.Channel?.Type, inline: true);

            DiscordAuditLogChannelEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogChannelEntry>(AuditLogActionType.ChannelDelete);
            emb.AddFieldsFromAuditLogEntry(entry);

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.ChannelDeleted)]
        public static Task ChannelDeleteBackupEventHandlerAsync(TheGodfatherBot bot, ChannelCreateEventArgs e)
        {
            bot.Services.GetRequiredService<BackupService>().RemoveChannel(e.Channel.GuildId, e.Channel.Id);
            LogExt.Debug(bot.GetId(e.Guild.Id), "Removed channel from backup service: {Channel}, {Guild}", e.Channel, e.Guild);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.ChannelPinsUpdated)]
        public static async Task ChannelPinsUpdateEventHandlerAsync(TheGodfatherBot bot, ChannelPinsUpdateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Pins update: {Channel}, {Guild}", e.Channel, e.Guild);

            if (e.Guild is null || LoggingService.IsChannelExempted(bot, e.Guild, e.Channel, out _))
                return;

            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.ChannelPinsUpdated, "evt-chn-pins-update");
            emb.AddLocalizedTitleField("str-chn", e.Channel.Mention);

            IReadOnlyList<DiscordMessage> pinned = await e.Channel.GetPinnedMessagesAsync();
            if (pinned.Any()) {
                emb.WithDescription(Formatter.MaskedUrl("Jumplink", pinned[0].JumpLink));
                string content = string.IsNullOrWhiteSpace(pinned[0].Content) ? "<embed>" : pinned[0].Content;
                emb.AddLocalizedTitleField("str-top-pin-content", Formatter.BlockCode(Formatter.Strip(content.Truncate(900))));
            }
            emb.AddLocalizedTimestampField("str-last-pin-timestamp", e.LastPinTimestamp);

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.ChannelUpdated)]
        public static async Task ChannelUpdateEventHandlerAsync(TheGodfatherBot bot, ChannelUpdateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Channel update: {Channel}, {Guild}", e.ChannelBefore, e.Guild);

            if (e.ChannelBefore.IsPrivate || LoggingService.IsChannelExempted(bot, e.Guild, e.ChannelBefore, out _))
                return;

            if (e.ChannelBefore.Position != e.ChannelAfter.Position)
                return;

            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle(DiscordEventType.ChannelPinsUpdated, "evt-chn-update");

            DiscordAuditLogEntry? entry = await e.Guild.GetRecentAuditLogEntryAsync();
            if (entry is DiscordAuditLogChannelEntry centry) {
                Log.Verbose("Retrieved channel update information from audit log");
                emb.WithDescription(centry?.Target?.ToString() ?? e.ChannelBefore.ToString());
                emb.AddFieldsFromAuditLogEntry(centry, (emb, ent) => {
                    emb.AddLocalizedPropertyChangeField("evt-upd-name", ent.NameChange, inline: false);
                    emb.AddLocalizedPropertyChangeField("evt-upd-nsfw", ent.NsfwChange);
                    emb.AddLocalizedPropertyChangeField("evt-upd-bitrate", ent.BitrateChange);
                    emb.AddLocalizedPropertyChangeField("evt-upd-ratelimit", ent.PerUserRateLimitChange);
                    emb.AddLocalizedPropertyChangeField("evt-upd-type", ent.TypeChange);
                    emb.AddLocalizedTitleField("evt-chn-ow-change", ent.OverwriteChange.After.Count, unknown: false);
                    if (ent.TopicChange is { }) {
                        string before = Formatter.BlockCode(
                            Formatter.Strip(string.IsNullOrWhiteSpace(ent.TopicChange.Before) ? " " : ent.TopicChange.Before).Truncate(450, "...")
                        );
                        string after = Formatter.BlockCode(
                            Formatter.Strip(string.IsNullOrWhiteSpace(ent.TopicChange.After) ? " " : ent.TopicChange.After).Truncate(450, "...")
                        );
                        emb.AddLocalizedField("evt-chn-topic-change", "fmt-from-to-block", false, null, new[] { before, after });
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
                        emb.AddLocalizedTitleField("str-allowed", owentry.Target.Allowed.ToPermissionString(), inline: true);
                    if (owentry.DenyChange is { })
                        emb.AddLocalizedTitleField("str-denied", owentry.Target.Denied.ToPermissionString(), inline: true);
                } catch {
                    Log.Verbose("Failed to retrieve permission overwrite target");
                    emb.AddLocalizedTitleField("evt-invoke-target", owentry.Target?.Id, inline: true);
                }
                emb.AddInvocationFields(owentry.UserResponsible);
            } else {
                emb.AddLocalizedTitleField("str-chn", e.ChannelBefore.Mention, inline: true);
                emb.AddLocalizedPropertyChangeField("evt-upd-name", e.ChannelBefore.Name, e.ChannelAfter.Name, inline: true);
                emb.AddLocalizedPropertyChangeField("evt-upd-nsfw", e.ChannelBefore.IsNSFW, e.ChannelAfter.IsNSFW, inline: true);
                emb.AddLocalizedPropertyChangeField("evt-upd-bitrate", e.ChannelBefore.Bitrate, e.ChannelAfter.Bitrate, inline: true);
                emb.AddLocalizedPropertyChangeField("evt-upd-ratelimit", e.ChannelBefore.PerUserRateLimit, e.ChannelAfter.PerUserRateLimit, inline: true);
                emb.AddLocalizedPropertyChangeField("evt-upd-type", e.ChannelBefore.Type, e.ChannelAfter.Type, inline: true);
                if (!e.ChannelBefore.Topic.Equals(e.ChannelAfter.Topic, StringComparison.InvariantCultureIgnoreCase)) {
                    string before = Formatter.BlockCode(
                        Formatter.Strip(string.IsNullOrWhiteSpace(e.ChannelBefore.Topic) ? " " : e.ChannelBefore.Topic).Truncate(450, "...")
                    );
                    string after = Formatter.BlockCode(
                        Formatter.Strip(string.IsNullOrWhiteSpace(e.ChannelAfter.Topic) ? " " : e.ChannelAfter.Topic).Truncate(450, "...")
                    );
                    emb.AddLocalizedField("evt-chn-topic-change", "fmt-from-to-block", false, null, new[] { before, after });
                }
                emb.AddInsufficientAuditLogPermissionsField();
            }

            await logService.LogAsync(e.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.WebhooksUpdated)]
        public static async Task WebhooksUpdateEventHandlerAsync(TheGodfatherBot bot, WebhooksUpdateEventArgs e)
        {
            LogExt.Debug(bot.GetId(e.Guild.Id), "Webhooks update: {Channel}, {Guild}", e.Channel, e.Guild);

            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
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
