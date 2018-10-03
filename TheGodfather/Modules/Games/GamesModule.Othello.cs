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
        [Group("othello")]
        [Description("Starts an \"Othello\" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play. You can also specify a time window in which player must submit their move.")]
        [Aliases("reversi", "oth", "rev")]
        [UsageExamples("!game othello",
                       "!game othello 10s")]
        public class OthelloModule : TheGodfatherModule
        {

            public OthelloModule(SharedData shared, DBService db)
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

                await this.InformAsync(ctx, StaticDiscordEmoji.Question, $"Who wants to play Othello against {ctx.User.Username}?");
                DiscordUser opponent = await ctx.WaitForGameOpponentAsync();
                if (opponent is null)
                    return;

                if (movetime?.TotalSeconds < 2 || movetime?.TotalSeconds > 120)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");

                var game = new OthelloGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                this.Shared.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await game.RunAsync();
                    
                    if (!(game.Winner is null)) {
                        if (game.IsTimeoutReached)
                            await this.InformAsync(ctx, StaticDiscordEmoji.Trophy, $"{game.Winner.Mention} won due to no replies from opponent!");
                        else
                            await this.InformAsync(ctx, StaticDiscordEmoji.Trophy, $"The winner is: {game.Winner.Mention}!");

                        await this.Database.UpdateUserStatsAsync(game.Winner.Id, GameStatsType.OthellosWon);
                        if (game.Winner.Id == ctx.User.Id)
                            await this.Database.UpdateUserStatsAsync(opponent.Id, GameStatsType.OthellosLost);
                        else
                            await this.Database.UpdateUserStatsAsync(ctx.User.Id, GameStatsType.OthellosLost);
                    } else {
                        await this.InformAsync(ctx, StaticDiscordEmoji.Joystick, "A draw... Pathetic...");
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_OTHELLO_RULES
            [Command("rules")]
            [Description("Explain the Othello game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!game othello rules")]
            public Task RulesAsync(CommandContext ctx)
            {
                return this.InformAsync(ctx, 
                    StaticDiscordEmoji.Information,
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
            [UsageExamples("!game othello stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                string top = await this.Database.GetTopOthelloPlayersStringAsync(ctx.Client);
                await this.InformAsync(ctx, StaticDiscordEmoji.Trophy, $"Top players in Othello:\n\n{top}");
            }
            #endregion
        }
    }
}
