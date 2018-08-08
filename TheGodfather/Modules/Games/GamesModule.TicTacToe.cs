#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
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
        [UsageExamples("!game tictactoe",
                       "!game tictactoe 10s")]
        public class TicTacToeModule : TheGodfatherModule
        {

            public TicTacToeModule(SharedData shared, DBService db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Teal;
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? movetime = null)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                await InformAsync(ctx, StaticDiscordEmoji.Question, $"Who wants to play Tic-Tac-Toe with {ctx.User.Username}?");
                DiscordUser opponent = await ctx.WaitForGameOpponentAsync();
                if (opponent == null)
                    return;

                if (movetime?.TotalSeconds < 2 || movetime?.TotalSeconds > 120)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");

                var game = new TicTacToeGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                this.Shared.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await game.RunAsync();

                    if (game.Winner != null) {
                        if (game.IsTimeoutReached)
                            await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"{game.Winner.Mention} won due to no replies from opponent!");
                        else
                            await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"The winner is: {game.Winner.Mention}!");

                        await this.Database.UpdateUserStatsAsync(game.Winner.Id, GameStatsType.TicTacToesWon);
                        if (game.Winner.Id == ctx.User.Id)
                            await this.Database.UpdateUserStatsAsync(opponent.Id, GameStatsType.TicTacToesWon);
                        else
                            await this.Database.UpdateUserStatsAsync(ctx.User.Id, GameStatsType.TicTacToesWon);
                    } else {
                        await InformAsync(ctx, StaticDiscordEmoji.Joystick, "A draw... Pathetic...");
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_TICTACTOE_RULES
            [Command("rules")]
            [Description("Explain the Tic-Tac-Toe game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!game tictactoe rules")]
            public Task RulesAsync(CommandContext ctx)
            {
                return InformAsync(ctx,
                    StaticDiscordEmoji.Information,
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
            [UsageExamples("!game tictactoe stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                string top = await this.Database.GetTopTTTPlayersStringAsync(ctx.Client);
                await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"Top players in Tic-Tac-Toe:\n\n{top}");
            }
            #endregion
        }
    }
}
