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
    [Group("poll")]
    [Description("Miscellaneous poll commands. If invoked without a subcommand, starts a new poll in the channel.")]
    [Aliases("vote")]
    [UsageExample("!poll Do you vote for User1 or User2?")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class PollModule : TheGodfatherBaseModule
    {

        public PollModule(SharedData shared) : base(shared) { }

        
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx, 
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
                await poll.RunAsync(TimeSpan.FromSeconds(30))
                    .ConfigureAwait(false);
                await ctx.RespondAsync(embed: poll.EmbedPollResults())
                    .ConfigureAwait(false);
            } finally {
                Poll.UnregisterPollInChannel(ctx.Channel.Id);
            }
        }

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
