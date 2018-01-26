#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
    [Group("messages", CanInvokeWithoutSubcommand = false)]
    [Description("Commands to manipulate messages on the channel.")]
    [Aliases("m", "msg", "msgs", "message")]
    [Cooldown(2, 5, CooldownBucketType.User)]
    [PreExecutionCheck]
    public class MessageAdminModule : GodfatherBaseModule
    {

        public MessageAdminModule(SharedData shared, DatabaseService db) : base(shared, db) { }


        #region COMMAND_MESSAGES_ATTACHMENTS
        [Command("attachments")]
        [Description("Print all message attachments.")]
        [Aliases("a", "files", "la")]
        public async Task ListAttachmentsAsync(CommandContext ctx,
                                              [Description("Message ID.")] ulong id = 0)
        {
            DiscordMessage msg;
            if (id != 0) {
                msg = await ctx.Channel.GetMessageAsync(id)
                    .ConfigureAwait(false);
            } else {
                var msgs = await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1);
                msg = msgs.First();
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
        [Command("deletefrom")]
        [Description("Deletes given amount of most-recent messages from given user.")]
        [Aliases("-user", "deluser", "du", "dfu", "delfrom")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteMessagesFromUserAsync(CommandContext ctx, 
                                                     [Description("User.")] DiscordUser user,
                                                     [Description("Amount.")] int amount = 5,
                                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            if (user == null)
                throw new InvalidCommandUsageException("User missing.");
            if (amount <= 0 || amount > 100)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 100].");

            var msgs = await ctx.Channel.GetMessagesAsync(100)
                .ConfigureAwait(false);
            await ctx.Channel.DeleteMessagesAsync(msgs.Where(m => m.Author.Id == user.Id).Take(amount), GetReasonString(ctx, reason))
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_DELETE_REGEX
        [Command("deleteregex")]
        [Description("Deletes given amount of most-recent messages that match a given regular expression.")]
        [Aliases("-regex", "delregex", "dr", "dfr")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteMessagesFromRegexAsync(CommandContext ctx,
                                                      [Description("User.")] string pattern,
                                                      [Description("Amount.")] int amount = 5,
                                                      [RemainingText, Description("Reason.")] string reason = null)
        {
            if (pattern == null)
                throw new InvalidCommandUsageException("Regex missing.");
            if (amount <= 0 || amount > 100)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 100].");

            Regex regex;
            try {
                regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            } catch (ArgumentException e) {
                throw new CommandFailedException("Pattern parsing error.", e);
            }

            var msgs = await ctx.Channel.GetMessagesAsync(100)
                .ConfigureAwait(false);
            await ctx.Channel.DeleteMessagesAsync(msgs.Where(m => regex.IsMatch(m.Content)).Take(amount), GetReasonString(ctx, reason))
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_LISTPINNED
        [Command("listpinned")]
        [Description("List latest amount of pinned messages.")]
        [Aliases("lp", "listpins", "listpin", "pinned")]
        public async Task ListPinnedMessagesAsync(CommandContext ctx)
        {
            var pinned = await ctx.Channel.GetPinnedMessagesAsync()
                .ConfigureAwait(false);

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
        
        #region COMMAND_MESSAGES_PIN
        [Command("pin")]
        [Description("Pins the last sent message. If the ID is given, pins that message.")]
        [Aliases("p")]
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

            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_UNPIN
        [Command("unpin")]
        [Description("Unpins the message at given index (starting from 0).")]
        [Aliases("up")]
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
            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_UNPINALL
        [Command("unpinall")]
        [Description("Unpins all pinned messages.")]
        [Aliases("upa")]
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
            await ReplySuccessAsync(ctx, failed > 0 ? $"Failed to unpin {failed} messages!" : "All messages successfully unpinned!")
                .ConfigureAwait(false);
        }
        #endregion
    }
}