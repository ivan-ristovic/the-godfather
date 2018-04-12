#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Polls
{
    [Group("poll"), Module(ModuleType.Polls)]
    [Description("Starts a new poll in the current channel. You can provide also the time for the poll to run.")]
    [UsageExample("!poll Do you vote for User1 or User2?")]
    [UsageExample("!poll 5m Do you vote for User1 or User2?")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class PollModule : TheGodfatherBaseModule
    {

        [GroupCommand, Priority(2)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Time for poll to run.")] TimeSpan timeout,
                                           [RemainingText, Description("Question.")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException("Poll requires a question.");

            if (Poll.RunningInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Another poll is already running in this channel.");

            if (timeout < TimeSpan.FromSeconds(10) || timeout >= TimeSpan.FromDays(1))
                throw new InvalidCommandUsageException("Poll cannot run for less than 10 seconds or more than 1 day(s).");

            var poll = new Poll(ctx.Client.GetInteractivity(), ctx.Channel, question);
            if (!Poll.RegisterPollInChannel(poll, ctx.Channel.Id))
                throw new CommandFailedException("Failed to start the poll. Please try again.");
            try {
                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Question, "And what will be the possible answers? (separate with semicolon ``;``)")
                    .ConfigureAwait(false);
                var options = await ctx.WaitAndParsePollOptionsAsync()
                    .ConfigureAwait(false);
                if (options.Count < 2 || options.Count > 10)
                    throw new CommandFailedException("Poll must have minimum 2 and maximum 10 options!");
                poll.SetOptions(options);
                await poll.RunAsync(timeout)
                    .ConfigureAwait(false);
            } finally {
                Poll.UnregisterPollInChannel(ctx.Channel.Id);
            }
        }

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Question.")] string question,
                                     [Description("Time for poll to run.")] TimeSpan timeout)
            => ExecuteGroupAsync(ctx, timeout, question);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Question.")] string question)
            => ExecuteGroupAsync(ctx, TimeSpan.FromMinutes(1), question);


        #region COMMAND_STOP
        [Command("stop"), Module(ModuleType.Polls)]
        [Description("Stops a running poll.")]
        [UsageExample("!poll stop")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task StopAsync(CommandContext ctx)
        {
            await Task.Delay(0).ConfigureAwait(false);

            var poll = Poll.GetPollInChannel(ctx.Channel.Id);
            if (poll == null || poll is ReactionsPoll)
                throw new CommandFailedException("There are no text polls running in this channel.");

            poll.Stop();
        }
        #endregion
    }
}
