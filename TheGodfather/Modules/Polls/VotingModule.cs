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
    [Group("vote"), Module(ModuleType.Polls)]
    [Description("Commands for voting in running polls. If invoked without subcommands, registers a vote in the current poll to the option you entered.")]
    [Aliases("votefor", "vf")]
    [UsageExamples("!vote 1")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class VotingModule : TheGodfatherModule
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
            await ctx.InformSuccessAsync($"{ctx.User.Mention} voted for: {Formatter.Bold(poll.OptionWithId(option))} in poll: {Formatter.Italic($"\"{poll.Question}\"")}")
                .ConfigureAwait(false);
        }


        #region COMMAND_CANCEL
        [Command("cancel"), Module(ModuleType.Polls)]
        [Description("Vote for an option in the current running poll.")]
        [Aliases("c", "reset")]
        [UsageExamples("!vote cancel")]
        public async Task CancelAsync(CommandContext ctx)
        {
            var poll = Poll.GetPollInChannel(ctx.Channel.Id);
            if (poll == null || !poll.Running || poll is ReactionsPoll)
                throw new CommandFailedException("There are no polls running in this channel.");
            
            if (!poll.UserVoted(ctx.User.Id))
                throw new CommandFailedException("You have not voted in this poll!");

            if (!poll.CancelVote(ctx.User.Id))
                throw new CommandFailedException("Failed to cancel your vote!");

            await ctx.InformSuccessAsync("Your vote has been cancelled!")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
