#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Polls
{
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class ReactionsPollModule : TheGodfatherBaseModule
    {
        #region COMMAND_REACTIONSPOLL
        [Command("reactionspoll"), Priority(1)]
        [Module(ModuleType.Polls)]
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

            if (timeout < TimeSpan.FromSeconds(10) || timeout >= TimeSpan.FromDays(1))
                throw new InvalidCommandUsageException("Poll cannot run for less than 10 seconds or more than 1 day(s).");

            var rpoll = new ReactionsPoll(ctx.Client.GetInteractivity(), ctx.Channel, question);
            if (!Poll.RegisterPollInChannel(rpoll, ctx.Channel.Id))
                throw new CommandFailedException("Failed to start the poll. Please try again.");
            try {
                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Question, "And what will be the possible answers? (separate with semicolon ``;``)")
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
