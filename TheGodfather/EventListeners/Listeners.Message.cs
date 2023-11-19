using System.IO;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Misc.Services;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Modules.Reactions.Extensions;
using TheGodfather.Modules.Reactions.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.EventListeners;

internal static partial class Listeners
{
    [AsyncEventListener(DiscordEventType.MessagesBulkDeleted)]
    public static async Task BulkDeleteEventHandlerAsync(TheGodfatherBot bot, MessageBulkDeleteEventArgs e)
    {
        if (e.Guild is null)
            return;

        if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
            return;

        if (LoggingService.IsChannelExempted(bot, e.Guild, e.Channel, out GuildConfigService gcs))
            return;

        emb.WithLocalizedTitle(DiscordEventType.MessagesBulkDeleted, TranslationKey.evt_msg_del_bulk, e.Channel);
        emb.AddLocalizedField(TranslationKey.str_count, e.Messages.Count, true);
        await using var ms = new MemoryStream();
        await using var sw = new StreamWriter(ms);
        foreach (DiscordMessage msg in e.Messages) {
            sw.WriteLine($"[{msg.Timestamp}] {msg.Author}");
            sw.WriteLine(string.IsNullOrWhiteSpace(msg.Content) ? "?" : msg.Content);
            sw.WriteLine(msg.Attachments.Select(a => $"{a.FileName} ({a.FileSize})").JoinWith(", "));
            sw.Flush();
        }
        ms.Seek(0, SeekOrigin.Begin);
        DiscordChannel? chn = gcs.GetLogChannelForGuild(e.Guild);
        if (chn is not null)
            await chn.SendMessageAsync(
                new DiscordMessageBuilder()
                    .WithEmbed(emb.Build())
                    .AddFile($"{e.Channel.Name}-deleted-messages.txt", ms)
            );
    }

    [AsyncEventListener(DiscordEventType.MessageCreated)]
    public static async Task MessageCreateEventHandlerAsync(TheGodfatherBot bot, MessageCreateEventArgs e)
    {
        if (e.Author.IsBot)
            return;

        if (e.Guild is null) {
            LogExt.Debug(bot.GetId(null), new[] { "DM message received from {User}:", "{Message}" }, e.Author, e.Message);
            return;
        }

        if (bot.Services.GetRequiredService<BlockingService>().IsBlocked(e.Guild.Id, e.Channel.Id, e.Author.Id))
            return;

        if (string.IsNullOrWhiteSpace(e.Message?.Content))
            return;

        if (!e.Message.Content.StartsWith(bot.Services.GetRequiredService<GuildConfigService>().GetGuildPrefix(e.Guild.Id))) {
            short rank = bot.Services.GetRequiredService<UserRanksService>().ChangeXp(e.Guild.Id, e.Author.Id);
            if (rank != 0) {
                LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
                LevelRole? lr = await bot.Services.GetRequiredService<LevelRoleService>().GetAsync(e.Guild.Id, rank);
                DiscordRole? levelRole = lr is not null ? e.Guild.GetRole(lr.RoleId) : null;
                if (levelRole is not null) {
                    DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
                    await member.GrantRoleAsync(levelRole);
                }

                GuildConfig gcfg = await bot.Services.GetRequiredService<GuildConfigService>().GetConfigAsync(e.Guild.Id);
                if (!gcfg.SilentLevelUpEnabled) {
                    XpRank? rankInfo = await bot.Services.GetRequiredService<GuildRanksService>().GetAsync(e.Guild.Id, rank);
                    string rankupStr = levelRole is not null
                                           ? ls.GetString(e.Guild.Id, TranslationKey.fmt_rankup_lr(e.Author.Mention, Formatter.Bold(rank.ToString()), rankInfo?.Name ?? "/", levelRole.Mention))
                        : ls.GetString(e.Guild.Id, TranslationKey.fmt_rankup(e.Author.Mention, Formatter.Bold(rank.ToString()), rankInfo?.Name ?? "/"));
                    await e.Channel.EmbedAsync(rankupStr, Emojis.Medal);
                }
            }
        }
    }

    [AsyncEventListener(DiscordEventType.MessageCreated)]
    public static async Task MessageCreateProtectionHandlerAsync(TheGodfatherBot bot, MessageCreateEventArgs e)
    {
        if (e.Author.IsBot || e.Guild is null)
            return;

        if (bot.Services.GetRequiredService<BlockingService>().IsChannelBlocked(e.Channel.Id))
            return;

        CachedGuildConfig? gcfg = bot.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(e.Guild.Id);
        if (gcfg is not null) {
            if (gcfg.RatelimitSettings.Enabled)
                await bot.Services.GetRequiredService<RatelimitService>().HandleNewMessageAsync(e, gcfg.RatelimitSettings);
            if (gcfg.AntispamSettings.Enabled)
                await bot.Services.GetRequiredService<AntispamService>().HandleNewMessageAsync(e, gcfg.AntispamSettings);
            if (gcfg.AntiMentionSettings.Enabled)
                await bot.Services.GetRequiredService<AntiMentionService>().HandleNewMessageAsync(e, gcfg.AntiMentionSettings);
        }
    }

    [AsyncEventListener(DiscordEventType.MessageCreated)]
    public static Task MessageCreateBackupHandlerAsync(TheGodfatherBot bot, MessageCreateEventArgs e)
        => e.Guild is null ? Task.CompletedTask : bot.Services.GetRequiredService<BackupService>().BackupAsync(e.Message);

    [AsyncEventListener(DiscordEventType.MessageCreated)]
    public static async Task MessageFilterEventHandlerAsync(TheGodfatherBot bot, MessageCreateEventArgs e)
    {
        if (e.Author.IsBot || e.Guild is null || string.IsNullOrWhiteSpace(e.Message?.Content))
            return;

        if (bot.Services.GetRequiredService<BlockingService>().IsChannelBlocked(e.Channel.Id))
            return;

        CachedGuildConfig? gcfg = bot.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(e.Guild.Id);
        if (gcfg?.LinkfilterSettings.Enabled ?? false)
            if (await bot.Services.GetRequiredService<LinkfilterService>().HandleNewMessageAsync(e, gcfg.LinkfilterSettings))
                return;

        if (!bot.Services.GetRequiredService<FilteringService>().TextContainsFilter(e.Guild.Id, e.Message.Content, out Filter? match))
            return;

        await SanitizeFilteredMessage(bot, e.Message, match?.OnHitAction);
    }

    [AsyncEventListener(DiscordEventType.MessageCreated)]
    public static async Task MessageReactionEventHandlerAsync(TheGodfatherBot bot, MessageCreateEventArgs e)
    {
        if (e.Author.IsBot || e.Guild is null || string.IsNullOrWhiteSpace(e.Message?.Content))
            return;

        if (bot.Services.GetRequiredService<BlockingService>().IsBlocked(e.Guild.Id, e.Channel.Id, e.Author.Id))
            return;

        ReactionsService rs = bot.Services.GetRequiredService<ReactionsService>();
        Permissions perms = e.Channel.PermissionsFor(e.Guild.CurrentMember);

        if (perms.HasFlag(Permissions.AddReactions)) {
            DiscordClient client = bot.Client.GetShard(e.Guild.Id);
            try {
                await rs.HandleEmojiReactionsAsync(client, e.Message);
            } catch (NotFoundException) {
                LogExt.Debug(bot.GetId(e.Guild.Id), "Trying to react to a deleted message.");
            }
        }

        if (perms.HasFlag(Permissions.SendMessages))
            await rs.HandleTextReactionsAsync(e.Message);
    }

    [AsyncEventListener(DiscordEventType.MessageDeleted)]
    public static async Task MessageDeleteEventHandlerAsync(TheGodfatherBot bot, MessageDeleteEventArgs e)
    {
        if (e.Guild is null || e.Message is null)
            return;

        if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
            return;

        if (LoggingService.IsChannelExempted(bot, e.Guild, e.Channel, out GuildConfigService gcs))
            return;

        bool eventRunning = bot.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(e.Channel.Id);
        if (e.Message.Author == bot.Client.CurrentUser && eventRunning)
            return;

        emb.WithLocalizedTitle(DiscordEventType.MessageDeleted, TranslationKey.evt_msg_del);
        emb.AddLocalizedField(TranslationKey.str_chn, e.Channel.Mention, true);
        emb.AddLocalizedField(TranslationKey.str_author, e.Message.Author?.Mention, true);

        DiscordAuditLogMessageEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogMessageEntry>(AuditLogActionType.MessageDelete);
        if (entry is not null) {
            if (eventRunning && entry.UserResponsible.IsCurrent)
                return;
        
            DiscordMember? member = await e.Guild.GetMemberAsync(entry.UserResponsible.Id);
            if (member is not null && gcs.IsMemberExempted(e.Guild.Id, member.Id, member.Roles.SelectIds()))
                return;
            if (member is null && e.Message.Author is not null) {
                DiscordMember? author = await e.Guild.GetMemberAsync(e.Message.Author.Id);
                if (author is not null && gcs.IsMemberExempted(e.Guild.Id, author.Id, author.Roles.SelectIds()))
                    return;
            }
            emb.AddFieldsFromAuditLogEntry(entry);
        }

        if (!string.IsNullOrWhiteSpace(e.Message.Content)) {
            string sanitizedContent = Formatter.BlockCode(Formatter.Strip(e.Message.Content.Truncate(1000)));
            emb.AddLocalizedField(TranslationKey.str_content, sanitizedContent);
            if (bot.Services.GetRequiredService<FilteringService>().TextContainsFilter(e.Guild.Id, e.Message.Content, out _)) {
                LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
                emb.WithDescription(Formatter.Italic(ls.GetString(e.Guild.Id, TranslationKey.rsn_filter_match)));
            }
        }

        if (e.Message.Embeds.Any())
            emb.AddLocalizedField(TranslationKey.str_embeds, e.Message.Embeds.Count, true);
        if (e.Message.Reactions.Any())
            emb.AddLocalizedField(TranslationKey.str_reactions, e.Message.Reactions.Select(r => r.Emoji.GetDiscordName()).JoinWith(" "), true);
        if (e.Message.Stickers.Any())
            emb.AddLocalizedField(TranslationKey.str_stickers, e.Message.Stickers.JoinWith(), true);
        if (e.Message.Attachments.Any())
            emb.AddLocalizedField(TranslationKey.str_attachments, e.Message.Attachments.Select(a => a.ToMaskedUrl()).JoinWith(), true);
        if (e.Message.CreationTimestamp is { })
            emb.AddLocalizedTimestampField(TranslationKey.str_created_at, e.Message.CreationTimestamp, true);

        await logService.LogAsync(e.Channel.Guild, emb);
    }

    [AsyncEventListener(DiscordEventType.MessageUpdated)]
    public static async Task MessageUpdateEventHandlerAsync(TheGodfatherBot bot, MessageUpdateEventArgs e)
    {
        if (e.Guild is null || (e.Author?.IsBot ?? false) || e.Channel is null || e.Message is null || e.Author is null)
            return;

        if (bot.Services.GetRequiredService<BlockingService>().IsChannelBlocked(e.Channel.Id))
            return;

        if (e.Message.Author == bot.Client.CurrentUser && bot.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(e.Channel.Id))
            return;

        if (e.MessageBefore?.Embeds?.Count < e.Message.Embeds?.Count)
            return;     // Discord added embed(s)

        LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
        FilteringService fs = bot.Services.GetRequiredService<FilteringService>();
        if (!string.IsNullOrWhiteSpace(e.Message.Content) && fs.TextContainsFilter(e.Guild.Id, e.Message.Content, out Filter? match))
            await SanitizeFilteredMessage(bot, e.Message, match?.OnHitAction);

        if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
            return;

        if (LoggingService.IsChannelExempted(bot, e.Guild, e.Channel, out GuildConfigService gcs))
            return;

        DiscordMember? member = await e.Guild.GetMemberAsync(e.Author.Id);
        if (member is not null && gcs.IsMemberExempted(e.Guild.Id, member.Id, member.Roles.SelectIds()))
            return;

        string jumplink = Formatter.MaskedUrl(ls.GetString(e.Guild.Id, TranslationKey.str_jumplink), e.Message.JumpLink);
        emb.WithLocalizedTitle(DiscordEventType.MessageUpdated, TranslationKey.evt_msg_upd, jumplink);
        emb.AddLocalizedField(TranslationKey.str_location, e.Channel.Mention, true);
        emb.AddLocalizedField(TranslationKey.str_author, e.Message.Author?.Mention, true);
        emb.AddLocalizedPropertyChangeField(TranslationKey.str_pinned, e.MessageBefore?.Pinned, e.Message.Pinned);

        emb.AddLocalizedField(
            TranslationKey.str_upd_bef,
            TranslationKey.fmt_msg_cre(
                ls.GetLocalizedTimeString(e.Guild.Id, e.Message.CreationTimestamp, unknown: true),
                e.MessageBefore?.Embeds?.Count ?? 0,
                e.MessageBefore?.Reactions?.Count ?? 0,
                e.MessageBefore?.Attachments?.Count ?? 0,
                FormatContent(e.MessageBefore)
            ),
            inline: false
        );
        emb.AddLocalizedField(
            TranslationKey.str_upd_aft,
            TranslationKey.fmt_msg_upd(
                ls.GetLocalizedTimeString(e.Guild.Id, e.Message.EditedTimestamp, unknown: true),
                e.Message.Embeds?.Count ?? 0,
                e.Message.Reactions?.Count ?? 0,
                e.Message.Attachments?.Count ?? 0,
                FormatContent(e.Message)
            ),
            inline: true
        );

        await logService.LogAsync(e.Channel.Guild, emb);


        static string? FormatContent(DiscordMessage? msg)
            => string.IsNullOrWhiteSpace(msg?.Content) ? null : Formatter.BlockCode(Formatter.Strip(msg.Content.Truncate(700)));
    }


    private static async Task SanitizeFilteredMessage(TheGodfatherBot bot, DiscordMessage msg, Filter.Action? action)
    {
        if (msg.Channel.GuildId is null)
            return;

        LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
        string reason = ls.GetString(msg.Channel.GuildId, TranslationKey.rsn_filter_match);

        if (msg.Channel.PermissionsFor(msg.Channel.Guild.CurrentMember).HasFlag(Permissions.ManageMessages))
            try {
                await msg.DeleteAsync(reason);
                if (action.GetValueOrDefault(Filter.Action.Delete) == Filter.Action.Sanitize)
                    await msg.Channel.LocalizedEmbedAsync(ls, TranslationKey.fmt_filter(msg.Author.Mention, Formatter.Spoiler(Formatter.Strip(msg.Content))));
            } catch {
                await SendErrorReportAsync();
            }
        else
            await SendErrorReportAsync();

        if (action != Filter.Action.Delete && action != Filter.Action.Sanitize) {
            DiscordMember? member = await msg.Channel.Guild.GetMemberAsync(msg.Author.Id);
            if (member is null)
                return;
            if (Enum.TryParse(action.ToString(), out Punishment.Action punishment))
                await bot.Services.GetRequiredService<ProtectionService>().PunishMemberAsync(msg.Channel.Guild, member, punishment, reason: reason);
            else
                LogExt.Warning(bot.GetId(msg.Channel.GuildId), "Failed to interpret filter on-hit action as a punishment: {FilterAction}", action!);
        }


        async Task SendErrorReportAsync()
        {
            if (LoggingService.IsLogEnabledForGuild(bot, msg.Channel.GuildId.Value, out LoggingService? logService, out LocalizedEmbedBuilder? emb)) {
                emb.WithColor(DiscordColor.Red);
                emb.WithLocalizedDescription(TranslationKey.err_f(msg.Channel.Mention));
                await logService.LogAsync(msg.Channel.Guild, emb);
            }
        }
    }
}