#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule : TheGodfatherBaseModule
    {
        [Group("othello"), Module(ModuleType.Games)]
        [Description("Starts an \"Othello\" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play. You can also specify a time window in which player must submit their move.")]
        [Aliases("reversi", "oth", "rev")]
        [UsageExamples("!game othello",
                       "!game othello 10s")]
        public class OthelloModule : TheGodfatherBaseModule
        {

            public OthelloModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? movetime = null)
            {
                if (ChannelEvent.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Question, $"Who wants to play Othello with {ctx.User.Username}?")
                    .ConfigureAwait(false);
                var opponent = await ctx.WaitForGameOpponentAsync()
                    .ConfigureAwait(false);
                if (opponent == null)
                    return;

                if (movetime?.TotalSeconds > 120 || movetime?.TotalSeconds < 2)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");

                var othello = new Othello(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                ChannelEvent.RegisterEventInChannel(othello, ctx.Channel.Id);
                try {
                    await othello.RunAsync()
                        .ConfigureAwait(false);
                    
                    if (othello.Winner != null) {
                        if (othello.TimedOut == false)
                            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Trophy, $"The winner is: {othello.Winner.Mention}!").ConfigureAwait(false);
                        else
                            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Trophy, $"{othello.Winner.Mention} won due to no replies from opponent!").ConfigureAwait(false);

                        await Database.UpdateUserStatsAsync(othello.Winner.Id, GameStatsType.OthellosWon)
                            .ConfigureAwait(false);
                        if (othello.Winner.Id == ctx.User.Id)
                            await Database.UpdateUserStatsAsync(opponent.Id, GameStatsType.OthellosLost).ConfigureAwait(false);
                        else
                            await Database.UpdateUserStatsAsync(ctx.User.Id, GameStatsType.OthellosLost).ConfigureAwait(false);
                    } else {
                        await ctx.RespondWithIconEmbedAsync("A draw... Pathetic...", ":video_game:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    ChannelEvent.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_OTHELLO_RULES
            [Command("rules"), Module(ModuleType.Games)]
            [Description("Explain the Othello game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!game othello rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.RespondWithIconEmbedAsync(
                    "Othello (or ``Reversi``) is a strategy board game for two players, played on an 8×8 uncheckered board. " +
                    "There are sixty-four identical game pieces called disks (often spelled \"discs\"), " +
                    "which are light on one side and dark on the other. Players take turns placing disks on the " +
                    "board with their assigned color facing up. During a play, any disks of the opponent's " +
                    "color that are in a straight line and bounded by the disk just placed and another disk " +
                    "of the current player's color are turned over to the current player's color. " +
                    "The object of the game is to have the majority of disks turned to display your color when " +
                    "the last playable empty square is filled.",
                    ":book:"
                ).ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_OTHELLO_STATS
            [Command("stats"), Module(ModuleType.Games)]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExamples("!game othello stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                var top = await Database.GetTopOthelloPlayersStringAsync(ctx.Client)
                    .ConfigureAwait(false);

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Trophy, $"Top players in Othello:\n\n{top}")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
