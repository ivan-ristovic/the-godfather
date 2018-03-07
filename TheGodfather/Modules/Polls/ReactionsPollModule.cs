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
    public class ReactionsPollModule : TheGodfatherBaseModule
    {
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
            Poll.RegisterPollInChannel(rpoll, ctx.Channel.Id);
            try {
                await ctx.RespondWithIconEmbedAsync("And what will be the possible answers? (separate with semicolon ``;``)", ":question:")
                    .ConfigureAwait(false);
                var options = await InteractivityUtil.WaitAndParsePollOptionsAsync(ctx)
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
