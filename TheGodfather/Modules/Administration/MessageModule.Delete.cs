using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

namespace TheGodfather.Modules.Administration
{
    public partial class MessageModule
    {
        [Group("delete"), UsesInteractivity]
        [Aliases("-", "prune", "del", "d")]
        [RequirePermissions(Permissions.ManageMessages), RequireUserPermissions(Permissions.Administrator)]
        public class MessageDeleteModule : TheGodfatherModule
        {
            #region message delete
            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-message-del-amount")] int amount = 1,
                                               [RemainingText, Description("desc-rsn")] string? reason = null)
            {
                if (amount < 1 || amount > 10000)
                    throw new CommandFailedException(ctx, "cmd-err-msg-del-range", 1, 10000);

                if (amount > 10 && !await ctx.WaitForBoolReplyAsync("q-msg-del", args: amount))
                    return;

                IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesAsync(amount);
                if (!msgs.Any())
                    throw new CommandFailedException(ctx, "cmd-err-msg-del-none");

                await ctx.Channel.DeleteMessagesAsync(msgs, ctx.BuildInvocationDetailsString(reason));
            }
            #endregion

            #region message delete after
            [Command("after")]
            [Aliases("aft", "af")]
            public async Task DeleteMessagesAfterAsync(CommandContext ctx,
                                                      [Description("desc-message")] DiscordMessage message,
                                                      [Description("desc-message-del-amount")] int amount = 1,
                                                      [RemainingText, Description("desc-rsn")] string? reason = null)
            {
                if (amount < 1 || amount > 10000)
                    throw new CommandFailedException(ctx, "cmd-err-msg-del-range", 1, 10000);

                IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesAfterAsync(message.Id, amount);
                await ctx.Channel.DeleteMessagesAsync(msgs, ctx.BuildInvocationDetailsString(reason));
            }
            #endregion

            #region message delete before
            [Command("before")]
            [Aliases("bef", "bf")]
            public async Task DeleteMessagesBeforeAsync(CommandContext ctx,
                                                       [Description("desc-message")] DiscordMessage message,
                                                       [Description("desc-message-del-amount")] int amount = 1,
                                                       [RemainingText, Description("desc-rsn")] string? reason = null)
            {
                if (amount < 1 || amount > 10000)
                    throw new CommandFailedException(ctx, "cmd-err-msg-del-range", 1, 10000);

                IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesBeforeAsync(message.Id, amount);
                await ctx.Channel.DeleteMessagesAsync(msgs, ctx.BuildInvocationDetailsString(reason));
            }
            #endregion

            #region message delete from
            [Command("from"), Priority(1)]
            [Aliases("f", "frm")]
            public async Task DeleteMessagesFromUserAsync(CommandContext ctx,
                                                         [Description("desc-member")] DiscordMember member,
                                                         [Description("desc-msg-del-amount")] int amount = 1,
                                                         [RemainingText, Description("desc-rsn")] string? reason = null)
            {
                if (amount < 1 || amount > 10000)
                    throw new CommandFailedException(ctx, "cmd-err-msg-del-range", 1, 10000);

                IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesAsync(amount);
                await ctx.Channel.DeleteMessagesAsync(msgs.Where(m => m.Author == member), ctx.BuildInvocationDetailsString(reason));
            }

            [Command("from"), Priority(0)]
            public Task DeleteMessagesFromUserAsync(CommandContext ctx,
                                                   [Description("desc-message-del-amount")] int amount,
                                                   [Description("desc-member")] DiscordMember member,
                                                   [RemainingText, Description("desc-rsn")] string? reason = null)
                => this.DeleteMessagesFromUserAsync(ctx, member, amount, reason);
            #endregion

            #region message delete reactions
            [Command("reactions")]
            [Aliases("react", "re")]
            public async Task DeleteReactionsAsync(CommandContext ctx,
                                                  [Description("desc-message")] DiscordMessage? message = null,
                                                  [RemainingText, Description("desc-rsn")] string? reason = null)
            {
                message ??= await ctx.Channel.GetLastMessageAsync();
                if (message is null)
                    throw new CommandFailedException(ctx, "cmd-err-msg-404");

                await message.DeleteAllReactionsAsync(ctx.BuildInvocationDetailsString(reason));
                await ctx.InfoAsync(this.ModuleColor);
            }
            #endregion

            #region message delete regex
            [Command("regex"), Priority(1)]
            [Aliases("r", "rgx", "regexp", "reg")]
            public async Task DeleteMessagesFromRegexAsync(CommandContext ctx,
                                                          [Description("desc-regex")] string pattern,
                                                          [Description("desc-message-del-amount")] int amount = 5,
                                                          [RemainingText, Description("desc-rsn")] string? reason = null)
            {
                if (amount < 1 || amount > 100)
                    throw new CommandFailedException(ctx, "cmd-err-msg-del-range", 1, 100);

                if (!pattern.TryParseRegex(out Regex? regex) || regex is null)
                    throw new CommandFailedException(ctx, "cmd-err-regex");

                IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, amount);
                IEnumerable<DiscordMessage> toDelete = msgs.Where(m => !string.IsNullOrWhiteSpace(m.Content) && regex.IsMatch(m.Content));
                await ctx.Channel.DeleteMessagesAsync(toDelete, ctx.BuildInvocationDetailsString(reason));
            }

            [Command("regex"), Priority(0)]
            public Task DeleteMessagesFromRegexAsync(CommandContext ctx,
                                                    [Description("desc-message-del-amount")] int amount,
                                                    [Description("desc-regex")] string pattern,
                                                    [RemainingText, Description("desc-rsn")] string? reason = null)
                => this.DeleteMessagesFromRegexAsync(ctx, pattern, amount, reason);
            #endregion
        }
    }
}
