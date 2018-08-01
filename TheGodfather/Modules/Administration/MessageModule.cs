#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("message"), Module(ModuleType.Administration), NotBlocked]
    [Description("Commands for manipulating messages.")]
    [Aliases("m", "msg", "msgs", "messages")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class MessageModule : TheGodfatherModule
    {

        public MessageModule()
            : base()
        {
            this.ModuleColor = DiscordColor.Azure; 
        }


        #region COMMAND_MESSAGES_ATTACHMENTS
        [Command("attachments")]
        [Description("View all message attachments. If the message is not provided, scans the last sent message before command invocation.")]
        [Aliases("a", "files", "la")]
        [UsageExamples("!message attachments",
                       "!message attachments 408226948855234561")]
        public async Task ListAttachmentsAsync(CommandContext ctx,
                                              [Description("Message ID.")] ulong id = 0)
        {
            DiscordMessage msg;
            if (id != 0) 
                msg = await ctx.Channel.GetMessageAsync(id);
             else 
                msg = (await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1))?.FirstOrDefault();

            if (msg == null)
                throw new CommandFailedException("Cannot retrieve the message!");

            var emb = new DiscordEmbedBuilder() {
                Title = "Attachments:",
                Color = this.ModuleColor
            };
            foreach (DiscordAttachment attachment in msg.Attachments) 
                emb.AddField($"{attachment.FileName} ({attachment.FileSize} bytes)", attachment.Url);

            await ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region COMMAND_MESSAGES_DELETE
        [Command("delete")]
        [Description("Deletes the specified amount of most-recent messages from the channel.")]
        [Aliases("-", "prune", "del", "d")]
        [UsageExamples("!messages delete 10",
                       "!messages delete 10 Cleaning spam")]
        [RequirePermissions(Permissions.ManageMessages), RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteMessagesAsync(CommandContext ctx, 
                                             [Description("Amount.")] int amount = 5,
                                             [RemainingText, Description("Reason.")] string reason = null)
        {
            if (amount < 1 || amount > 10000)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 10000].");

            if (amount > 100 && !await ctx.WaitForBoolReplyAsync($"Are you sure you want to delete {Formatter.Bold(amount.ToString())} messages from this channel?"))
                return;

            IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesAsync(amount);
            if (!msgs.Any())
                throw new CommandFailedException("None of the messages in the given range match your description.");

            await ctx.Channel.DeleteMessagesAsync(msgs, ctx.BuildReasonString(reason));
        }
        #endregion

        #region COMMAND_MESSAGES_DELETE_FROM
        [Command("deletefrom"), Priority(1)]
        [Description("Deletes messages from given user in amount of given messages.")]
        [Aliases("-user", "-u", "deluser", "du", "dfu", "delfrom")]
        [UsageExamples("!messages deletefrom @Someone 10 Cleaning spam",
                       "!messages deletefrom 10 @Someone Cleaning spam")]
        [RequirePermissions(Permissions.ManageMessages), RequireUserPermissions(Permissions.ManageMessages)]
        public async Task DeleteMessagesFromUserAsync(CommandContext ctx, 
                                                     [Description("User whose messages to delete.")] DiscordUser user,
                                                     [Description("Message range.")] int amount = 5,
                                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            if (amount <= 0 || amount > 10000)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 10000].");

            IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesAsync(amount);

            await ctx.Channel.DeleteMessagesAsync(msgs.Where(m => m.Author.Id == user.Id), ctx.BuildReasonString(reason));
        }

        [Command("deletefrom"), Priority(0)]
        public Task DeleteMessagesFromUserAsync(CommandContext ctx,
                                               [Description("Amount.")] int amount,
                                               [Description("User.")] DiscordUser user,
                                               [RemainingText, Description("Reason.")] string reason = null)
            => DeleteMessagesFromUserAsync(ctx, user, amount, reason);
        #endregion

        #region COMMAND_MESSAGES_DELETE_REACTIONS
        [Command("deletereactions")]
        [Description("Deletes all reactions from the given message.")]
        [Aliases("-reactions", "-r", "delreactions", "dr")]
        [UsageExamples("!messages deletereactions 408226948855234561")]
        [RequirePermissions(Permissions.ManageMessages), RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteReactionsAsync(CommandContext ctx,
                                              [Description("ID.")] ulong id = 0,
                                              [RemainingText, Description("Reason.")] string reason = null)
        {
            DiscordMessage msg;
            if (id != 0)
                msg = await ctx.Channel.GetMessageAsync(id);
            else
                msg = (await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1)).FirstOrDefault();

            if (msg == null)
                throw new CommandFailedException("Cannot find the specified message.");

            await msg.DeleteAllReactionsAsync(ctx.BuildReasonString(reason));
            await ctx.InformSuccessAsync();
        }
        #endregion

        #region COMMAND_MESSAGES_DELETE_REGEX
        [Command("deleteregex"), Priority(1)]
        [Description("Deletes given amount of most-recent messages that match a given regular expression withing a given message amount.")]
        [Aliases("-regex", "-rx", "delregex", "drx")]
        [UsageExamples("!messages deletefrom s+p+a+m+ 10 Cleaning spam",
                       "!messages deletefrom 10 s+p+a+m+ Cleaning spam")]
        [RequirePermissions(Permissions.ManageMessages), RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteMessagesFromRegexAsync(CommandContext ctx,
                                                      [Description("Pattern (Regex).")] string pattern,
                                                      [Description("Amount.")] int amount = 100,
                                                      [RemainingText, Description("Reason.")] string reason = null)
        {
            if (amount <= 0 || amount > 100)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 100].");

            if (!pattern.TryParseRegex(out Regex regex))
                throw new CommandFailedException("Regex pattern specified is not valid!");

            IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, amount);

            await ctx.Channel.DeleteMessagesAsync(msgs.Where(m => !string.IsNullOrWhiteSpace(m.Content) && regex.IsMatch(m.Content)), ctx.BuildReasonString(reason));
        }

        [Command("deleteregex"), Priority(0)]
        public Task DeleteMessagesFromRegexAsync(CommandContext ctx,
                                                [Description("Amount.")] int amount,
                                                [Description("Pattern (Regex).")] string pattern,
                                                [RemainingText, Description("Reason.")] string reason = null)
            => DeleteMessagesFromRegexAsync(ctx, pattern, amount, reason);
        #endregion

        #region COMMAND_MESSAGES_LISTPINNED
        [Command("listpinned")]
        [Description("List pinned messages in this channel.")]
        [Aliases("lp", "listpins", "listpin", "pinned")]
        [UsageExamples("!messages listpinned")]
        public async Task ListPinnedMessagesAsync(CommandContext ctx)
        {
            IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();

            if (!pinned.Any()) {
                await ctx.InformFailureAsync("No pinned messages in this channel");
                return;
            }

            var pages = pinned.Select(m => new Page() {
                Content = $"Author: {Formatter.Bold(m.Author.Username)} {m.CreationTimestamp.ToUtcTimestamp()}",
                Embed = m.Embeds.FirstOrDefault() ?? new DiscordEmbedBuilder() {
                    Description = m.Content ?? Formatter.Italic("Empty message.")
                }.Build()
            }).ToList();

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages);
        }
        #endregion

        #region COMMAND_MESSAGES_MODIFY
        [Command("modify")]
        [Description("Modify the given message.")]
        [Aliases("edit", "mod", "e", "m")]
        [UsageExamples("!messages modify 408226948855234561 modified text")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task ModifyMessageAsync(CommandContext ctx,
                                            [Description("Message ID.")] ulong id,
                                            [RemainingText, Description("New content.")] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new CommandFailedException("Missing new message content!");

            DiscordMessage msg = await ctx.Channel.GetMessageAsync(id);
            await msg.ModifyAsync(content);
        }
        #endregion

        #region COMMAND_MESSAGES_PIN
        [Command("pin")]
        [Description("Pins the message given by ID. If the message is not provided, pins the last sent message before command invocation.")]
        [Aliases("p")]
        [UsageExamples("!messages pin",
                       "!messages pin 408226948855234561")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task PinMessageAsync(CommandContext ctx,
                                         [Description("ID.")] ulong id = 0)
        {
            DiscordMessage msg;
            if (id != 0)
                msg = await ctx.Channel.GetMessageAsync(id);
            else
                msg = (await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1))?.FirstOrDefault();

            if (msg == null)
                throw new CommandFailedException("Cannot retrieve the message!");

            await msg.PinAsync();
        }
        #endregion

        #region COMMAND_MESSAGES_UNPIN
        [Command("unpin")]
        [Description("Unpins the message at given index (starting from 1). If the index is not given, unpins the most recent one.")]
        [Aliases("up")]
        [UsageExamples("!messages unpin",
                       "!messages unpin 10")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinMessageAsync(CommandContext ctx,
                                           [Description("Index (starting from 1).")] int index = 1)
        {
            IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();

            if (index < 1 || index > pinned.Count)
                throw new CommandFailedException($"Invalid index (must be in range [1-{pinned.Count}]!");

            await pinned.ElementAt(index - 1).UnpinAsync();
            await ctx.InformSuccessAsync();
        }
        #endregion

        #region COMMAND_MESSAGES_UNPINALL
        [Command("unpinall")]
        [Description("Unpins all pinned messages in this channel.")]
        [Aliases("upa")]
        [UsageExamples("!messages unpinall")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinAllMessagesAsync(CommandContext ctx)
        {
            IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();

            int failed = 0;
            foreach (DiscordMessage m in pinned) {
                try {
                    await m.UnpinAsync();
                } catch {
                    failed++;
                }
            }

            await ctx.InformSuccessAsync(failed > 0 ? $"Failed to unpin {failed} messages!" : null);
        }
        #endregion
    }
}