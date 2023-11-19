using System.Collections.Concurrent;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.EventListeners;

internal static partial class Listeners
{
    private static readonly ConcurrentDictionary<ulong, ConcurrentQueue<ChannelUpdateEventArgs>> _channelUpdates = new();


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

        emb.WithLocalizedTitle(DiscordEventType.ChannelCreated, TranslationKey.evt_chn_create, e.Channel);
        emb.AddLocalizedField(TranslationKey.str_chn_type, e.Channel?.Type, true);

        DiscordAuditLogChannelEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogChannelEntry>(AuditLogActionType.ChannelCreate);
        emb.AddFieldsFromAuditLogEntry(entry);

        await logService.LogAsync(e.Guild, emb);
    }

    [AsyncEventListener(DiscordEventType.ChannelCreated)]
    public static Task ChannelCreateBackupEventHandlerAsync(TheGodfatherBot bot, ChannelCreateEventArgs e)
    {
        if (e.Channel.GuildId is null)
            return Task.CompletedTask;

        LogExt.Debug(bot.GetId(e.Guild.Id), "Adding newly created channel to backup service: {Channel}, {Guild}", e.Channel, e.Guild);
        return bot.Services.GetRequiredService<BackupService>().AddChannelAsync(e.Channel.GuildId.Value, e.Channel.Id);
    }

    [AsyncEventListener(DiscordEventType.ChannelDeleted)]
    public static async Task ChannelDeleteEventHandlerAsync(TheGodfatherBot bot, ChannelDeleteEventArgs e)
    {
        LogExt.Debug(bot.GetId(e.Guild.Id), "Delete: {Channel}, {Guild}", e.Channel, e.Guild);
        if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
            return;

        emb.WithLocalizedTitle(DiscordEventType.ChannelDeleted, TranslationKey.evt_chn_delete, e.Channel);
        emb.AddLocalizedField(TranslationKey.str_chn_type, e.Channel?.Type, true);

        DiscordAuditLogChannelEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogChannelEntry>(AuditLogActionType.ChannelDelete);
        emb.AddFieldsFromAuditLogEntry(entry);

        await logService.LogAsync(e.Guild, emb);
    }

    [AsyncEventListener(DiscordEventType.ChannelDeleted)]
    public static Task ChannelDeleteBackupEventHandlerAsync(TheGodfatherBot bot, ChannelDeleteEventArgs e)
    {
        if (e.Channel.GuildId is null)
            return Task.CompletedTask;

        bot.Services.GetRequiredService<BackupService>().RemoveChannel(e.Channel.GuildId.Value, e.Channel.Id);
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

        emb.WithLocalizedTitle(DiscordEventType.ChannelPinsUpdated, TranslationKey.evt_chn_pins_update);
        emb.AddLocalizedField(TranslationKey.str_chn, e.Channel.Mention);

        DiscordAuditLogMessagePinEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogMessagePinEntry>();
        DiscordMessage? pinned;
        if (entry is null) {
            IReadOnlyList<DiscordMessage> pinnedMessages = await e.Channel.GetPinnedMessagesAsync();
            pinned = pinnedMessages.FirstOrDefault();
        } else {
            pinned = entry.Message;
            emb.AddInvocationFields(entry.UserResponsible);
            emb.AddReason(entry.Reason);
        }

        if (pinned is not null) {
            emb.WithDescription(Formatter.MaskedUrl("Jumplink", pinned.JumpLink));
            string content = string.IsNullOrWhiteSpace(pinned.Content) ? "<embed>" : pinned.Content;
            emb.AddLocalizedField(TranslationKey.str_pin_content, Formatter.BlockCode(Formatter.Strip(content.Truncate(900))));
            emb.WithLocalizedTimestamp(pinned.Timestamp);
        }

        await logService.LogAsync(e.Guild, emb);
    }

    [AsyncEventListener(DiscordEventType.ChannelUpdated)]
    public static async Task ChannelUpdateEventHandlerAsync(TheGodfatherBot bot, ChannelUpdateEventArgs e)
    {
        LogExt.Debug(bot.GetId(e.Guild.Id), "Channel update: {Channel}, {Guild}", e.ChannelBefore, e.Guild);

        if (e.ChannelBefore.IsPrivate || LoggingService.IsChannelExempted(bot, e.Guild, e.ChannelBefore, out _))
            return;

        if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
            return;

        ConcurrentQueue<ChannelUpdateEventArgs> upds = _channelUpdates.GetOrAdd(e.ChannelAfter.Id, new ConcurrentQueue<ChannelUpdateEventArgs>());
        upds.Enqueue(e);
        int updatesBefore = upds.Count;
        await Task.Delay(e.ChannelAfter.IsCategory ? TimeSpan.FromSeconds(3) : TimeSpan.FromSeconds(2));

        Log.Debug("Channel update {Id} event count: {BeforeCount} -> {Count}", e.ChannelAfter.Id, updatesBefore, upds.Count);
        _channelUpdates.TryRemove(e.ChannelAfter.Id, out _);
        if ((e.ChannelAfter.ParentId is not null && _channelUpdates.ContainsKey(e.ChannelAfter.ParentId.Value)) || updatesBefore < upds.Count)
            return;

        emb.WithLocalizedTitle(DiscordEventType.ChannelPinsUpdated, TranslationKey.evt_chn_update);

        DiscordAuditLogEntry? entry = await e.Guild.GetRecentAuditLogEntryAsync();
        if (entry is DiscordAuditLogChannelEntry centry) {
            Log.Verbose("Retrieved channel update information from audit log");
            emb.WithDescription(centry?.Target?.ToString() ?? e.ChannelBefore.ToString());
            emb.AddFieldsFromAuditLogEntry(centry, (emb, ent) => {
                emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_name, ent.NameChange, false);
                emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_nsfw, ent.NsfwChange);
                emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_bitrate, ent.BitrateChange);
                emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_ratelimit, ent.PerUserRateLimitChange);
                emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_type, ent.TypeChange);
                emb.AddLocalizedField(TranslationKey.evt_chn_ow_change, ent.OverwriteChange?.After?.Count, unknown: false);
                if (ent.TopicChange is not null) {
                    string before = Formatter.BlockCode(
                        Formatter.Strip(string.IsNullOrWhiteSpace(ent.TopicChange.Before) ? " " : ent.TopicChange.Before).Truncate(450, "...")
                    );
                    string after = Formatter.BlockCode(
                        Formatter.Strip(string.IsNullOrWhiteSpace(ent.TopicChange.After) ? " " : ent.TopicChange.After).Truncate(450, "...")
                    );
                    emb.AddLocalizedField(TranslationKey.evt_chn_topic_change, TranslationKey.fmt_from_to_block(before, after), false);
                }
            });
        } else if (entry is DiscordAuditLogOverwriteEntry owentry) {

            // TODO use `deny_new` and `allow_new` once they are implemented by D#+

            Log.Verbose("Retrieved permission overwrite update information from audit log");
            emb.WithDescription($"{owentry.Channel} (Overwrite {owentry.ActionCategory.Humanize()})");

            try {
                DiscordMember? member = owentry.Target.Type == OverwriteType.Member ? await owentry.Target.GetMemberAsync() : null;
                DiscordRole? role = owentry.Target.Type == OverwriteType.Role ? await owentry.Target.GetRoleAsync() : null;
                emb.AddLocalizedField(TranslationKey.evt_invoke_target, member?.Mention ?? role?.Mention, true);
                if (owentry.AllowChange is not null)
                    emb.AddLocalizedField(TranslationKey.str_allowed, owentry.Target.Allowed.ToPermissionString(), true);
                if (owentry.DenyChange is not null)
                    emb.AddLocalizedField(TranslationKey.str_denied, owentry.Target.Denied.ToPermissionString(), true);
            } catch {
                Log.Verbose("Failed to retrieve permission overwrite target");
                emb.AddLocalizedField(TranslationKey.evt_invoke_target, owentry.Target?.Id, true);
            }
            emb.AddInvocationFields(owentry.UserResponsible);
        } else {
            emb.AddLocalizedField(TranslationKey.str_chn, e.ChannelBefore, true);
            ChannelUpdateEventArgs fst = upds.First();
            ChannelUpdateEventArgs lst = upds.Last();
            emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_name, fst.ChannelBefore.Name, lst.ChannelAfter.Name, true);
            emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_nsfw, fst.ChannelBefore.IsNSFW, lst.ChannelAfter.IsNSFW, true);
            emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_bitrate, fst.ChannelBefore.Bitrate, lst.ChannelAfter.Bitrate, true);
            emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_position, fst.ChannelBefore.Position, lst.ChannelAfter.Position, true);
            emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_ratelimit, fst.ChannelBefore.PerUserRateLimit, lst.ChannelAfter.PerUserRateLimit, true);
            emb.AddLocalizedPropertyChangeField(TranslationKey.evt_upd_type, fst.ChannelBefore.Type, lst.ChannelAfter.Type, true);
            if (!fst.ChannelBefore.Topic?.Equals(lst.ChannelAfter.Topic, StringComparison.InvariantCultureIgnoreCase) ?? false) {
                string before = Formatter.BlockCode(
                    Formatter.Strip(string.IsNullOrWhiteSpace(fst.ChannelBefore.Topic) ? " " : fst.ChannelBefore.Topic).Truncate(450, "...")
                );
                string after = Formatter.BlockCode(
                    Formatter.Strip(string.IsNullOrWhiteSpace(lst.ChannelAfter.Topic) ? " " : lst.ChannelAfter.Topic).Truncate(450, "...")
                );
                emb.AddLocalizedField(TranslationKey.evt_chn_topic_change, TranslationKey.fmt_from_to_block(before, after), false);
            }
            if (fst.ChannelBefore.Position != lst.ChannelAfter.Position)
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

        emb.WithLocalizedTitle(DiscordEventType.WebhooksUpdated, TranslationKey.evt_gld_wh_upd, e.Channel);

        DiscordAuditLogWebhookEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogWebhookEntry>(AuditLogActionType.WebhookUpdate);
        emb.AddFieldsFromAuditLogEntry(entry, (emb, ent) => {
            emb.WithDescription($"{ent.Target.Name}, {ent.Target.ChannelId}");
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_name, ent.NameChange);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_ahash, ent.AvatarHashChange);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_chn, ent.ChannelChange);
            emb.AddLocalizedPropertyChangeField(TranslationKey.str_type, ent.TypeChange);
        });

        await logService.LogAsync(e.Guild, emb);
    }
}