using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
using TheGodfather.Misc.Services;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Modules.Reactions.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.EventListeners
{
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

            emb.WithLocalizedTitle(DiscordEventType.MessagesBulkDeleted, "evt-msg-del-bulk", e.Channel);
            emb.AddLocalizedTitleField("str-count", e.Messages.Count, inline: true);
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            foreach (DiscordMessage msg in e.Messages) {
                sw.WriteLine($"[{msg.Timestamp}] {msg.Author}");
                sw.WriteLine(string.IsNullOrWhiteSpace(msg.Content) ? "?" : msg.Content);
                sw.WriteLine(msg.Attachments.Select(a => $"{a.FileName} ({a.FileSize})").JoinWith(", "));
                sw.Flush();
            }
            ms.Seek(0, SeekOrigin.Begin);
            DiscordChannel? chn = gcs.GetLogChannelForGuild(e.Guild);
            await (chn?.SendFileAsync($"{e.Channel.Name}-deleted-messages.txt", ms, embed: emb.Build()) ?? Task.CompletedTask);
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

            if (!string.IsNullOrWhiteSpace(e.Message?.Content)) {
                // TODO move to service
                if (!e.Message.Content.StartsWith(bot.Services.GetRequiredService<GuildConfigService>().GetGuildPrefix(e.Guild.Id))) {
                    short rank = bot.Services.GetRequiredService<UserRanksService>().ChangeXp(e.Author.Id);
                    if (rank != 0) {
                        LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
                        XpRank? rankInfo;
                        using (TheGodfatherDbContext db = bot.Database.CreateContext())
                            rankInfo = await db.XpRanks.FindAsync((long)e.Guild.Id, rank);
                        string rankupStr = ls.GetString(e.Guild.Id, "fmt-rankup", e.Author.Mention, Formatter.Bold(rank.ToString()), rankInfo?.Name ?? " / ");
                        await e.Channel.EmbedAsync(rankupStr, Emojis.Medal);
                    }
                }
            }
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageCreateProtectionHandlerAsync(TheGodfatherBot bot, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Guild is null || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (bot.Services.GetRequiredService<BlockingService>().IsChannelBlocked(e.Channel.Id))
                return;

            CachedGuildConfig? gcfg = bot.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(e.Guild.Id);
            if (gcfg is { }) {
                if (gcfg.RatelimitSettings.Enabled)
                    await bot.Services.GetRequiredService<RatelimitService>().HandleNewMessageAsync(e, gcfg.RatelimitSettings);
                if (gcfg.AntispamSettings.Enabled)
                    await bot.Services.GetRequiredService<AntispamService>().HandleNewMessageAsync(e, gcfg.AntispamSettings);
            }
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static Task MessageCreateBackupHandlerAsync(TheGodfatherBot bot, MessageCreateEventArgs e)
        {
            return e.Guild is null 
                ? Task.CompletedTask 
                : bot.Services.GetRequiredService<BackupService>().BackupAsync(e.Message);
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageFilterEventHandlerAsync(TheGodfatherBot bot, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Guild is null || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (bot.Services.GetRequiredService<BlockingService>().IsChannelBlocked(e.Channel.Id))
                return;

            CachedGuildConfig? gcfg = bot.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(e.Guild.Id);
            if (gcfg?.LinkfilterSettings.Enabled ?? false) {
                if (await bot.Services.GetRequiredService<LinkfilterService>().HandleNewMessageAsync(e, gcfg.LinkfilterSettings))
                    return;
            }

            if (!bot.Services.GetRequiredService<FilteringService>().TextContainsFilter(e.Guild.Id, e.Message.Content, out _))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.ManageMessages))
                return;

            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();

            // TODO automatize, same below in message update handler
            await e.Message.DeleteAsync(ls.GetString(e.Guild.Id, "rsn-filter-match"));
            string sanitizedContent = Formatter.Strip(e.Message.Content);
            string localizedSpoiler = ls.GetString(e.Guild.Id, "fmt-filter", e.Author.Mention, sanitizedContent);
            await e.Channel.SendMessageAsync(localizedSpoiler);
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageReactionEventHandlerAsync(TheGodfatherBot bot, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Guild is null || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (bot.Services.GetRequiredService<BlockingService>().IsBlocked(e.Guild.Id, e.Channel.Id, e.Author.Id))
                return;

            ReactionsService rs = bot.Services.GetRequiredService<ReactionsService>();

            if (e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.AddReactions)) {
                EmojiReaction? er = rs.FindMatchingEmojiReactions(e.Guild.Id, e.Message.Content)
                    .Shuffle()
                    .FirstOrDefault();

                // TODO move to service
                if (er is { }) {
                    try {
                        var client = bot.Client.GetShard(e.Guild.Id);
                        var emoji = DiscordEmoji.FromName(client, er.Response);
                        await e.Message.CreateReactionAsync(emoji);
                    } catch (ArgumentException) {
                        using TheGodfatherDbContext db = bot.Database.CreateContext();
                        db.EmojiReactions.RemoveRange(
                            db.EmojiReactions
                                .Where(r => r.GuildIdDb == (long)e.Guild.Id)
                                .AsEnumerable()
                                .Where(r => r.HasSameResponseAs(er))
                        );
                        await db.SaveChangesAsync();
                    }
                }
            }

            TextReaction? tr = rs.FindMatchingTextReaction(e.Guild.Id, e.Message.Content);
            // TODO move to service
            if (tr is { } && tr.CanSend())
                await e.Channel.SendMessageAsync(tr.Response.Replace("%user%", e.Author.Mention));
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

            if (e.Message.Author == bot.Client.CurrentUser && bot.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(e.Channel.Id))
                return;

            emb.WithLocalizedTitle(DiscordEventType.MessageDeleted, "evt-msg-del");
            emb.AddLocalizedTitleField("str-chn", e.Channel.Mention, inline: true);
            emb.AddLocalizedTitleField("str-author", e.Message.Author?.Mention, inline: true);

            DiscordAuditLogMessageEntry? entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogMessageEntry>(AuditLogActionType.MessageDelete);
            if (entry is { }) {
                DiscordMember? member = await e.Guild.GetMemberAsync(entry.UserResponsible.Id);
                if (member is { } && gcs.IsMemberExempted(e.Guild.Id, member.Id, member.Roles.Select(r => r.Id)))
                    return;
                emb.AddFieldsFromAuditLogEntry(entry);
            }

            if (!string.IsNullOrWhiteSpace(e.Message.Content)) {
                string sanitizedContent = Formatter.BlockCode(Formatter.Strip(e.Message.Content.Truncate(1000)));
                emb.AddLocalizedTitleField("str-content", sanitizedContent, inline: true);
                if (bot.Services.GetRequiredService<FilteringService>().TextContainsFilter(e.Guild.Id, e.Message.Content, out _)) {
                    LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
                    emb.WithDescription(Formatter.Italic(ls.GetString(e.Guild.Id, "rsn-filter-match")));
                }
            }
            if (e.Message.Embeds.Any())
                emb.AddLocalizedTitleField("str-embeds", e.Message.Embeds.Count, inline: true);
            if (e.Message.Reactions.Any())
                emb.AddLocalizedTitleField("str-reactions", e.Message.Reactions.Select(r => r.Emoji.GetDiscordName()).JoinWith(" "), inline: true);
            if (e.Message.Attachments.Any()) {
                emb.AddLocalizedTitleField("str-attachments", e.Message.Attachments.Select(a => ToMaskedUrl(a)).JoinWith(), inline: true);

                static string ToMaskedUrl(DiscordAttachment a)
                    => Formatter.MaskedUrl($"{a.FileName} ({a.FileSize.ToMetric(decimals: 0)}B)", new Uri(a.Url));
            }
            if (e.Message.CreationTimestamp is { })
                emb.AddLocalizedTimestampField("str-created-at", e.Message.CreationTimestamp, inline: true);

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

            LocalizationService ls = bot.Services.GetRequiredService<LocalizationService>();
            if (e.Message.Content is { } && bot.Services.GetRequiredService<FilteringService>().TextContainsFilter(e.Guild.Id, e.Message.Content, out _)) {
                try {
                    await e.Message.DeleteAsync(ls.GetString(e.Guild.Id, "rsn-filter-match"));
                    string sanitizedContent = Formatter.Strip(e.Message.Content);
                    string localizedSpoiler = ls.GetString(e.Guild.Id, "fmt-filter", e.Author.Mention, sanitizedContent);
                    await e.Channel.SendMessageAsync(localizedSpoiler);
                } catch {
                    // TODO
                }
            }

            if (!LoggingService.IsLogEnabledForGuild(bot, e.Guild.Id, out LoggingService logService, out LocalizedEmbedBuilder emb))
                return;

            if (LoggingService.IsChannelExempted(bot, e.Guild, e.Channel, out GuildConfigService gcs))
                return;

            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
            if (member is { } && gcs.IsMemberExempted(e.Guild.Id, member.Id, member.Roles.Select(r => r.Id)))
                return;

            string jumplink = Formatter.MaskedUrl(ls.GetString(e.Guild.Id, "str-jumplink"), e.Message.JumpLink);
            emb.WithLocalizedTitle(DiscordEventType.MessageUpdated, "evt-msg-upd", desc: jumplink);
            emb.AddLocalizedTitleField("str-location", e.Channel.Mention, inline: true);
            emb.AddLocalizedTitleField("str-author", e.Message.Author?.Mention, inline: true);

            emb.AddLocalizedContentField(
                "str-upd-bef",
                "fmt-msg-cre",
                inline: false,
                ls.GetLocalizedTimeString(e.Guild.Id, e.Message.CreationTimestamp, unknown: true),
                e.MessageBefore?.Embeds?.Count ?? 0,
                e.MessageBefore?.Reactions?.Count ?? 0,
                e.MessageBefore?.Attachments?.Count ?? 0,
                FormatContent(e.MessageBefore)
            );
            emb.AddLocalizedContentField(
                "str-upd-aft",
                "fmt-msg-upd",
                inline: true,
                ls.GetLocalizedTimeString(e.Guild.Id, e.Message.EditedTimestamp, unknown: true),
                e.Message.Embeds?.Count ?? 0,
                e.Message.Reactions?.Count ?? 0,
                e.Message.Attachments?.Count ?? 0,
                FormatContent(e.Message)
            );

            await logService.LogAsync(e.Channel.Guild, emb);


            static string? FormatContent(DiscordMessage? msg)
                => string.IsNullOrWhiteSpace(msg?.Content) ? null : Formatter.BlockCode(msg.Content.Truncate(700));
        }
    }
}
