#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("nunchi")]
        [Description("Nunchi game commands")]
        [Aliases("n")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(3, 5, CooldownBucketType.Channel)]
        [ListeningCheckAttribute]
        public class NunchiModule
        {
            #region PRIVATE_FIELDS
            private ConcurrentDictionary<ulong, Nunchi> _games = new ConcurrentDictionary<ulong, Nunchi>();
            #endregion


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (_games.ContainsKey(ctx.Channel.Id)) {
                    await JoinGameAsync(ctx)
                        .ConfigureAwait(false);
                    return;
                }

                Nunchi game = new Nunchi(ctx.Client, ctx.Channel.Id);
                if (!_games.TryAdd(ctx.Channel.Id, game))
                    throw new CommandFailedException("Failed to create a new nunchi game! Please try again.");
                await ctx.RespondAsync("The game will start in 30s or when there are 10 participants. Type " + Formatter.InlineCode("!game nunchi") + " to join the game.")
                    .ConfigureAwait(false);

                await JoinGameAsync(ctx)
                    .ConfigureAwait(false);
                await Task.Delay(30000)
                    .ConfigureAwait(false);

                if (game.ParticipantCount > 1) {
                    await game.PlayAsync()
                        .ConfigureAwait(false);
                    if (game.Winner != null)
                        await ctx.Services.GetService<DatabaseService>().UpdateUserStatsAsync(game.Winner.Id, "nunchi_won")
                            .ConfigureAwait(false);
                } else {
                    await ctx.RespondAsync("Not enough users joined the game.")
                        .ConfigureAwait(false);
                }

                if (!_games.TryRemove(ctx.Channel.Id, out _))
                    throw new CommandFailedException("Failed to stop a nunchi game! Please report this.");
            }


            #region COMMAND_NUNCHI_JOIN
            [Command("join")]
            [Description("Join a nunchi game.")]
            [Aliases("+", "compete")]
            public async Task JoinGameAsync(CommandContext ctx)
            {
                if (_games[ctx.Channel.Id].GameRunning)
                    throw new CommandFailedException("Game already started, you can't join it.");

                if (_games[ctx.Channel.Id].ParticipantCount >= 10)
                    throw new CommandFailedException("Game is full, kthxbye.");

                if (!_games[ctx.Channel.Id].AddParticipant(ctx.User.Id))
                    throw new CommandFailedException("You are already participating in the game!");

                await ctx.RespondAsync($"{ctx.User.Mention} joined the game.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_NUNCHI_RULES
            [Command("rules")]
            [Description("Explain the game.")]
            [Aliases("help")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.RespondAsync(
                    "I will start by typing a number. Users have to count up by 1 from that number. " +
                    "If someone makes a mistake (types an incorrent number, or repeats the same number) " +
                    "they are out of the game. If nobody posts a number 20s after the last number was posted, " +
                    "then the user that posted that number wins the game."
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}