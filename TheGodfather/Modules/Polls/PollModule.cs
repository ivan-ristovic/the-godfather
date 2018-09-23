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
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;
using TheGodfather.Modules.Polls.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Polls
{
    [Group("poll"), Module(ModuleType.Polls), NotBlocked, UsesInteractivity]
    [Description("Starts a new poll in the current channel. You can provide also the time for the poll to run.")]
    [UsageExamples("!poll Do you vote for User1 or User2?",
                   "!poll 5m Do you vote for User1 or User2?")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class PollModule : TheGodfatherModule
    {

        public PollModule(SharedData shared, DBService db)
            : base(shared, db)
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

            if (PollService.IsPollRunningInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Another poll is already running in this channel.");

            if (timeout < TimeSpan.FromSeconds(10) || timeout >= TimeSpan.FromDays(1))
                throw new InvalidCommandUsageException("Poll cannot run for less than 10 seconds or more than 1 day(s).");

            var poll = new Poll(ctx.Client.GetInteractivity(), ctx.Channel, question);
            if (!PollService.RegisterPollInChannel(poll, ctx.Channel.Id))
                throw new CommandFailedException("Failed to start the poll. Please try again.");
            try {
                await this.InformAsync(ctx, StaticDiscordEmoji.Question, "And what will be the possible answers? (separate with a semicolon)");
                var options = await ctx.WaitAndParsePollOptionsAsync();
                if (options == null || options.Count < 2 || options.Count > 10)
                    throw new CommandFailedException("Poll must have minimum 2 and maximum 10 options!");
                poll.Options = options;

                await poll.RunAsync(timeout);

            } finally {
                PollService.UnregisterPollInChannel(ctx.Channel.Id);
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
        [UsageExamples("!poll stop")]
        [RequireUserPermissions(Permissions.Administrator)]
        public Task StopAsync(CommandContext ctx)
        {
            var poll = PollService.GetPollInChannel(ctx.Channel.Id);
            if (poll == null || poll is ReactionsPoll)
                throw new CommandFailedException("There are no text polls running in this channel.");

            poll.Stop();

            return Task.CompletedTask;
        }
        #endregion
    }
}
