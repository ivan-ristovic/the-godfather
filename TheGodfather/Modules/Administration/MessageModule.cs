#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("message"), Module(ModuleType.Administration), NotBlocked]
    [Description("Commands for manipulating messages.")]
    [Aliases("m", "msg", "msgs", "messages")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class MessageModule : TheGodfatherModule
    {

        public MessageModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Azure; 
        }


        #region COMMAND_MESSAGES_ATTACHMENTS
        [Command("attachments")]
        [Description("View all message attachments. If the message is not provided, scans the last sent message before command invocation.")]
        [Aliases("a", "files", "la")]
        [UsageExampleArgs("408226948855234561")]
        public async Task ListAttachmentsAsync(CommandContext ctx,
                                              [Description("Message.")] DiscordMessage message = null)
        {
            message = message ?? (await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1))?.FirstOrDefault();

            if (message is null)
                throw new CommandFailedException("Cannot retrieve the message!");

            var emb = new DiscordEmbedBuilder() {
                Title = "Attachments:",
                Color = this.ModuleColor
            };
            foreach (DiscordAttachment attachment in message.Attachments) 
                emb.AddField($"{attachment.FileName} ({attachment.FileSize} bytes)", attachment.Url);

            await ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region COMMAND_MESSAGES_FLAG
        [Command("flag")]
        [Description("Flags the message given by ID for deletion vote. If the message is not provided, flags the last sent message before command invocation.")]
        [Aliases("f")]
        [UsageExampleArgs("408226948855234561")]
        [RequireBotPermissions(Permissions.ManageMessages)]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task FlagMessageAsync(CommandContext ctx,
                                          [Description("Message.")] DiscordMessage msg = null,
                                          [Description("Voting timespan.")] TimeSpan? timespan = null)
        {
            msg = msg ?? (await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1))?.FirstOrDefault();

            if (msg is null)
                throw new CommandFailedException("Cannot retrieve the message!");

            if (timespan?.TotalSeconds < 5 || timespan?.TotalMinutes > 5)
                throw new InvalidCommandUsageException("Timespan cannot be greater than 5 minutes or lower than 5 seconds.");

            IEnumerable<PollEmoji> res = await msg.DoPollAsync(new[] { StaticDiscordEmoji.ArrowUp, StaticDiscordEmoji.ArrowDown }, PollBehaviour.Default, timeout: timespan ?? TimeSpan.FromMinutes(1));
            var votes = res.ToDictionary(pe => pe.Emoji, pe => pe.Voted.Count);
            if (votes.GetValueOrDefault(StaticDiscordEmoji.ArrowDown) > 2 * votes.GetValueOrDefault(StaticDiscordEmoji.ArrowUp)) {
                string sanitized = FormatterExtensions.Spoiler(FormatterExtensions.StripMarkdown(msg.Content));
                await msg.DeleteAsync();
                await ctx.RespondAsync($"{msg.Author.Mention} said: {sanitized}");
            } else {
                await this.InformFailureAsync(ctx, "Not enough downvotes required for deletion.");
            }
        }
        #endregion

        #region COMMAND_MESSAGES_LISTPINNED
        [Command("listpinned")]
        [Description("List pinned messages in this channel.")]
        [Aliases("lp", "listpins", "listpin", "pinned")]
        public async Task ListPinnedMessagesAsync(CommandContext ctx)
        {
            IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();

            if (!pinned.Any()) {
                await this.InformFailureAsync(ctx, "No pinned messages in this channel");
                return;
            }

            IEnumerable<Page> pages = pinned.Select(m => new Page(
                $"Author: {Formatter.Bold(m.Author.Username)} {m.CreationTimestamp.ToUtcTimestamp()}",
                GetFirstEmbedOrDefaultAsBuilder(m)
            ));

            await ctx.Client.GetInteractivity().SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);


            DiscordEmbedBuilder GetFirstEmbedOrDefaultAsBuilder(DiscordMessage m)
            {
                DiscordEmbed em = m.Embeds.FirstOrDefault();
                if (!(em is null))
                    return new DiscordEmbedBuilder(m.Embeds.First());
                return new DiscordEmbedBuilder() {
                    Title = "Jump to",
                    Description = m.Content ?? Formatter.Italic("Empty message."),
                    Url = m.JumpLink.ToString()
                };
            }
        }
        #endregion

        #region COMMAND_MESSAGES_MODIFY
        [Command("modify")]
        [Description("Modify the given message.")]
        [Aliases("edit", "mod", "e", "m")]
        [UsageExampleArgs("408226948855234561 modified text")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task ModifyMessageAsync(CommandContext ctx,
                                            [Description("Message.")] DiscordMessage message,
                                            [RemainingText, Description("New content.")] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new CommandFailedException("Missing new message content!");
            
            await message.ModifyAsync(content);
            await this.InformAsync(ctx, important: false);
        }
        #endregion

        #region COMMAND_MESSAGES_PIN
        [Command("pin")]
        [Description("Pins the message given by ID. If the message is not provided, pins the last sent message before command invocation.")]
        [Aliases("p")]
        [UsageExampleArgs("408226948855234561")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task PinMessageAsync(CommandContext ctx,
                                         [Description("Message.")] DiscordMessage message = null)
        {
            message = message ?? (await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1))?.FirstOrDefault();

            if (message is null)
                throw new CommandFailedException("Cannot retrieve the message!");

            await message.PinAsync();
        }
        #endregion

        #region COMMAND_MESSAGES_UNPIN
        [Command("unpin"), Priority(1)]
        [Description("Unpins the message at given index (starting from 1) or message ID. If the index is not given, unpins the most recent one.")]
        [Aliases("up")]
        [UsageExampleArgs("12345645687955", "10")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinMessageAsync(CommandContext ctx,
                                           [Description("Message.")] DiscordMessage message)
        {
            await message.UnpinAsync();
            await this.InformAsync(ctx, "Removed the specified pin.", important: false);
        }

        [Command("unpin"), Priority(0)]
        public async Task UnpinMessageAsync(CommandContext ctx,
                                           [Description("Index (starting from 1).")] int index = 1)
        {
            IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();

            if (index < 1 || index > pinned.Count)
                throw new CommandFailedException($"Invalid index (must be in range [1, {pinned.Count}]!");

            await pinned.ElementAt(index - 1).UnpinAsync();
            await this.InformAsync(ctx, "Removed the specified pin.", important: false);
        }
        #endregion

        #region COMMAND_MESSAGES_UNPINALL
        [Command("unpinall")]
        [Description("Unpins all pinned messages in this channel.")]
        [Aliases("upa")]
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

            if (failed > 0)
                await this.InformFailureAsync(ctx, $"Failed to unpin {failed} messages!");
            else
                await this.InformAsync(ctx, "Successfully unpinned all messages in this channel", important: false);
        }
        #endregion
    }
}