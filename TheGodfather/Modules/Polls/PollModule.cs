#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Polls
{
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class PollModule : TheGodfatherBaseModule
    {
        #region COMMAND_POLL
        [Command("poll"), Priority(1)]
        [Description("Starts a new poll in the current channel. You can provide also the time for the poll to run.")]
        [UsageExample("!poll Do you vote for User1 or User2?")]
        [UsageExample("!poll 5m Do you vote for User1 or User2?")]
        public async Task PollAsync(CommandContext ctx,
                                   [Description("Time for poll to run.")] TimeSpan timeout,
                                   [RemainingText, Description("Question.")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException("Poll requires a question.");

            if (Poll.RunningInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Another poll is already running in this channel.");

            var poll = new Poll(ctx.Client.GetInteractivity(), ctx.Channel, question);
            if (!Poll.RegisterPollInChannel(poll, ctx.Channel.Id))
                throw new CommandFailedException("Failed to start the poll. Please try again.");
            try {
                await ctx.RespondWithIconEmbedAsync("And what will be the possible answers? (separate with semicolon ``;``)", ":question:")
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

        [Command("poll"), Priority(0)]
        public async Task PollAsync(CommandContext ctx,
                                   [RemainingText, Description("Question.")] string question)
            => await PollAsync(ctx, TimeSpan.FromMinutes(1), question).ConfigureAwait(false);
        #endregion

        #region COMMAND_REACTIONSPOLL
        [Command("reactionspoll"), Priority(1)]
        [Description("Starts a poll with reactions in the channel.")]
        [Aliases("rpoll", "pollr", "voter")]
        [UsageExample("!rpoll :smile: :joy:")]
        public async Task ReactionsPollAsync(CommandContext ctx,
                                            [Description("Time for poll to run.")] TimeSpan timeout,
                                            [RemainingText, Description("Question.")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException("Poll requires a question.");

            if (Poll.RunningInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Another poll is already running in this channel.");

            var rpoll = new ReactionsPoll(ctx.Client.GetInteractivity(), ctx.Channel, question);
            if (!Poll.RegisterPollInChannel(rpoll, ctx.Channel.Id))
                throw new CommandFailedException("Failed to start the poll. Please try again.");
            try {
                await ctx.RespondWithIconEmbedAsync("And what will be the possible answers? (separate with semicolon ``;``)", ":question:")
                    .ConfigureAwait(false);
                var options = await ctx.WaitAndParsePollOptionsAsync()
                    .ConfigureAwait(false);
                if (options.Count < 2 || options.Count > 10)
                    throw new CommandFailedException("Poll must have minimum 2 and maximum 10 options!");
                rpoll.SetOptions(options);
                await rpoll.RunAsync(timeout)
                    .ConfigureAwait(false);
            } finally {
                Poll.UnregisterPollInChannel(ctx.Channel.Id);
            }
        }

        [Command("reactionspoll"), Priority(0)]
        public async Task ReactionsPollAsync(CommandContext ctx,
                                            [RemainingText, Description("Question.")] string question)
            => await ReactionsPollAsync(ctx, TimeSpan.FromMinutes(1), question).ConfigureAwait(false);
        #endregion
    }
}
