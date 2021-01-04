#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
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
        [Group("othello")]
        [Description("Starts an \"Othello\" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play. You can also specify a time window in which player must submit their move.")]
        [Aliases("reversi", "oth", "rev")]

        [RequireGuild]
        public class OthelloModule : TheGodfatherServiceModule<ChannelEventService>
        {

            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? movetime = null)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                await this.InformAsync(ctx, Emojis.Question, $"Who wants to play Othello against {ctx.User.Username}?");
                DiscordUser opponent = await ctx.WaitForGameOpponentAsync();
                if (opponent is null)
                    return;

                if (movetime?.TotalSeconds is < 2 or > 120)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");

                var othello = new OthelloGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                this.Service.RegisterEventInChannel(othello, ctx.Channel.Id);
                try {
                    await othello.RunAsync(ctx.Services.GetRequiredService<LocalizationService>());

                    if (!(othello.Winner is null)) {
                        if (othello.IsTimeoutReached)
                            await this.InformAsync(ctx, Emojis.Trophy, $"{othello.Winner.Mention} won due to no replies from opponent!");
                        else
                            await this.InformAsync(ctx, Emojis.Trophy, $"The winner is: {othello.Winner.Mention}!");

                        await this.Database.UpdateStatsAsync(othello.Winner.Id, s => s.OthelloWon++);
                        if (othello.Winner.Id == ctx.User.Id)
                            await this.Database.UpdateStatsAsync(opponent.Id, s => s.OthelloLost++);
                        else
                            await this.Database.UpdateStatsAsync(ctx.User.Id, s => s.OthelloLost++);
                    } else {
                        await this.InformAsync(ctx, Emojis.Joystick, "A draw... Pathetic...");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_OTHELLO_RULES
            [Command("rules")]
            [Description("Explain the Othello game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
            {
                return this.InformAsync(ctx,
                    Emojis.Information,
                    "Othello (or ``Reversi``) is a strategy board game for two players, played on an 8×8 " +
                    "uncheckered board. There are sixty-four identical game pieces called disks (often spelled " +
                    "\"discs\"), which are light on one side and dark on the other. Players take turns placing " +
                    "disks on the board with their assigned color facing up. During a play, any disks of the " +
                    "opponent's color that are in a straight line and bounded by the disk just placed and another " +
                    "disk of the current player's color are turned over to the current player's color. The " +
                    "objective of the game is to have the majority of disks turned to display your color when " +
                    "the last playable empty square is filled."
                );
            }
            #endregion

            #region COMMAND_OTHELLO_STATS
            [Command("stats")]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            public async Task StatsAsync(CommandContext ctx)
            {
                IReadOnlyList<GameStats> topStats = await this.Database.GetTopOthelloStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildOthelloStatsString());
                await this.InformAsync(ctx, Emojis.Trophy, $"Top players in Othello:\n\n{top}");
            }
            #endregion
        }
    }
}
