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
    internal static class MessageListeners
    {
        [AsyncExecuter(EventTypes.MessagesBulkDeleted)]
        public static async Task Client_MessagesBulkDeleted(TheGodfatherShard shard, MessageBulkDeleteEventArgs e)
        {
            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Channel.Guild.Id)
                   .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendMessageAsync(embed: new DiscordEmbedBuilder() {
                    Title = $"Bulk message deletion occured ({e.Messages.Count} total)",
                    Description = $"In channel {e.Channel.Mention}",
                    Color = DiscordColor.SpringGreen
                }).ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.MessageCreated)]
        public static async Task Client_MessageCreated(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || !TheGodfather.Listening || e.Channel.IsPrivate || shard.Shared.BlockedChannels.Contains(e.Channel.Id) || shard.Shared.BlockedUsers.Contains(e.Author.Id))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return;

            var rank = shard.Shared.UpdateMessageCount(e.Author.Id);
            if (rank != -1) {
                var rankname = await shard.Database.GetCustomRankNameForGuildAsync(e.Guild.Id, rank)
                    .ConfigureAwait(false);
                await e.Channel.SendIconEmbedAsync($"GG {e.Author.Mention}! You have advanced to level {Formatter.Bold(rank.ToString())}: {rankname ?? "No custom name set for this rank in this guild"})!", DiscordEmoji.FromName(shard.Client, ":military_medal:"))
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.MessageCreated)]
        public static async Task Client_MessageCreatedFilters(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || !TheGodfather.Listening || e.Message?.Content == null || e.Channel.IsPrivate || shard.Shared.BlockedChannels.Contains(e.Channel.Id))
                return;

            if (shard.Shared.MessageContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Message.DeleteAsync("_gf: Filter hit")
                        .ConfigureAwait(false);
                    shard.Log(LogLevel.Debug,
                        $"Filter triggered in message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    shard.Log(LogLevel.Debug,
                        $"Filter triggered in message but missing permissions to delete!<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                }
                return;
            }
        }

        [AsyncExecuter(EventTypes.MessageCreated)]
        public static async Task Client_MessageCreatedEmojiReactions(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (!TheGodfather.Listening || e.Author.IsBot || e.Message?.Content == null || e.Channel.IsPrivate || shard.Shared.BlockedChannels.Contains(e.Channel.Id) || shard.Shared.BlockedUsers.Contains(e.Author.Id))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.AddReactions))
                return;

            if (shard.Shared.EmojiReactions.ContainsKey(e.Guild.Id)) {
                var ereactions = shard.Shared.EmojiReactions[e.Guild.Id].Where(er => er.Matches(e.Message.Content));
                foreach (var er in ereactions) {
                    shard.Log(LogLevel.Debug,
                        $"Emoji reaction detected: {er.Response}<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                    try {
                        var emoji = DiscordEmoji.FromName(shard.Client, er.Response);
                        await e.Message.CreateReactionAsync(emoji)
                            .ConfigureAwait(false);
                    } catch (ArgumentException) {
                        await shard.Database.RemoveAllEmojiReactionTriggersForReactionAsync(e.Guild.Id, er.Response)
                            .ConfigureAwait(false);
                    } catch (UnauthorizedException) {
                        shard.Log(LogLevel.Debug,
                            $"Emoji reaction trigger found but missing permissions to add reactions!<br>" +
                            $"Message: '{e.Message.Content.Replace('\n', ' ')}<br>" +
                            $"{e.Message.Author.ToString()}<br>" +
                            $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                        );
                        break;
                    }
                }
            }
        }

        [AsyncExecuter(EventTypes.MessageCreated)]
        public static async Task Client_MessageCreatedTextReactions(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || !TheGodfather.Listening || e.Message?.Content == null || e.Channel.IsPrivate || shard.Shared.BlockedChannels.Contains(e.Channel.Id) || shard.Shared.BlockedUsers.Contains(e.Author.Id))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return;

            if (shard.Shared.TextReactions.ContainsKey(e.Guild.Id)) {
                var tr = shard.Shared.TextReactions[e.Guild.Id]?.FirstOrDefault(r => r.Matches(e.Message.Content));
                if (tr != null && tr.CanSend()) {
                    shard.Log(LogLevel.Debug,
                        $"Text reaction detected: {tr.Response}<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                    await e.Channel.SendMessageAsync(tr.Response.Replace("%user%", e.Author.Mention))
                        .ConfigureAwait(false);
                }
            }
        }

        [AsyncExecuter(EventTypes.MessageDeleted)]
        public static async Task Client_MessageDeleted(TheGodfatherShard shard, MessageDeleteEventArgs e)
        {
            if (e.Channel.IsPrivate)
                return;

            var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null && e.Message != null) {
                var emb = new DiscordEmbedBuilder() {
                    Description = $"In channel {e.Channel.Mention}\nFrom {e.Message.Author?.ToString() ?? "<unknown>"}",
                    Color = DiscordColor.SpringGreen
                };

                var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.MessageDelete)
                    .ConfigureAwait(false);
                if (entry != null && DateTime.UtcNow - entry.CreationTimestamp.ToUniversalTime() < TimeSpan.FromSeconds(5) && entry is DiscordAuditLogMessageEntry mentry) {
                    emb.WithTitle($"Messages deleted ({mentry.MessageCount ?? 1} total)");
                    emb.AddField("User responsible", mentry.UserResponsible.Mention, inline: true);
                    if (!string.IsNullOrWhiteSpace(mentry.Reason))
                        emb.AddField("Reason", mentry.Reason);
                    emb.WithFooter($"At {mentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", mentry.UserResponsible.AvatarUrl);
                } else {
                    emb.WithTitle("Message deleted");
                }

                if (!string.IsNullOrWhiteSpace(e.Message.Content) && shard.Shared.MessageContainsFilter(e.Guild.Id, e.Message.Content))
                    emb.AddField("Reason", "Filter triggered");
                if (e.Message.Embeds.Count > 0)
                    emb.AddField("Embeds", e.Message.Embeds.Count.ToString(), inline: true);
                if (e.Message.Reactions.Count > 0)
                    emb.AddField("Reactions", e.Message.Reactions.Count.ToString(), inline: true);
                if (e.Message.Attachments.Count > 0)
                    emb.AddField("Attachments", e.Message.Attachments.Count.ToString(), inline: true);
                emb.AddField("Created at", e.Message.CreationTimestamp != null ? e.Message.CreationTimestamp.ToUniversalTime().ToString() : "<unknown timestamp>", inline: true);
                emb.AddField("Content", $"{Formatter.BlockCode(string.IsNullOrWhiteSpace(e.Message.Content) ? "<empty content>" : e.Message.Content)}");

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        [AsyncExecuter(EventTypes.MessageUpdated)]
        public static async Task Client_MessageUpdated(TheGodfatherShard shard, MessageUpdateEventArgs e)
        {
            if (e.Author == null || e.Author.IsBot || e.Message == null || !TheGodfather.Listening || e.Channel.IsPrivate)
                return;

            if (shard.Shared.BlockedChannels.Contains(e.Channel.Id))
                return;
            
            if (e.Message.Content != null && shard.Shared.MessageContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Message.DeleteAsync("_gf: Filter hit after update")
                        .ConfigureAwait(false);

                    shard.Log(LogLevel.Debug,
                        $"Filter triggered after message edit:<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    shard.Log(LogLevel.Debug,
                        $"Filter triggered in edited message but missing permissions to delete!<br>" +
                        $"Message: '{e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                }
            }

            try {
                var logchn = await shard.Shared.GetLogChannelForGuild(shard.Client, e.Guild.Id)
                    .ConfigureAwait(false);
                if (logchn != null && !e.Author.IsBot && e.Message.EditedTimestamp != null) {
                    var detailspre = $"{Formatter.BlockCode(string.IsNullOrWhiteSpace(e.MessageBefore?.Content) ? "<empty or unknown content>" : e.MessageBefore.Content)}Created at: {(e.Message.CreationTimestamp != null ? e.Message.CreationTimestamp.ToUniversalTime().ToString() : "<unknown>")}; Embeds: {e.MessageBefore?.Embeds?.Count ?? 0}; Reactions: {e.MessageBefore?.Reactions?.Count ?? 0}; Attachments: {e.MessageBefore?.Attachments?.Count ?? 0}";
                    var detailsafter = $"{Formatter.BlockCode(string.IsNullOrWhiteSpace(e.Message?.Content) ? "<empty or unknown content>" : e.Message.Content)}Edited at: {(e.Message.EditedTimestamp != null ? e.Message.EditedTimestamp.ToUniversalTime().ToString() : "<unknown>")}; Embeds: {e.Message.Embeds.Count}; Reactions: {e.Message.Reactions.Count}; Attachments: {e.Message.Attachments.Count}";

                    var emb = new DiscordEmbedBuilder() {
                        Title = "Message updated",
                        Description = $"In channel {e.Channel.Mention}",
                        Color = DiscordColor.SpringGreen
                    };
                    emb.AddField("Before update", detailspre);
                    emb.AddField("After update", detailsafter);

                    await logchn.SendMessageAsync(embed: emb.Build())
                        .ConfigureAwait(false);
                }
            } catch {

            }
        }
    }
}
