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
        [Group("othello")]
        [Description("Starts an \"Othello\" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play.")]
        [Aliases("reversi", "oth", "rev")]
        [UsageExample("!game othello")]
        public class OthelloModule : GodfatherBaseModule
        {

            public OthelloModule(SharedData shared, DatabaseService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                await ctx.RespondAsync($"Who wants to play Othello with {ctx.User.Username}?")
                    .ConfigureAwait(false);
                var opponent = await InteractivityUtil.WaitForGameOpponentAsync(ctx)
                    .ConfigureAwait(false);
                if (opponent == null)
                    return;

                var othello = new Othello(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent);
                Game.RegisterGameInChannel(othello, ctx.Channel.Id);
                try {
                    await othello.RunAsync()
                        .ConfigureAwait(false);

                    if (othello.Winner != null) {
                        await ctx.RespondAsync($"The winner is: {othello.Winner.Mention}!")
                            .ConfigureAwait(false);
                        
                        await DatabaseService.UpdateUserStatsAsync(othello.Winner.Id, "othello_won")
                            .ConfigureAwait(false);
                        if (othello.Winner.Id == ctx.User.Id)
                            await DatabaseService.UpdateUserStatsAsync(opponent.Id, "othello_lost").ConfigureAwait(false);
                        else
                            await DatabaseService.UpdateUserStatsAsync(ctx.User.Id, "othello_lost").ConfigureAwait(false);
                    } else if (othello.NoReply == false) {
                        await ctx.RespondAsync("A draw... Pathetic...")
                            .ConfigureAwait(false);
                    } else {
                        await ctx.RespondAsync("No reply, aborting Othello game...")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }
        }
    }
}
