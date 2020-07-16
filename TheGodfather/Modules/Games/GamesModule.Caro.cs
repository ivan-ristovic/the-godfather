#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Services;
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
        
        public class CaroModule : TheGodfatherServiceModule<ChannelEventService>
        {

            public CaroModule(ChannelEventService service, DbContextBuilder db)
                : base(service, db)
            {
                
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? moveTime = null)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                await this.InformAsync(ctx, Emojis.Question, $"Who wants to play Caro against {ctx.User.Username}?");
                DiscordUser opponent = await ctx.WaitForGameOpponentAsync();
                if (opponent is null)
                    return;

                if (moveTime?.TotalSeconds < 2 || moveTime?.TotalSeconds > 120)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");
                    
                var caro = new CaroGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, moveTime);
                this.Service.RegisterEventInChannel(caro, ctx.Channel.Id);
                try {
                    await caro.RunAsync();

                    if (!(caro.Winner is null)) {
                        if (caro.IsTimeoutReached)
                            await this.InformAsync(ctx, Emojis.Trophy, $"{caro.Winner.Mention} won due to no replies from opponent!");
                        else
                            await this.InformAsync(ctx, Emojis.Trophy, $"The winner is: {caro.Winner.Mention}!");

                        await this.Database.UpdateStatsAsync(caro.Winner.Id, s => s.CaroWon++);
                        if (caro.Winner.Id == ctx.User.Id)
                            await this.Database.UpdateStatsAsync(opponent.Id, s => s.CaroLost++);
                        else
                            await this.Database.UpdateStatsAsync(ctx.User.Id, s => s.CaroLost++);
                    } else {
                        await this.InformAsync(ctx, Emojis.Joystick, "A draw... Pathetic...");
                    } 
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_CARO_RULES
            [Command("rules")]
            [Description("Explain the Caro game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
            {
                return this.InformAsync(ctx,
                    Emojis.Information,
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
            public async Task StatsAsync(CommandContext ctx)
            {
                IReadOnlyList<GameStats> topStats = await this.Database.GetTopCaroStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildCaroStatsString());
                await this.InformAsync(ctx, Emojis.Trophy, $"Top players in Caro:\n\n{top}");
            }
            #endregion
        }
    }
}
