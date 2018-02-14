#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Games.Common;

using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("numberrace")]
        [Description("Number racing game commands.")]
        [Aliases("n", "nunchi", "numbers", "numbersrace")]
        [UsageExample("!game numberrace")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(3, 5, CooldownBucketType.Channel)]
        [ListeningCheck]
        public class NumberRaceModule : GodfatherBaseModule
        {

            public NumberRaceModule(DatabaseService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (Game.RunningInChannel(ctx.Channel.Id)) {
                    if (Game.GetRunningGameInChannel(ctx.Channel.Id) is NumberRace)
                        await JoinGameAsync(ctx).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another game is already running in the current channel.");
                    return;
                }

                var game = new NumberRace(ctx.Client.GetInteractivity(), ctx.Channel);
                Game.RegisterGameInChannel(game, ctx.Channel.Id);
                try {
                    await ReplyWithEmbedAsync(ctx, $"The race will start in 30s or when there are 10 participants. Type {Formatter.InlineCode("!game numberrace")} to join the race.", ":clock1:")
                        .ConfigureAwait(false);
                    await JoinGameAsync(ctx)
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync()
                            .ConfigureAwait(false);

                        if (game.NoReply) {
                            if (game.Winner != null) {
                                await ReplyWithEmbedAsync(ctx, $"{game.Winner.Mention} won due to no replies from other users!", ":trophy:")
                                    .ConfigureAwait(false);
                            } else {
                                await ReplyWithEmbedAsync(ctx, "No replies, aborting Number Race...", ":alarm_clock:")
                                        .ConfigureAwait(false);
                            }
                        } else {
                            await ReplyWithEmbedAsync(ctx, "Winner: " + game.Winner.Mention, ":trophy:")
                                .ConfigureAwait(false);
                        }

                        if (game.Winner != null)
                            await DatabaseService.UpdateUserStatsAsync(game.Winner.Id, "numraces_won")
                                .ConfigureAwait(false);
                    } else {
                        await ReplyWithEmbedAsync(ctx, "Not enough users joined the race.", ":alarm_clock:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_NUMBERRACE_JOIN
            [Command("join")]
            [Description("Join an existing number race game.")]
            [Aliases("+", "compete", "j", "enter")]
            [UsageExample("!game numberrace join")]
            public async Task JoinGameAsync(CommandContext ctx)
            {
                var game = Game.GetRunningGameInChannel(ctx.Channel.Id) as NumberRace;
                if (game == null)
                    throw new CommandFailedException("There is no number race game running in this channel.");

                if (game.GameStarted)
                    throw new CommandFailedException("Race has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Race slots are full (max 10 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User))
                    throw new CommandFailedException("You are already participating in the race!");

                await ReplyWithEmbedAsync(ctx, $"{ctx.User.Mention} joined the game.", ":bicyclist:")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_NUMBERRACE_RULES
            [Command("rules")]
            [Description("Explain the number race rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExample("!game numberrace rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ReplyWithEmbedAsync(
                    ctx,
                    "I will start by typing a number. Users have to count up by 1 from that number. " +
                    "If someone makes a mistake (types an incorrent number, or repeats the same number) " +
                    "they are out of the game. If nobody posts a number 20s after the last number was posted, " +
                    "then the user that posted that number wins the game. The game ends when only one user remains.",
                    ":information_source:"
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}