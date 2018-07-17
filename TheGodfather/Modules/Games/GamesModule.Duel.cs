#region USING_DIRECTIVES
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services.Common;
using TheGodfather.Services.Database.Stats;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule : TheGodfatherBaseModule
    {
        [Group("duel"), Module(ModuleType.Games)]
        [Description("Starts a duel which I will commentate.")]
        [Aliases("fight", "vs", "d")]
        [UsageExamples("!game duel @Someone")]
        public class DuelModule : TheGodfatherBaseModule
        {

            public DuelModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Who to fight with?")] DiscordUser opponent)
            {
                if (ChannelEvent.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another event is already running in the current channel!");

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
                ChannelEvent.RegisterEventInChannel(duel, ctx.Channel.Id);

                try {
                    await duel.RunAsync()
                        .ConfigureAwait(false);

                    await ctx.RespondAsync($"{duel.Winner.Username} {duel.FinishingMove ?? "wins"}!")
                        .ConfigureAwait(false);

                    await Database.UpdateUserStatsAsync(duel.Winner.Id, GameStatsType.DuelsWon)
                        .ConfigureAwait(false);
                    await Database.UpdateUserStatsAsync(duel.Winner.Id == ctx.User.Id ? opponent.Id : ctx.User.Id, GameStatsType.DuelsLost)
                        .ConfigureAwait(false);
                } finally {
                    ChannelEvent.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_DUEL_RULES
            [Command("rules"), Module(ModuleType.Games)]
            [Description("Explain the Duel game rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!game duel rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.InformSuccessAsync(
                    "\nDuel is a death battle with no rules! Rumours say that typing ``hp`` might heal give you an extra boost during the duel...",
                    ":book:"
                ).ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_DUEL_STATS
            [Command("stats"), Module(ModuleType.Games)]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            [UsageExamples("!game duel stats")]
            public async Task StatsAsync(CommandContext ctx)
            {
                var top = await Database.GetTopDuelistsStringAsync(ctx.Client)
                    .ConfigureAwait(false);

                await ctx.InformSuccessAsync(StaticDiscordEmoji.Trophy, $"Top Duelists:\n\n{top}")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
