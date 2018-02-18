#region USING_DIRECTIVES
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
        [Group("connect4")]
        [Description("Starts a \"Connect 4\" game. Play a move by writing a number from 1 to 9 corresponding to the column where you wish to insert your piece.")]
        [Aliases("connectfour", "chain4", "chainfour", "c4")]
        [UsageExample("!game connect4")]
        public class ConnectFourModule : TheGodfatherBaseModule
        {

            public ConnectFourModule(SharedData shared, DatabaseService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                await ReplyWithEmbedAsync(ctx, $"Who wants to play Connect4 with {ctx.User.Username}?", ":question:")
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
                        await ReplyWithEmbedAsync(ctx, $"The winner is: {connect4.Winner.Mention}!", ":trophy:")
                            .ConfigureAwait(false);

                        await DatabaseService.UpdateUserStatsAsync(connect4.Winner.Id, "chain4_won").ConfigureAwait(false);
                        if (connect4.Winner.Id == ctx.User.Id)
                            await DatabaseService.UpdateUserStatsAsync(opponent.Id, "chain4_lost").ConfigureAwait(false);
                        else
                            await DatabaseService.UpdateUserStatsAsync(ctx.User.Id, "chain4_lost").ConfigureAwait(false);
                    } else if (connect4.NoReply == false) {
                        await ReplyWithEmbedAsync(ctx, "A draw... Pathetic...", ":video_game:")
                            .ConfigureAwait(false);
                    } else {
                        await ReplyWithEmbedAsync(ctx, "No reply, aborting Connect4 game...", ":alarm_clock:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }
        }
    }
}
