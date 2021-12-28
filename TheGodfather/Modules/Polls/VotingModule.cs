using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Polls
{
    [Group("vote"), Module(ModuleType.Polls), NotBlocked]
    [Aliases("votefor", "vf")]
    [RequireGuild, Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class VotingModule : TheGodfatherServiceModule<ChannelEventService>
    {
        #region vote
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description(TranslationKey.desc_poll_o)] int option)
        {
            Poll? poll = this.Service.GetEventInChannel<Poll>(ctx.Channel.Id);
            if (poll is null || !poll.IsRunning || poll is ReactionsPoll)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_poll_none);

            option--;
            if (!poll.IsValidVote(option))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_poll_opt_inv(poll.Options.Count));

            if (poll.UserVoted(ctx.User.Id))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_poll_vote);

            poll.VoteFor(ctx.User.Id, option);
            await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_vote(ctx.User.Mention, poll.OptionWithId(option), poll.Question));
        }
        #endregion

        #region vote cancel
        [Command("cancel")]
        [Aliases("c", "reset")]
        public Task CancelAsync(CommandContext ctx)
        {
            Poll? poll = this.Service.GetEventInChannel<Poll>(ctx.Channel.Id);
            if (poll is null || !poll.IsRunning || poll is ReactionsPoll)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_poll_none);

            if (!poll.UserVoted(ctx.User.Id))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_poll_vote_cancel);

            if (!poll.CancelVote(ctx.User.Id))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_poll_vote_cancel_fail);

            return ctx.InfoAsync(this.ModuleColor);
        }
        #endregion
    }
}
