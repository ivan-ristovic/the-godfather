#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Polls
{
    [Group("vote")]
    [Description("Commands for voting in running polls. If invoked without subcommands, registers a vote in the current poll to the option you entered.")]
    [Aliases("votefor", "vf")]
    [UsageExample("!vote 1")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class VotingModule : TheGodfatherBaseModule
    {

        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
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
            await ctx.RespondWithIconEmbedAsync($"{ctx.User.Mention} voted for: {Formatter.Bold(poll.OptionWithId(option))} in poll: {Formatter.Italic($"\"{poll.Question}\"")}")
                .ConfigureAwait(false);
        }


        #region COMMAND_CANCEL
        [Command("cancel")]
        [Description("Vote for an option in the current running poll.")]
        [Aliases("c", "reset")]
        [UsageExample("!vote cancel")]
        public async Task CancelAsync(CommandContext ctx)
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
    }
}
