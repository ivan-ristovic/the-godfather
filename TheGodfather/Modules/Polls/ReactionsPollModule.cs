using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Polls
{
    [Group("reactionspoll"), Module(ModuleType.Polls), NotBlocked, UsesInteractivity]
    [Aliases("rpoll", "pollr", "voter")]
    [RequireGuild, Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class ReactionsPollModule : TheGodfatherServiceModule<ChannelEventService>
    {
        #region reactionspoll
        [GroupCommand, Priority(2)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("desc-poll-t")] TimeSpan timeout,
                                           [RemainingText, Description("desc-poll-q")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException(ctx, "cmd-err-poll-q-none");

            if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                throw new InvalidCommandUsageException(ctx, "cmd-err-poll-dup");

            if (timeout < TimeSpan.FromSeconds(10) || timeout >= TimeSpan.FromDays(1))
                throw new InvalidCommandUsageException(ctx, "cmd-err-poll-time", Poll.MinTimeSeconds, Poll.MaxTimeDays);

            var rpoll = new ReactionsPoll(ctx.Client.GetInteractivity(), ctx.Channel, ctx.Member, question, timeout);
            this.Service.RegisterEventInChannel(rpoll, ctx.Channel.Id);
            try {
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Question, "q-poll-ans");
                List<string>? options = await ctx.WaitAndParsePollOptionsAsync();
                if (options is null || options.Count < 2 || options.Count > Poll.MaxPollOptions)
                    throw new CommandFailedException(ctx, "cmd-err-poll-opt", Poll.MaxPollOptions);
                rpoll.Options = options;

                await rpoll.RunAsync(this.Localization);
            } catch (TaskCanceledException) {
                await ctx.FailAsync("cmd-err-poll-cancel");
            } finally {
                this.Service.UnregisterEventInChannel(ctx.Channel.Id);
            }
        }

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-poll-q")] string question,
                                     [Description("desc-poll-t")] TimeSpan timeout)
            => this.ExecuteGroupAsync(ctx, timeout, question);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-poll-q")] string question)
            => this.ExecuteGroupAsync(ctx, TimeSpan.FromMinutes(1), question);
        #endregion

        #region reactionspoll stop
        [Command("stop")]
        [Aliases("end", "cancel")]
        public Task StopAsync(CommandContext ctx)
        {
            Poll? poll = this.Service.GetEventInChannel<Poll>(ctx.Channel.Id);
            if (poll is null or not ReactionsPoll)
                throw new CommandFailedException(ctx, "cmd-err-poll-none");

            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.Administrator) && ctx.User != poll.Initiator)
                throw new CommandFailedException(ctx, "cmd-err-poll-cancel-perms");

            poll.Stop();
            this.Service.UnregisterEventInChannel(ctx.Channel.Id);

            return Task.CompletedTask;
        }
        #endregion
    }
}
