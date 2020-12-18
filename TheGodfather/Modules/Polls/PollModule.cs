using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Polls
{
    [Group("poll"), Module(ModuleType.Polls), NotBlocked, UsesInteractivity]
    [RequireGuild, Cooldown(3, 5, CooldownBucketType.Channel)]
    public class PollModule : TheGodfatherServiceModule<ChannelEventService>
    {
        public PollModule(ChannelEventService service)
            : base(service) { }


        #region poll
        [GroupCommand, Priority(2)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("desc-poll-t")] TimeSpan timeout,
                                           [RemainingText, Description("desc-poll-q")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException(ctx, "cmd-err-poll-q-none");

            if (this.Service.IsEventRunningInChannel(ctx.Channel.Id, out _))
                throw new InvalidCommandUsageException(ctx, "cmd-err-poll-dup");

            if (timeout < TimeSpan.FromSeconds(Poll.MinTimeSeconds) || timeout >= TimeSpan.FromDays(Poll.MaxTimeDays))
                throw new InvalidCommandUsageException(ctx, "cmd-err-poll-time", Poll.MinTimeSeconds, Poll.MaxTimeDays);

            var poll = new Poll(ctx.Client.GetInteractivity(), ctx.Channel, ctx.Member, question, timeout);
            this.Service.RegisterEventInChannel(poll, ctx.Channel.Id);
            try {
                await ctx.InfoAsync(this.ModuleColor, Emojis.Question, "q-poll-ans");
                List<string>? options = await ctx.WaitAndParsePollOptionsAsync();
                if (options is null || options.Count < 2 || options.Count > Poll.MaxPollOptions)
                    throw new CommandFailedException(ctx, "cmd-err-poll-opt", Poll.MaxPollOptions);
                poll.Options = options;

                await poll.RunAsync(ctx.Services.GetRequiredService<LocalizationService>());
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

        #region poll stop
        [Command("stop")]
        [Aliases("end", "cancel")]
        public Task StopAsync(CommandContext ctx)
        {
            Poll poll = this.Service.GetEventInChannel<Poll>(ctx.Channel.Id);
            if (poll is null || poll is ReactionsPoll)
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
