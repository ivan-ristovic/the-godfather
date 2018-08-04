#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services.Common;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Stats;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("caro")]
        [Description("Starts a \"Caro\" game. Play a move by writing a pair of numbers from 1 to 10 corresponding " +
                     "to the row and column where you wish to play. You can also specify a time window in which " +
                     "players must submit their move.")]
        [Aliases("c", "gomoku", "gobang")]
        [UsageExamples("!game caro",
                       "!game caro 10s")]
        public class CaroModule : TheGodfatherModule
        {

            public CaroModule(SharedData shared, DBService db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Teal;
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? moveTime = null)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                await InformAsync(ctx, StaticDiscordEmoji.Question, $"Who wants to play Caro against {ctx.User.Username}?");
                DiscordUser opponent = await ctx.WaitForGameOpponentAsync();
                if (opponent == null)
                    return;

                if (moveTime?.TotalSeconds < 2 || moveTime?.TotalSeconds > 120)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");
                    
                var caro = new CaroGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, moveTime);
                this.Shared.RegisterEventInChannel(caro, ctx.Channel.Id);
                try {
                    await caro.RunAsync();

                    if (caro.Winner != null) {
                        if (caro.IsTimeoutReached)
                            await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"{caro.Winner.Mention} won due to no replies from opponent!");
                        else
                            await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"The winner is: {caro.Winner.Mention}!");

                        await this.Database.UpdateUserStatsAsync(caro.Winner.Id, GameStatsType.CarosWon);
                        if (caro.Winner.Id == ctx.User.Id)
                            await this.Database.UpdateUserStatsAsync(opponent.Id, GameStatsType.CarosLost);
                        else
                            await this.Database.UpdateUserStatsAsync(ctx.User.Id, GameStatsType.CarosLost);
                    } else {
                        await InformAsync(ctx, StaticDiscordEmoji.Joystick, "A draw... Pathetic...");
                    } 
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_CARO_RULES
            [Command("rules")]
            [Description("Explain the Caro game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!game caro rules")]
            public Task RulesAsync(CommandContext ctx)
            {
                return InformAsync(ctx,
                    StaticDiscordEmoji.Information,
                    "Caro (aka ``Gomoku`` or ``Gobang``) is similar to a Tic-Tac-Toe game played on a 10x10 board." +
                    "The goal is to have an unbroken row of 5 symbols in order to win the game. Players play in " +
                    "turns, placing their symbols on the board. The game ends when someone makes 5 symbols " +
                    "in a row or when there are no more empty fields on the board."
                );
            }
            #endregion

            #region COMMAND_CARO_STATS
            [Command("stats")]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExamples("!game caro stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                string top = await this.Database.GetTopCaroPlayersStringAsync(ctx.Client);
                await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"Top players in Caro:\n\n{top}");
            }
            #endregion
        }
    }
}
