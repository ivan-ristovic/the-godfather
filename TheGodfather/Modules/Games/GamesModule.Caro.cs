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
    public partial class GamesModule : GodfatherBaseModule
    {
        [Group("caro")]
        [Description("Starts a \"Caro\" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play.")]
        [Aliases("c")]
        [UsageExample("!game caro")]
        public class CaroModule : GodfatherBaseModule
        {

            public CaroModule(SharedData shared, DatabaseService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                await ReplyWithEmbedAsync(ctx, $"Who wants to play Caro with {ctx.User.Username}?", ":question:")
                    .ConfigureAwait(false);
                var opponent = await InteractivityUtil.WaitForGameOpponentAsync(ctx)
                    .ConfigureAwait(false);
                if (opponent == null)
                    return;

                var caro = new Caro(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent);
                Game.RegisterGameInChannel(caro, ctx.Channel.Id);
                try {
                    await caro.RunAsync()
                        .ConfigureAwait(false);

                    if (caro.Winner != null) {
                        await ReplyWithEmbedAsync(ctx, $"The winner is: {caro.Winner.Mention}!", ":trophy:")
                            .ConfigureAwait(false);

                        await DatabaseService.UpdateUserStatsAsync(caro.Winner.Id, "caro_won")
                            .ConfigureAwait(false);
                        if (caro.Winner.Id == ctx.User.Id)
                            await DatabaseService.UpdateUserStatsAsync(opponent.Id, "caro_lost").ConfigureAwait(false);
                        else
                            await DatabaseService.UpdateUserStatsAsync(ctx.User.Id, "caro_lost").ConfigureAwait(false);
                    } else if (caro.NoReply == false) {
                        await ReplyWithEmbedAsync(ctx, "A draw... Pathetic...", ":video_game:")
                            .ConfigureAwait(false);
                    } else {
                        await ReplyWithEmbedAsync(ctx, "No reply, aborting Caro game...", ":alarm_clock:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }
        }
    }
}
