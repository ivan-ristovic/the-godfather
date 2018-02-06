#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule : GodfatherBaseModule
    {
        [Group("caro")]
        [Description("Caro game commands.")]
        [Aliases("c")]
        public class CaroModule : GodfatherBaseModule
        {

            public CaroModule(SharedData shared, DatabaseService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (Game.RunningInChannel(ctx.Channel.Id, SharedData.ActiveGames))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                await ctx.RespondAsync($"Who wants to play caro with {ctx.User.Username}?")
                    .ConfigureAwait(false);
                var opponent = await InteractivityUtil.WaitForGameOpponentAsync(ctx)
                    .ConfigureAwait(false);
                if (opponent == null)
                    return;

                var caro = new Caro(ctx.Client, ctx.Channel, ctx.User, opponent);
                Game.RegisterGameInChannel(caro, ctx.Channel.Id, SharedData.ActiveGames);
                await caro.PlayAsync()
                    .ConfigureAwait(false);

                if (caro.Winner != null) {
                    await ctx.RespondAsync($"The winner is: {caro.Winner.Mention}!")
                        .ConfigureAwait(false);

                    await DatabaseService.UpdateUserStatsAsync(caro.Winner.Id, "caro_won")
                        .ConfigureAwait(false);
                    if (caro.Winner.Id == ctx.User.Id)
                        await DatabaseService.UpdateUserStatsAsync(opponent.Id, "caro_lost").ConfigureAwait(false);
                    else
                        await DatabaseService.UpdateUserStatsAsync(ctx.User.Id, "caro_lost").ConfigureAwait(false);
                } else if (caro.NoReply == false) {
                    await ctx.RespondAsync("A draw... Pathetic...")
                        .ConfigureAwait(false);
                } else {
                    await ctx.RespondAsync("No reply, aborting...")
                        .ConfigureAwait(false);
                }

                Game.UnregisterGameInChannel(ctx.Channel.Id, SharedData.ActiveGames);
            }
        }
    }
}
