#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("message")]
    [Description("Commands for manipulating messages.")]
    [Aliases("m", "msg", "msgs", "messages")]
    [Cooldown(2, 5, CooldownBucketType.User)]
    [ListeningCheck]
    public class MessageAdminModule : GodfatherBaseModule
    {
        #region COMMAND_MESSAGES_ATTACHMENTS
        [Command("attachments")]
        [Description("View all message attachments. If the message is not provided, uses the last sent message before command invocation.")]
        [Aliases("a", "files", "la")]
        [UsageExample("!message attachments")]
        [UsageExample("!message attachments 408226948855234561")]
        public async Task ListAttachmentsAsync(CommandContext ctx,
                                              [Description("Message ID.")] ulong id = 0)
        {
            DiscordMessage msg;
            if (id != 0) {
                msg = await ctx.Channel.GetMessageAsync(id)
                    .ConfigureAwait(false);
            } else {
                var _ = await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1);
                msg = _.First();
            }

            var emb = new DiscordEmbedBuilder() {
                Title = "Attachments:",
                Color = DiscordColor.Azure
            };
            foreach (var attachment in msg.Attachments) 
                emb.AddField($"{attachment.FileName} ({attachment.FileSize} bytes)", attachment.Url);

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_DELETE
        [Command("delete")]
        [Description("Deletes the specified amount of most-recent messages from the channel.")]
        [Aliases("-", "prune", "del", "d")]
        [UsageExample("!messages delete 10")]
        [UsageExample("!messages delete 10 Cleaning spam")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteMessagesAsync(CommandContext ctx, 
                                             [Description("Amount.")] int amount = 5,
                                             [RemainingText, Description("Reason.")] string reason = null)
        {
            if (amount <= 0 || amount > 100)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 100].");

            var msgs = await ctx.Channel.GetMessagesAsync(amount)
                .ConfigureAwait(false);
            await ctx.Channel.DeleteMessagesAsync(msgs, GetReasonString(ctx, reason))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_DELETE_FROM
        [Command("deletefrom"), Priority(1)]
        [Description("Deletes given amount of most-recent messages from given user.")]
        [Aliases("-user", "-u", "deluser", "du", "dfu", "delfrom")]
        [UsageExample("!messages deletefrom @Someone 10 Cleaning spam")]
        [UsageExample("!messages deletefrom 10 @Someone Cleaning spam")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteMessagesFromUserAsync(CommandContext ctx, 
                                                     [Description("User.")] DiscordUser user,
                                                     [Description("Amount.")] int amount = 5,
                                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            if (amount <= 0 || amount > 100)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 100].");

            var msgs = await ctx.Channel.GetMessagesAsync(100)
                .ConfigureAwait(false);
            await ctx.Channel.DeleteMessagesAsync(msgs.Where(m => m.Author.Id == user.Id).Take(amount), GetReasonString(ctx, reason))
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }

        [Command("deletefrom"), Priority(0)]
        public async Task DeleteMessagesFromUserAsync(CommandContext ctx,
                                                     [Description("Amount.")] int amount,
                                                     [Description("User.")] DiscordUser user,
                                                     [RemainingText, Description("Reason.")] string reason = null)
            => await DeleteMessagesFromUserAsync(ctx, user, amount, reason).ConfigureAwait(false);
        #endregion

        #region COMMAND_MESSAGES_DELETE_REACTIONS
        [Command("deletereactions")]
        [Description("Deletes all reactions from the given message.")]
        [Aliases("-reactions", "-r", "delreactions", "dr")]
        [UsageExample("!messages deletereactions 408226948855234561")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteReactionsAsync(CommandContext ctx,
                                              [Description("ID.")] ulong id = 0,
                                              [RemainingText, Description("Reason.")] string reason = null)
        {
            DiscordMessage msg;
            if (id != 0)
                msg = await ctx.Channel.GetMessageAsync(id)
                    .ConfigureAwait(false);
            else {
                var _ = await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1)
                    .ConfigureAwait(false);
                msg = _.First();
            }

            await msg.DeleteAllReactionsAsync(GetReasonString(ctx, reason))
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_DELETE_REGEX
        [Command("deleteregex"), Priority(1)]
        [Description("Deletes given amount of most-recent messages that match a given regular expression.")]
        [Aliases("-regex", "-rx", "delregex", "drx")]
        [UsageExample("!messages deletefrom s+p+a+m+ 10 Cleaning spam")]
        [UsageExample("!messages deletefrom 10 s+p+a+m+ Cleaning spam")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteMessagesFromRegexAsync(CommandContext ctx,
                                                      [Description("Pattern (Regex).")] string pattern,
                                                      [Description("Amount.")] int amount = 5,
                                                      [RemainingText, Description("Reason.")] string reason = null)
        {
            if (amount <= 0 || amount > 100)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 100].");

            Regex regex;
            try {
                regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            } catch (ArgumentException e) {
                throw new CommandFailedException("Pattern parsing error.", e);
            }

            var msgs = await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 100)
                .ConfigureAwait(false);
            await ctx.Channel.DeleteMessagesAsync(msgs.Where(m => regex.IsMatch(m.Content)).Take(amount), GetReasonString(ctx, reason))
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }

        [Command("deleteregex"), Priority(0)]
        public async Task DeleteMessagesFromRegexAsync(CommandContext ctx,
                                                      [Description("Amount.")] int amount,
                                                      [Description("Pattern (Regex).")] string pattern,
                                                      [RemainingText, Description("Reason.")] string reason = null)
            => await DeleteMessagesFromRegexAsync(ctx, pattern, amount, reason).ConfigureAwait(false);
        #endregion

        #region COMMAND_MESSAGES_LISTPINNED
        [Command("listpinned")]
        [Description("List pinned messages in this channel.")]
        [Aliases("lp", "listpins", "listpin", "pinned")]
        [UsageExample("!messages listpinned")]
        public async Task ListPinnedMessagesAsync(CommandContext ctx)
        {
            var pinned = await ctx.Channel.GetPinnedMessagesAsync()
                .ConfigureAwait(false);
            
            if (!pinned.Any()) {
                await ReplyWithEmbedAsync(ctx, "No pinned messages in this channel")
                    .ConfigureAwait(false);
                return;
            }
            
            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx,
                "Pinned messages:",
                pinned,
                m => $"({Formatter.InlineCode(m.CreationTimestamp.ToString())}) {Formatter.Bold(m.Author.Username)} : {(string.IsNullOrWhiteSpace(m.Content) ? "<embedded message>" : m.Content)}" , 
                DiscordColor.Cyan,
                5
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_MODIFY
        [Command("modify")]
        [Description("Modify the given message.")]
        [Aliases("edit", "mod", "e", "m")]
        [UsageExample("!messages modify 408226948855234561 modified text")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task ModifyMessageAsync(CommandContext ctx,
                                            [Description("Message ID.")] ulong id,
                                            [RemainingText, Description("New content.")] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new CommandFailedException("Missing new message content!");

            var msg = await ctx.Channel.GetMessageAsync(id)
                .ConfigureAwait(false);
            await msg.ModifyAsync(content)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_PIN
        [Command("pin")]
        [Description("Pins the message given by ID. If the message is not provided, pins the last sent message before command invocation.")]
        [Aliases("p")]
        [UsageExample("!messages pin")]
        [UsageExample("!messages pin 408226948855234561")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task PinMessageAsync(CommandContext ctx,
                                         [Description("ID.")] ulong id = 0)
        {
            try {
                DiscordMessage msg;
                if (id != 0)
                    msg = await ctx.Channel.GetMessageAsync(id)
                        .ConfigureAwait(false);
                else {
                    var _ = await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1)
                        .ConfigureAwait(false);
                    msg = _.First();
                }

                await msg.PinAsync()
                    .ConfigureAwait(false);
            } catch (BadRequestException e) {
                throw new CommandFailedException("That message cannot be pinned!", e);
            }

            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_UNPIN
        [Command("unpin")]
        [Description("Unpins the message at given index (starting from 1). If the index is not given, unpins the most recent one.")]
        [Aliases("up")]
        [UsageExample("!messages unpin")]
        [UsageExample("!messages unpin 10")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinMessageAsync(CommandContext ctx,
                                           [Description("Index (starting from 1).")] int index = 1)
        {
            var pinned = await ctx.Channel.GetPinnedMessagesAsync()
                .ConfigureAwait(false);

            if (index < 1 || index > pinned.Count)
                throw new CommandFailedException($"Invalid index (must be in range [1-{pinned.Count}]!");

            await pinned.ElementAt(index - 1).UnpinAsync()
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_UNPINALL
        [Command("unpinall")]
        [Description("Unpins all pinned messages in this channel.")]
        [Aliases("upa")]
        [UsageExample("!messages unpinall")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinAllMessagesAsync(CommandContext ctx)
        {
            var pinned = await ctx.Channel.GetPinnedMessagesAsync()
                .ConfigureAwait(false);

            int failed = 0;
            foreach (var m in pinned) {
                try {
                    await m.UnpinAsync()
                        .ConfigureAwait(false);
                } catch {
                    failed++;
                }
            }
            await ReplyWithEmbedAsync(ctx, failed > 0 ? $"Failed to unpin {failed} messages!" : "All messages successfully unpinned!")
                .ConfigureAwait(false);
        }
        #endregion
    }
}