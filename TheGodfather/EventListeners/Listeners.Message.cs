#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Humanizer;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Misc.Extensions;
using TheGodfather.Modules.Reactions.Common;
using TheGodfather.Modules.Reactions.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.MessagesBulkDeleted)]
        public static async Task BulkDeleteEventHandlerAsync(TheGodfatherShard shard, MessageBulkDeleteEventArgs e)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Channel.Guild);
            if (logchn == null)
                return;
            
            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Message, $"Bulk message deletion occured ({e.Messages.Count} total)", $"In channel {e.Channel.Mention}");
            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageCreateEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate)
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id) || shard.SharedData.BlockedUsers.Contains(e.Author.Id))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return;

            if (!string.IsNullOrWhiteSpace(e.Message?.Content) && !e.Message.Content.StartsWith(shard.SharedData.GetGuildPrefix(e.Guild.Id))) {
                ushort rank = shard.SharedData.IncrementMessageCountForUser(e.Author.Id);
                if (rank != 0) {
                    string rankname = await shard.DatabaseService.GetRankAsync(e.Guild.Id, rank);
                    await e.Channel.EmbedAsync($"GG {e.Author.Mention}! You have advanced to level {Formatter.Bold(rank.ToString())} {(string.IsNullOrWhiteSpace(rankname) ? "" : $": {Formatter.Italic(rankname)}")} !", StaticDiscordEmoji.Medal);
                }
            }
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageCreateProtectionHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id) || shard.SharedData.BlockedUsers.Contains(e.Author.Id))
                return;

            if (shard.SharedData.GuildConfigurations[e.Guild.Id].RatelimitEnabled) {
                var rlService = shard.CNext.Services.GetService<RatelimitService>();
                if (!rlService.HandleNewMessage(e.Guild.Id, e.Author.Id, shard.SharedData.GuildConfigurations[e.Guild.Id].RatelimitSensitivity))
                    await rlService.PunishUserAsync(shard, e.Guild, e.Author as DiscordMember, shard.SharedData.GuildConfigurations[e.Guild.Id].RatelimitAction, "Ratelimit triggered");
            }
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageFilterEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id))
                return;

            if (!shard.SharedData.MessageContainsFilter(e.Guild.Id, e.Message.Content))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.ManageMessages))
                return;

            await e.Message.DeleteAsync("_gf: Filter hit");

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn == null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Message, $"Filter triggered");
            emb.AddField("User responsible", e.Message.Author.Mention);
            emb.AddField("Channel", e.Channel.Mention);
            emb.AddField("Content", Formatter.BlockCode(Formatter.Sanitize(e.Message.Content.Truncate(1020))));

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageEmojiReactionEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id) || shard.SharedData.BlockedUsers.Contains(e.Author.Id))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.AddReactions))
                return;

            if (!shard.SharedData.EmojiReactions.ContainsKey(e.Guild.Id))
                return;

            EmojiReaction ereaction = shard.SharedData.EmojiReactions[e.Guild.Id]
                .Where(er => er.IsMatch(e.Message?.Content ?? ""))
                .Shuffle()
                .FirstOrDefault();
            if (ereaction != null) {
                try {
                    var emoji = DiscordEmoji.FromName(shard.Client, ereaction.Response);
                    await e.Message.CreateReactionAsync(emoji);
                } catch (ArgumentException) {
                    await shard.DatabaseService.RemoveAllTriggersForEmojiReactionAsync(e.Guild.Id, ereaction.Response);
                }
            }
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageTextReactionEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id) || shard.SharedData.BlockedUsers.Contains(e.Author.Id))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return;

            if (!shard.SharedData.TextReactions.ContainsKey(e.Guild.Id))
                return;

            TextReaction tr = shard.SharedData.TextReactions[e.Guild.Id]?.FirstOrDefault(r => r.IsMatch(e.Message.Content));
            if (tr != null && !tr.IsCooldownActive())
                await e.Channel.SendMessageAsync(tr.Response.Replace("%user%", e.Author.Mention));
        }

        [AsyncEventListener(DiscordEventType.MessageDeleted)]
        public static async Task MessageDeleteEventHandlerAsync(TheGodfatherShard shard, MessageDeleteEventArgs e)
        {
            if (e.Channel.IsPrivate || e.Message == null)
                return;

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn == null)
                return;

            if (await shard.DatabaseService.IsEntityExemptedAsync(e.Guild.Id, e.Channel.Id, EntityType.Channel))
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Message, "Message deleted");
            emb.AddField("Location", e.Channel.Mention, inline: true);
            emb.AddField("Author", e.Message.Author?.Mention ?? _unknown, inline: true);

            var entry = await e.Guild.GetFirstAuditLogEntryAsync(AuditLogActionType.MessageDelete);
            if (entry != null && entry is DiscordAuditLogMessageEntry mentry) {
                if (await shard.DatabaseService.IsEntityExemptedAsync(e.Guild.Id, mentry.UserResponsible.Id, EntityType.Member))
                    return;
                DiscordMember member = await e.Guild.GetMemberAsync(mentry.UserResponsible.Id);
                foreach (DiscordRole role in member?.Roles)
                    if (await shard.DatabaseService.IsEntityExemptedAsync(e.Guild.Id, role.Id, EntityType.Role))
                        return;

                emb.AddField("User responsible", mentry.UserResponsible.Mention, inline: true);
                if (!string.IsNullOrWhiteSpace(mentry.Reason))
                    emb.AddField("Reason", mentry.Reason);
                emb.WithFooter(mentry.CreationTimestamp.ToUtcTimestamp(), mentry.UserResponsible.AvatarUrl);
            }

            if (!string.IsNullOrWhiteSpace(e.Message.Content)) {
                emb.AddField("Content", $"{Formatter.BlockCode(string.IsNullOrWhiteSpace(e.Message.Content) ? "<empty content>" : e.Message.Content.Truncate(1020))}");
                if (shard.SharedData.MessageContainsFilter(e.Guild.Id, e.Message.Content))
                    emb.WithDescription(Formatter.Italic("Message contained a filter."));
            }
            if (e.Message.Embeds.Any())
                emb.AddField("Embeds", e.Message.Embeds.Count.ToString(), inline: true);
            if (e.Message.Reactions.Any())
                emb.AddField("Reactions", string.Join(" ", e.Message.Reactions.Select(r => r.Emoji.GetDiscordName())), inline: true);
            if (e.Message.Attachments.Any())
                emb.AddField("Attachments", string.Join("\n", e.Message.Attachments.Select(a => a.FileName)), inline: true);
            if (e.Message.CreationTimestamp != null)
                emb.AddField("Message creation time", e.Message.CreationTimestamp.ToUtcTimestamp(), inline: true);

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.MessageUpdated)]
        public static async Task MessageUpdateEventHandlerAsync(TheGodfatherShard shard, MessageUpdateEventArgs e)
        {

            if (e.Author.IsBot || e.Channel.IsPrivate || e.Author == null || e.Message == null)
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id))
                return;

            if (e.Message.Content != null && shard.SharedData.MessageContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Message.DeleteAsync("_gf: Filter hit after update");
                } catch {

                }
            }

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn == null || !e.Message.IsEdited)
                return;

            if (await shard.DatabaseService.IsEntityExemptedAsync(e.Guild.Id, e.Channel.Id, EntityType.Channel))
                return;
            if (await shard.DatabaseService.IsEntityExemptedAsync(e.Guild.Id, e.Author.Id, EntityType.Member))
                return;
            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
            foreach (DiscordRole role in member?.Roles)
                if (await shard.DatabaseService.IsEntityExemptedAsync(e.Guild.Id, role.Id, EntityType.Role))
                    return;

            string pcontent = string.IsNullOrWhiteSpace(e.MessageBefore?.Content) ? "" : e.MessageBefore.Content.Truncate(700);
            string acontent = string.IsNullOrWhiteSpace(e.Message?.Content) ? "" : e.Message.Content.Truncate(700);
            string ctime = e.Message.CreationTimestamp != null ? e.Message.CreationTimestamp.ToUtcTimestamp() : _unknown;
            string etime = e.Message.EditedTimestamp != null ? e.Message.EditedTimestamp.Value.ToUtcTimestamp() : _unknown;
            string bextra = $"Embeds: {e.MessageBefore?.Embeds?.Count ?? 0}, Reactions: {e.MessageBefore?.Reactions?.Count ?? 0}, Attachments: {e.MessageBefore?.Attachments?.Count ?? 0}";
            string aextra = $"Embeds: {e.Message.Embeds.Count}, Reactions: {e.Message.Reactions.Count}, Attachments: {e.Message.Attachments.Count}";

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Message, "Message updated");
            emb.WithDescription(Formatter.MaskedUrl("Jump to message", e.Message.JumpLink));
            emb.AddField("Location", e.Channel.Mention, inline: true);
            emb.AddField("Author", e.Message.Author?.Mention ?? _unknown, inline: true);
            emb.AddField("Before update", $"Created {ctime}\n{bextra}\nContent:{Formatter.BlockCode(Formatter.Sanitize(pcontent))}");
            emb.AddField("After update", $"Edited {etime}\n{aextra}\nContent:{Formatter.BlockCode(Formatter.Sanitize(acontent))}");

            await logchn.SendMessageAsync(embed: emb.Build());
        }
    }
}
