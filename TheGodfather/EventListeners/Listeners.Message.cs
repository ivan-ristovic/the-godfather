using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
using TheGodfather.Misc.Services;
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
        public static async Task BulkDeleteEventHandlerAsync(TheGodfatherShard shard, MessageBulkDeleteEventArgs e)
        {
            if (e.Channel.IsPrivate)
                return;

            GuildConfigService gcs = shard.Services.GetService<GuildConfigService>();
            if (gcs.GetLogChannelForGuild(e.Channel.Guild) is null || gcs.IsChannelExempted(e.Channel.GuildId, e.Channel.Id, e.Channel.ParentId))
                return;

            var emb = new DiscordLogEmbedBuilder("Bulk message deletion occured", null, DiscordEventType.MessagesBulkDeleted);
            emb.AddField("Channel", e.Channel.Mention, inline: true);
            emb.AddField("Count", e.Messages.Count.ToString(), inline: true);

            await shard.Services.GetService<LoggingService>().LogAsync(e.Channel.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageCreateEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot)
                return;

            if (e.Channel.IsPrivate) {
                LogExt.Debug(shard.Id, new[] { "DM message received from {User}:", "{Message}" }, e.Author, e.Message);
                return;
            }

            if (shard.Services.GetService<BlockingService>().IsBlocked(e.Channel.Id, e.Author.Id))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return;

            if (!string.IsNullOrWhiteSpace(e.Message?.Content) && !e.Message.Content.StartsWith(shard.Services.GetService<GuildConfigService>().GetGuildPrefix(e.Guild.Id))) {
                short rank = shard.Services.GetService<UserRanksService>().IncrementMessageCountForUser(e.Author.Id);
                if (rank != 0) {
                    XpRank rankInfo;
                    using (TheGodfatherDbContext db = shard.Database.CreateContext())
                        rankInfo = db.XpRanks.SingleOrDefault(r => r.GuildId == e.Guild.Id && r.Rank == rank);
                    await e.Channel.EmbedAsync($"GG {e.Author.Mention}! You have advanced to level {Formatter.Bold(rank.ToString())} {(rankInfo is null ? "" : $": {Formatter.Italic(rankInfo.Name)}")} !", Emojis.Medal);
                }
            }
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageCreateProtectionHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.Services.GetService<BlockingService>().IsChannelBlocked(e.Channel.Id))
                return;

            CachedGuildConfig gcfg = shard.Services.GetService<GuildConfigService>().GetCachedConfig(e.Guild.Id);
            if (gcfg.RatelimitSettings.Enabled)
                await shard.CNext.Services.GetService<RatelimitService>().HandleNewMessageAsync(e, gcfg.RatelimitSettings);

            if (gcfg.AntispamSettings.Enabled)
                await shard.CNext.Services.GetService<AntispamService>().HandleNewMessageAsync(e, gcfg.AntispamSettings);
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageFilterEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.Services.GetService<BlockingService>().IsChannelBlocked(e.Channel.Id))
                return;

            CachedGuildConfig gcfg = shard.Services.GetService<GuildConfigService>().GetCachedConfig(e.Guild.Id);
            if (gcfg.LinkfilterSettings.Enabled) {
                if (await shard.CNext.Services.GetService<LinkfilterService>().HandleNewMessageAsync(e, gcfg.LinkfilterSettings))
                    return;
            }

            if (!shard.Services.GetService<FilteringService>().TextContainsFilter(e.Guild.Id, e.Message.Content))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.ManageMessages))
                return;

            await e.Message.DeleteAsync("_gf: Filter hit");
            await e.Channel.SendMessageAsync($"{e.Author.Mention} said: {FormatterExt.Spoiler(Formatter.BlockCode(FormatterExt.StripMarkdown(e.Message.Content)))}");
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageReactionEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.Services.GetService<BlockingService>().IsBlocked(e.Channel.Id, e.Author.Id))
                return;

            ReactionsService gdata = shard.Services.GetService<ReactionsService>();

            EmojiReaction triggeredEmojiReaction = gdata.FindMatchingEmojiReactions(e.Guild.Id, e.Message.Content)
                .Shuffle()
                .FirstOrDefault();

            if (!(triggeredEmojiReaction is null) && e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.AddReactions)) {
                try {
                    var emoji = DiscordEmoji.FromName(shard.Client, triggeredEmojiReaction.Response);
                    await e.Message.CreateReactionAsync(emoji);
                } catch (ArgumentException) {
                    using (TheGodfatherDbContext db = shard.Database.CreateContext()) {
                        db.EmojiReactions.RemoveRange(db.EmojiReactions.Where(er => er.GuildId == e.Guild.Id && er.HasSameResponseAs(triggeredEmojiReaction)));
                        await db.SaveChangesAsync();
                    }
                }
            }

            TextReaction triggeredTextReaction = gdata.FindMatchingTextReaction(e.Guild.Id, e.Message.Content);
            if (!(triggeredTextReaction is null) && triggeredTextReaction.CanSend())
                await e.Channel.SendMessageAsync(triggeredTextReaction.Response.Replace("%user%", e.Author.Mention));
        }

        [AsyncEventListener(DiscordEventType.MessageDeleted)]
        public static async Task MessageDeleteEventHandlerAsync(TheGodfatherShard shard, MessageDeleteEventArgs e)
        {
            if (e.Channel.IsPrivate || e.Message is null)
                return;

            GuildConfigService gcs = shard.Services.GetService<GuildConfigService>();
            if (gcs.GetLogChannelForGuild(e.Channel.Guild) is null || gcs.IsChannelExempted(e.Channel.GuildId, e.Channel.Id, e.Channel.ParentId))
                return;

            if (e.Message.Author == e.Client.CurrentUser && shard.Services.GetService<ChannelEventService>().IsEventRunningInChannel(e.Channel.Id))
                return;

            var emb = new DiscordLogEmbedBuilder("Message deleted", null, DiscordEventType.MessageDeleted);
            emb.AddField("Channel", e.Channel.Mention, inline: true);
            emb.AddField("Author", e.Message.Author?.Mention, inline: true);

            DiscordAuditLogMessageEntry entry = await e.Guild.GetLatestAuditLogEntryAsync<DiscordAuditLogMessageEntry>(AuditLogActionType.MessageDelete);
            if (!(entry is null)) {
                DiscordMember member = await e.Guild.GetMemberAsync(entry.UserResponsible.Id);
                if (!(member is null) && gcs.IsMemberExempted(e.Guild.Id, member.Id, member.Roles.Select(r => r.Id).ToList()))
                    return;
                emb.AddInvocationFields(entry.UserResponsible);
                emb.AddField("Reason", entry.Reason, null);
                emb.WithTimestampFooter(entry.CreationTimestamp, entry.UserResponsible.AvatarUrl);
            }

            if (!string.IsNullOrWhiteSpace(e.Message.Content)) {
                emb.AddField("Content", $"{Formatter.BlockCode(string.IsNullOrWhiteSpace(e.Message.Content) ? "<empty content>" : FormatterExt.StripMarkdown(e.Message.Content.Truncate(1000)))}");
                if (shard.Services.GetService<FilteringService>().TextContainsFilter(e.Guild.Id, e.Message.Content))
                    emb.WithDescription(Formatter.Italic("Message contained a filter."));
            }
            if (e.Message.Embeds.Any())
                emb.AddField("Embeds", e.Message.Embeds.Count.ToString(), inline: true);
            if (e.Message.Reactions.Any())
                emb.AddField("Reactions", e.Message.Reactions.Select(r => r.Emoji.GetDiscordName()), inline: true, sep: " ");
            if (e.Message.Attachments.Any())
                emb.AddField("Attachments", e.Message.Attachments.Select(a => a.FileName), inline: true);
            if (e.Message.CreationTimestamp != null)
                emb.AddField("Message creation time", e.Message.CreationTimestamp.ToUtcTimestamp(), inline: true);

            await shard.Services.GetService<LoggingService>().LogAsync(e.Channel.Guild, emb);
        }

        [AsyncEventListener(DiscordEventType.MessageUpdated)]
        public static async Task MessageUpdateEventHandlerAsync(TheGodfatherShard shard, MessageUpdateEventArgs e)
        {
            if (e.Author is null || e.Author.IsBot || e.Channel is null || e.Channel.IsPrivate || e.Message is null)
                return;

            if (shard.Services.GetService<BlockingService>().IsChannelBlocked(e.Channel.Id))
                return;

            if (e.Message.Author == e.Client.CurrentUser && shard.Services.GetService<ChannelEventService>().IsEventRunningInChannel(e.Channel.Id))
                return;

            if (!(e.Message.Content is null) && shard.Services.GetService<FilteringService>().TextContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Message.DeleteAsync("_gf: Filter hit after update");
                    await e.Channel.SendMessageAsync($"{e.Author.Mention} said: {FormatterExt.Spoiler(Formatter.BlockCode(FormatterExt.StripMarkdown(e.Message.Content)))}");
                } catch {

                }
            }

            GuildConfigService gcs = shard.Services.GetService<GuildConfigService>();
            if (gcs.GetLogChannelForGuild(e.Channel.Guild) is null || gcs.IsChannelExempted(e.Channel.GuildId, e.Channel.Id, e.Channel.ParentId))
                return;

            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
            if (!(member is null) && gcs.IsMemberExempted(e.Guild.Id, member.Id, member.Roles.Select(r => r.Id).ToList()))
                return;

            string pcontent = string.IsNullOrWhiteSpace(e.MessageBefore?.Content) ? "" : e.MessageBefore.Content.Truncate(700);
            string acontent = string.IsNullOrWhiteSpace(e.Message?.Content) ? "" : e.Message.Content.Truncate(700);
            string ctime = e.Message.CreationTimestamp == null ? "Unknown" : e.Message.CreationTimestamp.ToUtcTimestamp();
            string etime = e.Message.EditedTimestamp is null ? "Unknown" : e.Message.EditedTimestamp.Value.ToUtcTimestamp();
            string bextra = $"Embeds: {e.MessageBefore?.Embeds?.Count ?? 0}, Reactions: {e.MessageBefore?.Reactions?.Count ?? 0}, Attachments: {e.MessageBefore?.Attachments?.Count ?? 0}";
            string aextra = $"Embeds: {e.Message.Embeds.Count}, Reactions: {e.Message.Reactions.Count}, Attachments: {e.Message.Attachments.Count}";

            var emb = new DiscordLogEmbedBuilder("Message updated", Formatter.MaskedUrl("Jump to message", e.Message.JumpLink), DiscordEventType.MessageUpdated);
            emb.AddField("Location", e.Channel.Mention, inline: true);
            emb.AddField("Author", e.Message.Author?.Mention, inline: true);
            emb.AddField("Before update", $"Created {ctime}\n{bextra}\nContent:{Formatter.BlockCode(FormatterExt.StripMarkdown(pcontent))}");
            emb.AddField("After update", $"Edited {etime}\n{aextra}\nContent:{Formatter.BlockCode(FormatterExt.StripMarkdown(acontent))}");

            await shard.Services.GetService<LoggingService>().LogAsync(e.Channel.Guild, emb);
        }
    }
}
