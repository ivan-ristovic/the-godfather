#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule : TheGodfatherBaseModule
    {
        [Group("duel")]
        [Description("Starts a duel which I will commentate.")]
        [Aliases("fight", "vs", "d")]
        [UsageExample("!game duel @Someone")]
        public class DuelModule : TheGodfatherBaseModule
        {

            public DuelModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Who to fight with?")] DiscordUser opponent)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                if (opponent.Id == ctx.User.Id)
                    throw new CommandFailedException("You can't duel yourself...");

                if (opponent.Id == ctx.Client.CurrentUser.Id) {
                    await ctx.RespondAsync(
                        $"{ctx.User.Mention} {string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":black_large_square:"), 5))} :crossed_swords: " +
                        $"{string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":white_large_square:"), 5))} {opponent.Mention}" +
                        $"\n{ctx.Client.CurrentUser.Mention} {DiscordEmoji.FromName(ctx.Client, ":zap:")} {ctx.User.Mention}"
                    ).ConfigureAwait(false);
                    await ctx.RespondAsync($"{ctx.Client.CurrentUser.Mention} wins!")
                        .ConfigureAwait(false);
                    return;
                }

                var duel = new Duel(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent);
                Game.RegisterGameInChannel(duel, ctx.Channel.Id);

                try {
                    await duel.RunAsync()
                        .ConfigureAwait(false);

                    await ctx.RespondAsync($"{duel.Winner.Username} {duel.FinishingMove ?? "wins"}!")
                        .ConfigureAwait(false);

                    await Database.UpdateUserStatsAsync(duel.Winner.Id, "duels_won")
                        .ConfigureAwait(false);
                    await Database.UpdateUserStatsAsync(duel.Winner.Id == ctx.User.Id ? opponent.Id : ctx.User.Id, "duels_lost")
                        .ConfigureAwait(false);
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_DUEL_RULES
            [Command("rules")]
            [Description("Explain the Duel game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExample("!game duel rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.RespondWithIconEmbedAsync(
                    "\nDuel is a death battle with no rules! Rumours say that typing ``hp`` might heal give you an extra boost during the duel...",
                    ":book:"
                ).ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_DUEL_STATS
            [Command("stats")]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExample("!game duel stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                var top = await Database.GetTopDuelistsStringAsync(ctx.Client)
                    .ConfigureAwait(false);

                await ctx.RespondWithIconEmbedAsync(EmojiUtil.Trophy, $"Top Duelists:\n\n{top}")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
