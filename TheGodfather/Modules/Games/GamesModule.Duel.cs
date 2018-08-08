#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("duel")]
        [Description("Starts a duel which I will commentate.")]
        [Aliases("fight", "vs", "d")]
        [UsageExamples("!game duel @Someone")]
        public class DuelModule : TheGodfatherModule
        {

            public DuelModule(SharedData shared, DBService db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Teal;
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Who to fight with?")] DiscordUser opponent)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

                if (opponent.Id == ctx.User.Id)
                    throw new CommandFailedException("You can't duel yourself...");

                if (opponent.Id == ctx.Client.CurrentUser.Id) {
                    await ctx.RespondAsync(
                        $"{ctx.User.Mention} {string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":black_large_square:"), 5))} :crossed_swords: " +
                        $"{string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":white_large_square:"), 5))} {opponent.Mention}" +
                        $"\n{ctx.Client.CurrentUser.Mention} {DiscordEmoji.FromName(ctx.Client, ":zap:")} {ctx.User.Mention}"
                    );
                    await InformAsync(ctx, StaticDiscordEmoji.DuelSwords, $"{ctx.Client.CurrentUser.Mention} wins!");
                    return;
                }

                var duel = new DuelGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent);
                this.Shared.RegisterEventInChannel(duel, ctx.Channel.Id);

                try {
                    await duel.RunAsync();

                    await InformAsync(ctx, StaticDiscordEmoji.DuelSwords, $"{duel.Winner.Username} {duel.FinishingMove ?? "wins"}!");

                    await this.Database.UpdateUserStatsAsync(duel.Winner.Id, GameStatsType.DuelsWon);
                    await this.Database.UpdateUserStatsAsync(duel.Winner.Id == ctx.User.Id ? opponent.Id : ctx.User.Id, GameStatsType.DuelsLost);
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_DUEL_RULES
            [Command("rules")]
            [Description("Explain the Duel game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!game duel rules")]
            public Task RulesAsync(CommandContext ctx)
            {
                return InformAsync(ctx,
                    StaticDiscordEmoji.Information,
                    "\nDuel is a death battle with no rules! Rumours say that typing ``hp`` might heal give you " +
                    "an extra boost during the duel..."
                );
            }
            #endregion

            #region COMMAND_DUEL_STATS
            [Command("stats")]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExamples("!game duel stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                string top = await this.Database.GetTopDuelistsStringAsync(ctx.Client);
                await InformAsync(ctx, StaticDiscordEmoji.Trophy, $"Top Duelists:\n\n{top}");
            }
            #endregion
        }
    }
}
