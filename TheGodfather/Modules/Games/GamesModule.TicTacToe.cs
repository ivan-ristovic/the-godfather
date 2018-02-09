#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Attributes;

using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule : GodfatherBaseModule
    {
        [Group("tictactoe")]
        [Description("Starts a \"Tic-Tac-Toe\" game. Play a move by writing a number from 1 to 9 corresponding to the field where you wish to play.")]
        [Aliases("ttt")]
        [UsageExample("!game tictactoe")]
        public class TicTacToeModule : GodfatherBaseModule
        {

            public TicTacToeModule(SharedData shared, DatabaseService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                await ctx.RespondAsync($"Who wants to play tic-tac-toe with {ctx.User.Username}?")
                    .ConfigureAwait(false);
                var opponent = await InteractivityUtil.WaitForGameOpponentAsync(ctx)
                    .ConfigureAwait(false);
                if (opponent == null)
                    return;

                var ttt = new TicTacToe(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent);
                Game.RegisterGameInChannel(ttt, ctx.Channel.Id);
                try {
                    await ttt.RunAsync()
                        .ConfigureAwait(false);

                    if (ttt.Winner != null) {
                        await ctx.RespondAsync($"The winner is: {ttt.Winner.Mention}!")
                            .ConfigureAwait(false);

                        await DatabaseService.UpdateUserStatsAsync(ttt.Winner.Id, "ttt_won")
                            .ConfigureAwait(false);
                        if (ttt.Winner.Id == ctx.User.Id)
                            await DatabaseService.UpdateUserStatsAsync(opponent.Id, "ttt_lost").ConfigureAwait(false);
                        else
                            await DatabaseService.UpdateUserStatsAsync(ctx.User.Id, "ttt_lost").ConfigureAwait(false);
                    } else if (ttt.NoReply == false) {
                        await ctx.RespondAsync("A draw... Pathetic...")
                            .ConfigureAwait(false);
                    } else {
                        await ctx.RespondAsync("No reply, aborting TicTacToe game...")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }
        }
    }
}
