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
        [Group("caro")]
        [Description("Starts a \"Caro\" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play. You can also specify a time window in which player must submit their move.")]
        [Aliases("c", "gomoku", "gobang")]
        [UsageExample("!game caro")]
        [UsageExample("!game caro 10s")]
        public class CaroModule : TheGodfatherBaseModule
        {

            public CaroModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? movetime = null)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                await ReplyWithEmbedAsync(ctx, $"Who wants to play Caro with {ctx.User.Username}?", ":question:")
                    .ConfigureAwait(false);
                var opponent = await InteractivityUtil.WaitForGameOpponentAsync(ctx)
                    .ConfigureAwait(false);
                if (opponent == null)
                    return;

                if (movetime?.TotalSeconds > 120 || movetime?.TotalSeconds < 2)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");
                    
                var caro = new Caro(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                Game.RegisterGameInChannel(caro, ctx.Channel.Id);
                try {
                    await caro.RunAsync()
                        .ConfigureAwait(false);

                    if (caro.Winner != null) {
                        if (caro.NoReply == false)
                            await ReplyWithEmbedAsync(ctx, $"The winner is: {caro.Winner.Mention}!", ":trophy:").ConfigureAwait(false);
                        else
                            await ReplyWithEmbedAsync(ctx, $"{caro.Winner.Mention} won due to no replies from opponent!", ":trophy:").ConfigureAwait(false);

                        await Database.UpdateUserStatsAsync(caro.Winner.Id, "caro_won")
                            .ConfigureAwait(false);

                        if (caro.Winner.Id == ctx.User.Id)
                            await Database.UpdateUserStatsAsync(opponent.Id, "caro_lost").ConfigureAwait(false);
                        else
                            await Database.UpdateUserStatsAsync(ctx.User.Id, "caro_lost").ConfigureAwait(false);
                    } else {
                        await ReplyWithEmbedAsync(ctx, "A draw... Pathetic...", ":video_game:")
                            .ConfigureAwait(false);
                    } 
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_CARO_RULES
            [Command("rules")]
            [Description("Explain the caro game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExample("!game caro rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ReplyWithEmbedAsync(
                    ctx,
                    "\nCaro (aka ``Gomoku`` or ``Gobang``) is basically a Tic-Tac-Toe game played on a 10x10 board." +
                    "The goal is to have an unbroken row of 5 symbols in order to win the game." +
                    "Players play in turns, placing their symbols on the board. The game ends when someone makes 5 symbols " +
                    "in a row or when there are no more empty fields on the board.",
                    ":book:"
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
