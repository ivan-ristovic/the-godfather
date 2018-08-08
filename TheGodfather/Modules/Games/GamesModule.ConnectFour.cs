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
        [UsageExamples("!game connect4",
                       "!game connect4 10s")]
        public class ConnectFourModule : TheGodfatherModule
        {

            public ConnectFourModule(SharedData shared, DBService db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Teal;
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? movetime = null)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                await InformAsync(ctx, StaticDiscordEmoji.Question, $"Who wants to play Connect4 against {ctx.User.Username}?");
                DiscordUser opponent = await ctx.WaitForGameOpponentAsync();
                if (opponent == null)
                    return;

                if (movetime?.TotalSeconds < 2 || movetime?.TotalSeconds > 120)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");

                var connect4 = new ConnectFourGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                this.Shared.RegisterEventInChannel(connect4, ctx.Channel.Id);
                try {
                    await connect4.RunAsync();

                    if (connect4.Winner != null) {
                        if (connect4.IsTimeoutReached)
                            await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"{connect4.Winner.Mention} won due to no replies from opponent!").ConfigureAwait(false);
                        else
                            await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"The winner is: {connect4.Winner.Mention}!").ConfigureAwait(false);

                        await this.Database.UpdateUserStatsAsync(connect4.Winner.Id, GameStatsType.Connect4sWon);
                        if (connect4.Winner.Id == ctx.User.Id)
                            await this.Database.UpdateUserStatsAsync(opponent.Id, GameStatsType.Connect4sLost);
                        else
                            await this.Database.UpdateUserStatsAsync(ctx.User.Id, GameStatsType.Connect4sLost);
                    } else {
                        await InformAsync(ctx, StaticDiscordEmoji.Joystick, "A draw... Pathetic...");
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_CONNECT4_RULES
            [Command("rules")]
            [Description("Explain the Connect4 game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!game connect4 rules")]
            public Task RulesAsync(CommandContext ctx)
            {
                return InformAsync(ctx,
                    StaticDiscordEmoji.Information,
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
            [UsageExamples("!game connect4 stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                string top = await this.Database.GetTopChain4PlayersStringAsync(ctx.Client);
                await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"Top players in Connect Four:\n\n{top}");
            }
            #endregion
        }
    }
}
