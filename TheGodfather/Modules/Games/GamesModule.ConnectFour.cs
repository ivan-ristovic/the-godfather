#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule : GodfatherBaseModule
    {
        [Group("connectfour")]
        [Description("Starts a \"Connect 4\" game. Play a move by writing a number from 1 to 9 corresponding to the column where you wish to insert your piece.")]
        [Aliases("connect4", "chain4", "chainfour", "c4")]
        public class ConnectFourModule : GodfatherBaseModule
        {

            public ConnectFourModule(SharedData shared, DatabaseService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                await ctx.RespondAsync($"Who wants to play Connect4 with {ctx.User.Username}?")
                    .ConfigureAwait(false);
                var opponent = await InteractivityUtil.WaitForGameOpponentAsync(ctx)
                    .ConfigureAwait(false);
                if (opponent == null)
                    return;

                var connect4 = new Connect4(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent);
                Game.RegisterGameInChannel(connect4, ctx.Channel.Id);
                try {
                    await connect4.RunAsync()
                        .ConfigureAwait(false);

                    if (connect4.Winner != null) {
                        await ctx.RespondAsync($"The winner is: {connect4.Winner.Mention}!")
                            .ConfigureAwait(false);

                        await DatabaseService.UpdateUserStatsAsync(connect4.Winner.Id, "chain4_won").ConfigureAwait(false);
                        if (connect4.Winner.Id == ctx.User.Id)
                            await DatabaseService.UpdateUserStatsAsync(opponent.Id, "chain4_lost").ConfigureAwait(false);
                        else
                            await DatabaseService.UpdateUserStatsAsync(ctx.User.Id, "chain4_lost").ConfigureAwait(false);
                    } else if (connect4.NoReply == false) {
                        await ctx.RespondAsync("A draw... Pathetic...")
                            .ConfigureAwait(false);
                    } else {
                        await ctx.RespondAsync("No reply, aborting Connect4 game...")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }
        }
    }
}
