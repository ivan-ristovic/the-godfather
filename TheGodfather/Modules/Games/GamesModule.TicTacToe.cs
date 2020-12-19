#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("tictactoe")]
        [Description("Starts a \"Tic-Tac-Toe\" game. Play a move by writing a number from 1 to 9 corresponding " +
                     "to the field where you wish to play. You can also specify a time window in which players " +
                     "must submit their move.")]
        [Aliases("ttt")]

        public class TicTacToeModule : TheGodfatherServiceModule<ChannelEventService>
        {

            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? movetime = null)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                await this.InformAsync(ctx, Emojis.Question, $"Who wants to play Tic-Tac-Toe with {ctx.User.Username}?");
                DiscordUser opponent = await ctx.WaitForGameOpponentAsync();
                if (opponent is null)
                    return;

                if (movetime?.TotalSeconds < 2 || movetime?.TotalSeconds > 120)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");

                var ttt = new TicTacToeGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                this.Service.RegisterEventInChannel(ttt, ctx.Channel.Id);
                try {
                    await ttt.RunAsync(ctx.Services.GetRequiredService<LocalizationService>());

                    if (!(ttt.Winner is null)) {
                        if (ttt.IsTimeoutReached)
                            await this.InformAsync(ctx, Emojis.Trophy, $"{ttt.Winner.Mention} won due to no replies from opponent!");
                        else
                            await this.InformAsync(ctx, Emojis.Trophy, $"The winner is: {ttt.Winner.Mention}!");

                        await this.Database.UpdateStatsAsync(ttt.Winner.Id, s => s.TicTacToeWon++);
                        if (ttt.Winner.Id == ctx.User.Id)
                            await this.Database.UpdateStatsAsync(opponent.Id, s => s.TicTacToeLost++);
                        else
                            await this.Database.UpdateStatsAsync(ctx.User.Id, s => s.TicTacToeLost++);
                    } else {
                        await this.InformAsync(ctx, Emojis.Joystick, "A draw... Pathetic...");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_TICTACTOE_RULES
            [Command("rules")]
            [Description("Explain the Tic-Tac-Toe game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
            {
                return this.InformAsync(ctx,
                    Emojis.Information,
                    "The object of Tic Tac Toe is to get three in a row. " +
                    "You play on a three by three game board. The first player is known as X and the second is O. " +
                    "Players alternate placing Xs and Os on the game board until either oppent has three in a row " +
                    "or all nine squares are filled."
                );
            }
            #endregion

            #region COMMAND_TICTACTOE_STATS
            [Command("stats")]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            public async Task StatsAsync(CommandContext ctx)
            {
                IReadOnlyList<GameStats> topStats = await this.Database.GetTopTicTacToeStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildTicTacToeStatsString());
                await this.InformAsync(ctx, Emojis.Trophy, $"Top players in Tic-Tac-Toe:\n\n{top}");
            }
            #endregion
        }
    }
}
