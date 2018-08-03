#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services.Common;
using TheGodfather.Services.Database.Stats;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule : TheGodfatherModule
    {
        [Group("caro"), Module(ModuleType.Games)]
        [Description("Starts a \"Caro\" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play. You can also specify a time window in which player must submit their move.")]
        [Aliases("c", "gomoku", "gobang")]
        [UsageExamples("!game caro",
                       "!game caro 10s")]
        public class CaroModule : TheGodfatherModule
        {

            public CaroModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? movetime = null)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                await InformAsync(ctx, StaticDiscordEmoji.Question, $"Who wants to play Caro with {ctx.User.Username}?")
                    .ConfigureAwait(false);
                var opponent = await ctx.WaitForGameOpponentAsync()
                    .ConfigureAwait(false);
                if (opponent == null)
                    return;

                if (movetime?.TotalSeconds > 120 || movetime?.TotalSeconds < 2)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");
                    
                var caro = new Caro(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                this.Shared.RegisterEventInChannel(caro, ctx.Channel.Id);
                try {
                    await caro.RunAsync()
                        .ConfigureAwait(false);

                    if (caro.Winner != null) {
                        if (caro.IsTimeoutReached == false)
                            await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"The winner is: {caro.Winner.Mention}!").ConfigureAwait(false);
                        else
                            await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"{caro.Winner.Mention} won due to no replies from opponent!").ConfigureAwait(false);

                        await Database.UpdateUserStatsAsync(caro.Winner.Id, GameStatsType.CarosWon)
                            .ConfigureAwait(false);
                        if (caro.Winner.Id == ctx.User.Id)
                            await Database.UpdateUserStatsAsync(opponent.Id, GameStatsType.CarosLost).ConfigureAwait(false);
                        else
                            await Database.UpdateUserStatsAsync(ctx.User.Id, GameStatsType.CarosLost).ConfigureAwait(false);
                    } else {
                        await InformAsync(ctx, "A draw... Pathetic...", ":video_game:")
                            .ConfigureAwait(false);
                    } 
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_CARO_RULES
            [Command("rules"), Module(ModuleType.Games)]
            [Description("Explain the Caro game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!game caro rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await InformAsync(ctx, 
                    "\nCaro (aka ``Gomoku`` or ``Gobang``) is basically a Tic-Tac-Toe game played on a 10x10 board." +
                    "The goal is to have an unbroken row of 5 symbols in order to win the game." +
                    "Players play in turns, placing their symbols on the board. The game ends when someone makes 5 symbols " +
                    "in a row or when there are no more empty fields on the board.",
                    ":book:"
                ).ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_CARO_STATS
            [Command("stats"), Module(ModuleType.Games)]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExamples("!game caro stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                var top = await Database.GetTopCaroPlayersStringAsync(ctx.Client)
                    .ConfigureAwait(false);

                await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"Top players in Caro:\n\n{top}")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
