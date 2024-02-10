﻿using System.Globalization;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace TheGodfather.Modules.Administration;

public partial class MessageModule
{
    [Group("delete")][UsesInteractivity]
    [Aliases("-", "prune", "del", "d")]
    [RequirePermissions(Permissions.ManageMessages)][RequireUserPermissions(Permissions.Administrator)]
    public class MessageDeleteModule : TheGodfatherModule
    {
        #region message delete
        [GroupCommand, Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_msg_del_amount)] TimeSpan ts,
            [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        {
            var min = TimeSpan.FromMinutes(1);
            var max = DiscordLimits.MessageDeleteBeforeLimit;
            if (ts < min || ts > max) {
                CultureInfo culture = this.Localization.GetGuildCulture(ctx.Guild?.Id);
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_msg_del_ts(min.Humanize(1, culture), max.Humanize(1, culture)));
            }

            if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_msg_del_ts))
                return;

            DateTimeOffset now = DateTimeOffset.Now;
            DiscordMessage from = await ctx.Channel.GetLastMessageAsync() ?? ctx.Message;
            int amount = 25;
            int step = 1;
            do {
                IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesBeforeAsync(from.Id, amount);
                IReadOnlyList<DiscordMessage> toRemove = msgs.Where(m => now - m.Timestamp <= ts).ToList();
                if (!toRemove.Any())
                    break;
                
                await ctx.Channel.DeleteMessagesAsync(toRemove, ctx.BuildInvocationDetailsString(reason));
                
                if (toRemove.Count < msgs.Count)
                    break;
                if (amount < 100)
                    amount *= 2;
                step++;
            } while (step < 10);

            if (step == 10) {
                await ctx.FailAsync(TranslationKey.cmd_err_max_iter);
            }
            
            IReadOnlyList<DiscordMessage> newest = await ctx.Channel.GetMessagesAfterAsync(from.Id);
            await ctx.Channel.DeleteMessagesAsync(newest);
            await from.DeleteAsync();
        }
        
        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_msg_del_amount)] int amount = 1,
            [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        {
            if (amount is < 1 or > 10000)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_msg_del_range(1, 10000));

            if (amount > 10 && !await ctx.WaitForBoolReplyAsync(TranslationKey.q_msg_del(amount)))
                return;

            IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesAsync(amount);
            if (!msgs.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_msg_del_none);

            await ctx.Channel.DeleteMessagesAsync(msgs, ctx.BuildInvocationDetailsString(reason));
        }
        #endregion

        #region message delete after
        [Command("after")]
        [Aliases("aft", "af")]
        public async Task DeleteMessagesAfterAsync(CommandContext ctx,
            [Description(TranslationKey.desc_msg)] DiscordMessage message,
            [Description(TranslationKey.desc_msg_del_amount)] int amount = 1,
            [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        {
            if (amount is < 1 or > 10000)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_msg_del_range(1, 10000));

            IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesAfterAsync(message.Id, amount);
            await ctx.Channel.DeleteMessagesAsync(msgs, ctx.BuildInvocationDetailsString(reason));
        }
        #endregion

        #region message delete before
        [Command("before")]
        [Aliases("bef", "bf")]
        public async Task DeleteMessagesBeforeAsync(CommandContext ctx,
            [Description(TranslationKey.desc_msg)] DiscordMessage message,
            [Description(TranslationKey.desc_msg_del_amount)] int amount = 1,
            [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        {
            if (amount is < 1 or > 10000)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_msg_del_range(1, 10000));

            IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesBeforeAsync(message.Id, amount);
            await ctx.Channel.DeleteMessagesAsync(msgs, ctx.BuildInvocationDetailsString(reason));
        }
        #endregion

        #region message delete from
        [Command("from")][Priority(1)]
        [Aliases("f", "frm")]
        public async Task DeleteMessagesFromUserAsync(CommandContext ctx,
            [Description(TranslationKey.desc_member)] DiscordMember member,
            [Description(TranslationKey.desc_msg_del_amount)] int amount = 1,
            [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        {
            if (amount is < 1 or > 10000)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_msg_del_range(1, 10000));

            IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesAsync(amount);
            await ctx.Channel.DeleteMessagesAsync(msgs.Where(m => m.Author == member), ctx.BuildInvocationDetailsString(reason));
        }

        [Command("from")][Priority(0)]
        public Task DeleteMessagesFromUserAsync(CommandContext ctx,
            [Description(TranslationKey.desc_msg_del_amount)] int amount,
            [Description(TranslationKey.desc_member)] DiscordMember member,
            [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
            => this.DeleteMessagesFromUserAsync(ctx, member, amount, reason);
        #endregion

        #region message delete reactions
        [Command("reactions")]
        [Aliases("react", "re")]
        public async Task DeleteReactionsAsync(CommandContext ctx,
            [Description(TranslationKey.desc_msg)] DiscordMessage? message = null,
            [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        {
            message ??= await ctx.Channel.GetLastMessageAsync();
            if (message is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_msg_404);

            await message.DeleteAllReactionsAsync(ctx.BuildInvocationDetailsString(reason));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region message delete regex
        [Command("regex")][Priority(1)]
        [Aliases("r", "rgx", "regexp", "reg")]
        public async Task DeleteMessagesFromRegexAsync(CommandContext ctx,
            [Description(TranslationKey.desc_regex)] string pattern,
            [Description(TranslationKey.desc_msg_del_amount)] int amount = 5,
            [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
        {
            if (amount is < 1 or > 100)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_msg_del_range(1, 100));

            if (!pattern.TryParseRegex(out Regex? regex) || regex is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_regex);

            if (ctx.Channel.LastMessageId is null)
                return;

            IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId.Value, amount);
            IEnumerable<DiscordMessage> toDelete = msgs.Where(m => !string.IsNullOrWhiteSpace(m.Content) && regex.IsMatch(m.Content));
            await ctx.Channel.DeleteMessagesAsync(toDelete, ctx.BuildInvocationDetailsString(reason));
        }

        [Command("regex")][Priority(0)]
        public Task DeleteMessagesFromRegexAsync(CommandContext ctx,
            [Description(TranslationKey.desc_msg_del_amount)] int amount,
            [Description(TranslationKey.desc_regex)] string pattern,
            [RemainingText][Description(TranslationKey.desc_rsn)] string? reason = null)
            => this.DeleteMessagesFromRegexAsync(ctx, pattern, amount, reason);
        #endregion
    }
}