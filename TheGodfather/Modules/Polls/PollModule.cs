#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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
            Poll.RegisterPollInChannel(poll, ctx.Channel.Id);
            try {
                await ReplyWithEmbedAsync(ctx, "And what will be the possible answers? (separate with semicolon ``;``)", ":question:")
                    .ConfigureAwait(false);
                var options = await InteractivityUtil.WaitAndParsePollOptionsAsync(ctx)
                    .ConfigureAwait(false);
                if (options.Count < 2)
                    throw new CommandFailedException("Not enough disctict poll options provided (min. 2)!");
                poll.SetOptions(options);
                await ctx.RespondAsync(embed: poll.EmbedPoll())
                    .ConfigureAwait(false);
                await poll.RunAsync(timeout)
                    .ConfigureAwait(false);
                await ctx.RespondAsync(embed: poll.EmbedPollResults())
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

        #region COMMAND_VOTE
        [Command("vote")]
        [Description("Vote for an option in the current running poll.")]
        [UsageExample("!poll Do you vote for User1 or User2?")]
        public async Task VoteAsync(CommandContext ctx,
                                   [Description("Option to vote for.")] int option)
        {
            var poll = Poll.GetPollInChannel(ctx.Channel.Id);
            if (poll == null || !poll.Running)
                throw new CommandFailedException("There are no polls running in this channel.");

            if (!poll.IsValidVote(option))
                throw new CommandFailedException($"Invalid poll option. Valid range: [1, {poll.OptionCount}].");

            if (poll.UserVoted(ctx.User.Id))
                throw new CommandFailedException("You have already voted in this poll!");

            poll.VoteFor(ctx.User.Id, option);
            await ReplyWithEmbedAsync(ctx, $"{ctx.User.Mention} voted for: **{poll.OptionWithId(option)}** in poll: {Formatter.Italic($"\"{poll.Question}\"")}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_POLLR
        [Command("pollr")]
        [Description("Starts a poll with reactions in the channel.")]
        [Aliases("voter")]
        [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
        [ListeningCheckAttribute]
        public async Task Pollr(CommandContext ctx, 
                               [Description("Options")] params DiscordEmoji[] options)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var poll_options = options.Select(e => e.ToString());

            var embed = new DiscordEmbedBuilder {
                Title = "Poll time!",
                Description = string.Join(" ", poll_options)
            };
            var msg = await ctx.RespondAsync(embed: embed)
                .ConfigureAwait(false);

            for (var i = 0; i < options.Length; i++)
                await msg.CreateReactionAsync(options[i]).ConfigureAwait(false);

            var poll_result = await interactivity.CollectReactionsAsync(msg, TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            var results = poll_result.Reactions.Where(kvp => options.Contains(kvp.Key))
                .Select(kvp => $"{kvp.Key} : {kvp.Value}");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = "Results:",
                Description = string.Join("\n", results)
            }.Build()).ConfigureAwait(false);
        }
        #endregion
    }
}
