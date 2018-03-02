#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule : TheGodfatherBaseModule
    {
        [Group("tictactoe")]
        [Description("Starts a \"Tic-Tac-Toe\" game. Play a move by writing a number from 1 to 9 corresponding to the field where you wish to play. You can also specify a time window in which player must submit their move.")]
        [Aliases("ttt")]
        [UsageExample("!game tictactoe")]
        [UsageExample("!game tictactoe 10s")]
        public class TicTacToeModule : TheGodfatherBaseModule
        {

            public TicTacToeModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? movetime = null)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                await ReplyWithEmbedAsync(ctx, $"Who wants to play Tic-Tac-Toe with {ctx.User.Username}?", ":question:")
                    .ConfigureAwait(false);
                var opponent = await InteractivityUtil.WaitForGameOpponentAsync(ctx)
                    .ConfigureAwait(false);
                if (opponent == null)
                    return;

                if (movetime?.TotalSeconds > 120 || movetime?.TotalSeconds < 2)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");

                var ttt = new TicTacToe(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                Game.RegisterGameInChannel(ttt, ctx.Channel.Id);
                try {
                    await ttt.RunAsync()
                        .ConfigureAwait(false);

                    if (ttt.Winner != null) {
                        if (ttt.NoReply == false)
                            await ReplyWithEmbedAsync(ctx, $"The winner is: {ttt.Winner.Mention}!", ":trophy:").ConfigureAwait(false);
                        else
                            await ReplyWithEmbedAsync(ctx, $"{ttt.Winner.Mention} won due to no replies from opponent!", ":trophy:").ConfigureAwait(false);

                        await Database.UpdateUserStatsAsync(ttt.Winner.Id, "ttt_won")
                            .ConfigureAwait(false);
                        if (ttt.Winner.Id == ctx.User.Id)
                            await Database.UpdateUserStatsAsync(opponent.Id, "ttt_lost").ConfigureAwait(false);
                        else
                            await Database.UpdateUserStatsAsync(ctx.User.Id, "ttt_lost").ConfigureAwait(false);
                    } else {
                        await ReplyWithEmbedAsync(ctx, "A draw... Pathetic...", ":video_game:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_TICTACTOE_RULES
            [Command("rules")]
            [Description("Explain the Tic-Tac-Toe game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExample("!game tictactoe rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ReplyWithEmbedAsync(
                    ctx,
                    "The object of Tic Tac Toe is to get three in a row. " +
                    "You play on a three by three game board. The first player is known as X and the second is O. " +
                    "Players alternate placing Xs and Os on the game board until either oppent has three in a row " +
                    "or all nine squares are filled.",
                    ":book:"
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
