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
        [Group("othello")]
        [Description("Starts an \"Othello\" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play.")]
        [Aliases("reversi", "oth", "rev")]
        [UsageExample("!game othello")]
        public class OthelloModule : TheGodfatherBaseModule
        {

            public OthelloModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                await ReplyWithEmbedAsync(ctx, $"Who wants to play Othello with {ctx.User.Username}?", ":question:")
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
                        await ReplyWithEmbedAsync(ctx, $"The winner is: {othello.Winner.Mention}!", ":trophy:")
                            .ConfigureAwait(false);
                        
                        await Database.UpdateUserStatsAsync(othello.Winner.Id, "othello_won")
                            .ConfigureAwait(false);
                        if (othello.Winner.Id == ctx.User.Id)
                            await Database.UpdateUserStatsAsync(opponent.Id, "othello_lost").ConfigureAwait(false);
                        else
                            await Database.UpdateUserStatsAsync(ctx.User.Id, "othello_lost").ConfigureAwait(false);
                    } else if (othello.NoReply == false) {
                        await ReplyWithEmbedAsync(ctx, "A draw... Pathetic...", ":video_game:")
                            .ConfigureAwait(false);
                    } else {
                        await ReplyWithEmbedAsync(ctx, "No reply, aborting Othello game...", ":alarm_clock:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }
        }
    }
}
