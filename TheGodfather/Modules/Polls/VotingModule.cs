#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Polls
{
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class VotingModule : TheGodfatherBaseModule
    {
        #region COMMAND_CANCELVOTE
        [Command("cancelvote")]
        [Description("Vote for an option in the current running poll.")]
        [Aliases("cvote", "resetvote")]
        [UsageExample("!vote 1")]
        public async Task VoteAsync(CommandContext ctx)
        {
            var poll = Poll.GetPollInChannel(ctx.Channel.Id);
            if (poll == null || !poll.Running || poll is ReactionsPoll)
                throw new CommandFailedException("There are no polls running in this channel.");
            
            if (!poll.UserVoted(ctx.User.Id))
                throw new CommandFailedException("You have not voted in this poll!");

            if (!poll.CancelVote(ctx.User.Id))
                throw new CommandFailedException("Failed to cancel your vote!");

            await ctx.RespondWithIconEmbedAsync("Your vote has been cancelled!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_VOTE
        [Command("vote")]
        [Description("Vote for an option in the current running poll.")]
        [UsageExample("!vote 1")]
        public async Task VoteAsync(CommandContext ctx,
                                   [Description("Option to vote for.")] int option)
        {
            var poll = Poll.GetPollInChannel(ctx.Channel.Id);
            if (poll == null || !poll.Running || poll is ReactionsPoll)
                throw new CommandFailedException("There are no polls running in this channel.");

            option--;
            if (!poll.IsValidVote(option))
                throw new CommandFailedException($"Invalid poll option. Valid range: [1, {poll.OptionCount}].");

            if (poll.UserVoted(ctx.User.Id))
                throw new CommandFailedException("You have already voted in this poll!");

            poll.VoteFor(ctx.User.Id, option);
            await ctx.RespondWithIconEmbedAsync($"{ctx.User.Mention} voted for: **{poll.OptionWithId(option)}** in poll: {Formatter.Italic($"\"{poll.Question}\"")}")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
