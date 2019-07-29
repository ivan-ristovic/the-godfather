#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Polls
{
    [Module(ModuleType.Polls), NotBlocked, UsesInteractivity]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class ReactionsPollModule : TheGodfatherServiceModule<ChannelEventService>
    {

        public ReactionsPollModule(ChannelEventService service, SharedData shared, DatabaseContextBuilder db)
            : base(service, shared, db)
        {
            
        }


        #region COMMAND_REACTIONSPOLL
        [Command("reactionspoll"), Priority(1)]
        [Description("Starts a poll with reactions in the channel.")]
        [Aliases("rpoll", "pollr", "voter")]
        [UsageExampleArgs(":smile: :joy:")]
        public async Task ReactionsPollAsync(CommandContext ctx,
                                            [Description("Time for poll to run.")] TimeSpan timeout,
                                            [RemainingText, Description("Question.")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException("Poll requires a question.");

            if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Another event is already running in this channel.");

            if (timeout < TimeSpan.FromSeconds(10) || timeout >= TimeSpan.FromDays(1))
                throw new InvalidCommandUsageException("Poll cannot run for less than 10 seconds or more than 1 day(s).");

            var rpoll = new ReactionsPoll(ctx.Client.GetInteractivity(), ctx.Channel, ctx.Member, question, timeout);
            this.Service.RegisterEventInChannel(rpoll, ctx.Channel.Id);
            try {
                await this.InformAsync(ctx, StaticDiscordEmoji.Question, "And what will be the possible answers? (separate with a semicolon)");
                System.Collections.Generic.List<string> options = await ctx.WaitAndParsePollOptionsAsync();
                if (options.Count < 2 || options.Count > 10)
                    throw new CommandFailedException("Poll must have minimum 2 and maximum 10 options!");
                rpoll.Options = options;

                await rpoll.RunAsync();
            } finally {
                this.Service.UnregisterEventInChannel(ctx.Channel.Id);
            }
        }

        [Command("reactionspoll"), Priority(0)]
        public Task ReactionsPollAsync(CommandContext ctx,
                                      [RemainingText, Description("Question.")] string question)
            => this.ReactionsPollAsync(ctx, TimeSpan.FromMinutes(1), question);
        #endregion
    }
}
