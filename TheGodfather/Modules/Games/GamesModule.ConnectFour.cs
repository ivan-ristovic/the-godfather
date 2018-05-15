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
        [Group("connect4"), Module(ModuleType.Games)]
        [Description("Starts a \"Connect 4\" game. Play a move by writing a number from 1 to 9 corresponding to the column where you wish to insert your piece. You can also specify a time window in which player must submit their move.")]
        [Aliases("connectfour", "chain4", "chainfour", "c4", "fourinarow", "fourinaline", "4row", "4line", "cfour")]
        [UsageExample("!game connect4")]
        [UsageExample("!game connect4 10s")]
        public class ConnectFourModule : TheGodfatherBaseModule
        {

            public ConnectFourModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? movetime = null)
            {
                if (ChannelEvent.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Question, $"Who wants to play Connect4 with {ctx.User.Username}?")
                    .ConfigureAwait(false);
                var opponent = await ctx.WaitForGameOpponentAsync()
                    .ConfigureAwait(false);
                if (opponent == null)
                    return;

                if (movetime?.TotalSeconds > 120 || movetime?.TotalSeconds < 2)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");

                var connect4 = new Connect4(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                ChannelEvent.RegisterEventInChannel(connect4, ctx.Channel.Id);
                try {
                    await connect4.RunAsync()
                        .ConfigureAwait(false);

                    if (connect4.Winner != null) {
                        if (connect4.TimedOut == false)
                            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Trophy, $"The winner is: {connect4.Winner.Mention}!").ConfigureAwait(false);
                        else
                            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Trophy, $"{connect4.Winner.Mention} won due to no replies from opponent!").ConfigureAwait(false);
                        
                        await Database.UpdateUserStatsAsync(connect4.Winner.Id, GameStatsType.Connect4sWon)
                            .ConfigureAwait(false);
                        if (connect4.Winner.Id == ctx.User.Id)
                            await Database.UpdateUserStatsAsync(opponent.Id, GameStatsType.Connect4sLost).ConfigureAwait(false);
                        else
                            await Database.UpdateUserStatsAsync(ctx.User.Id, GameStatsType.Connect4sLost).ConfigureAwait(false);
                    } else {
                        await ctx.RespondWithIconEmbedAsync("A draw... Pathetic...", ":video_game:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    ChannelEvent.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_CONNECT4_RULES
            [Command("rules"), Module(ModuleType.Games)]
            [Description("Explain the Connect4 game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExample("!game connect4 rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.RespondWithIconEmbedAsync(
                    "\nConnect Four (also known as ``Four in a Row``, ``Four in a Line``) is a two-player connection game " +
                    "in which the players first choose a color and then take turns dropping colored discs from the top into a seven-column, " +
                    "six-row vertically suspended grid. The pieces fall straight down, occupying the next available space within the column. " +
                    "The objective of the game is to be the first to form a horizontal, vertical, or diagonal line of four of one's own discs.",
                    ":book:"
                ).ConfigureAwait(false);
            }


            #endregion

            #region COMMAND_CONNECT4_STATS
            [Command("stats"), Module(ModuleType.Games)]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExample("!game connect4 stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                var top = await Database.GetTopChain4PlayersStringAsync(ctx.Client)
                    .ConfigureAwait(false);

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Trophy, $"Top players in Connect Four:\n\n{top}")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
