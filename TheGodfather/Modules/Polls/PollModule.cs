#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Polls
{
    [Group("poll"), Module(ModuleType.Polls), NotBlocked, UsesInteractivity]
    [Description("Starts a new poll in the current channel. You can provide also the time for the poll to run.")]
    [UsageExampleArgs("Do you vote for User1 or User2?", "5m Do you vote for User1 or User2?")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class PollModule : TheGodfatherServiceModule<ChannelEventService>
    {

        public PollModule(ChannelEventService service, SharedData shared, DatabaseContextBuilder db)
            : base(service, shared, db)
        {
            this.ModuleColor = DiscordColor.Orange;
        }


        [GroupCommand, Priority(2)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Time for poll to run.")] TimeSpan timeout,
                                           [RemainingText, Description("Question.")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException("Poll requires a question.");

            if (this.Service.IsEventRunningInChannel(ctx.Channel.Id, out _))
                throw new CommandFailedException("Another event is already running in this channel.");

            if (timeout < TimeSpan.FromSeconds(10) || timeout >= TimeSpan.FromDays(1))
                throw new InvalidCommandUsageException("Poll cannot run for less than 10 seconds or more than 1 day(s).");

            var poll = new Poll(ctx.Client.GetInteractivity(), ctx.Channel, ctx.Member, question, timeout);
            this.Service.RegisterEventInChannel(poll, ctx.Channel.Id);
            try {
                await this.InformAsync(ctx, StaticDiscordEmoji.Question, "And what will be the possible answers? (separate with a semicolon)");
                System.Collections.Generic.List<string> options = await ctx.WaitAndParsePollOptionsAsync();
                if (options is null || options.Count < 2 || options.Count > 10)
                    throw new CommandFailedException("Poll must have minimum 2 and maximum 10 options!");
                poll.Options = options;

                await poll.RunAsync();

            } finally {
                this.Service.UnregisterEventInChannel(ctx.Channel.Id);
            }
        }

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Question.")] string question,
                                     [Description("Time for poll to run.")] TimeSpan timeout)
            => this.ExecuteGroupAsync(ctx, timeout, question);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Question.")] string question)
            => this.ExecuteGroupAsync(ctx, TimeSpan.FromMinutes(1), question);


        #region COMMAND_STOP
        [Command("stop")]
        [Description("Stops a running poll.")]
        [Aliases("end", "cancel")]
        public Task StopAsync(CommandContext ctx)
        {
            Poll poll = this.Service.GetEventInChannel<Poll>(ctx.Channel.Id);
            if (poll is null || poll is ReactionsPoll)
                throw new CommandFailedException("There are no text polls running in this channel.");

            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.Administrator) && ctx.User.Id != poll.Initiator.Id)
                throw new CommandFailedException("You do not have the sufficient permissions to close another person's poll!");

            poll.Stop();
            this.Service.UnregisterEventInChannel(ctx.Channel.Id);

            return Task.CompletedTask;
        }
        #endregion
    }
}
