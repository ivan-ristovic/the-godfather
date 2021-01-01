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
        [Group("connect4")]
        [Description("Starts a \"Connect 4\" game. Play a move by writing a number from 1 to 9 corresponding to " +
                     "the column where you wish to insert your piece. You can also specify a time window in " +
                     "which player must submit their move.")]
        [Aliases("connectfour", "chain4", "chainfour", "c4", "fourinarow", "fourinaline", "4row", "4line", "cfour")]

        public class ConnectFourModule : TheGodfatherServiceModule<ChannelEventService>
        {

            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? movetime = null)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                await this.InformAsync(ctx, Emojis.Question, $"Who wants to play Connect4 against {ctx.User.Username}?");
                DiscordUser opponent = await ctx.WaitForGameOpponentAsync();
                if (opponent is null)
                    return;

                if (movetime?.TotalSeconds is < 2 or > 120)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");

                var connect4 = new ConnectFourGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                this.Service.RegisterEventInChannel(connect4, ctx.Channel.Id);
                try {
                    await connect4.RunAsync(ctx.Services.GetRequiredService<LocalizationService>());

                    if (!(connect4.Winner is null)) {
                        if (connect4.IsTimeoutReached)
                            await this.InformAsync(ctx, Emojis.Trophy, $"{connect4.Winner.Mention} won due to no replies from opponent!").ConfigureAwait(false);
                        else
                            await this.InformAsync(ctx, Emojis.Trophy, $"The winner is: {connect4.Winner.Mention}!").ConfigureAwait(false);

                        await this.Database.UpdateStatsAsync(connect4.Winner.Id, s => s.Chain4Won++);
                        if (connect4.Winner.Id == ctx.User.Id)
                            await this.Database.UpdateStatsAsync(opponent.Id, s => s.Chain4Lost++);
                        else
                            await this.Database.UpdateStatsAsync(ctx.User.Id, s => s.Chain4Lost++);
                    } else {
                        await this.InformAsync(ctx, Emojis.Joystick, "A draw... Pathetic...");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_CONNECT4_RULES
            [Command("rules")]
            [Description("Explain the Connect4 game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
            {
                return this.InformAsync(ctx,
                    Emojis.Information,
                    "\nConnect Four (also known as ``Four in a Row``, ``Four in a Line``) is a two-player game " +
                    "in which the players first choose a color and then take turns dropping colored discs from the " +
                    "top into a seven-column, six-row vertically suspended grid. The pieces fall straight down, " +
                    "occupying the next available space within the column. The objective of the game is to be the " +
                    "first to form a horizontal, vertical, or diagonal line of four of one's own discs."
                );
            }
            #endregion

            #region COMMAND_CONNECT4_STATS
            [Command("stats")]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            public async Task StatsAsync(CommandContext ctx)
            {
                IReadOnlyList<GameStats> topStats = await this.Database.GetTopChain4StatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildChain4StatsString());
                await this.InformAsync(ctx, Emojis.Trophy, $"Top players in Connect4:\n\n{top}");
            }
            #endregion
        }
    }
}
