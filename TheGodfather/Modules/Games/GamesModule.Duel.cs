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
using TheGodfather.Modules.Games.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("duel")]
        [Aliases("fight", "vs", "d")]
        [RequireGuild]
        public sealed class DuelModule : TheGodfatherServiceModule<ChannelEventService>
        {
            #region game duel
            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-member")] DiscordMember opponent)
            {
                if (opponent == ctx.User)
                    throw new InvalidCommandUsageException(ctx, "cmd-err-self-action");
                
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException(ctx, "cmd-err-evt-dup");

                if (opponent == ctx.Client.CurrentUser) {
                    await ctx.RespondWithLocalizedEmbedAsync(emb => {
                        emb.WithDescription(DuelGame.AgainstBot(ctx.User.Mention, opponent.Mention));
                        emb.WithColor(this.ModuleColor);
                    });
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.DuelSwords, "fmt-winners", opponent.Mention);
                    return;
                }

                var game = new DuelGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await game.RunAsync(this.Localization);
                    if (game.Winner is { }) {
                        GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                        await gss.UpdateStatsAsync(game.Winner.Id, s => s.DuelsWon++);
                        await gss.UpdateStatsAsync(game.Winner == ctx.User ? opponent.Id : ctx.User.Id, s => s.DuelsLost++);
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Joystick, "str-game-draw");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region game duel rules
            [Command("rules")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
                => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, "str-game-duel");
            #endregion

            #region game duel stats
            [Command("stats"), Priority(1)]
            [Aliases("s")]
            public Task StatsAsync(CommandContext ctx,
                                  [Description("desc-member")] DiscordMember? member = null)
                => this.StatsAsync(ctx, member as DiscordUser);

            [Command("stats"), Priority(0)]
            public async Task StatsAsync(CommandContext ctx,
                                        [Description("desc-user")] DiscordUser? user = null)
            {
                user ??= ctx.User;
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();

                GameStats? stats = await gss.GetAsync(user.Id);
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle("fmt-game-stats", user.ToDiscriminatorString());
                    emb.WithColor(this.ModuleColor);
                    emb.WithThumbnail(user.AvatarUrl);
                    if (stats is null)
                        emb.WithLocalizedDescription("str-game-stats-none");
                    else
                        emb.WithDescription(stats.BuildDuelStatsString());
                });
            }
            #endregion

            #region game duel top
            [Command("top")]
            [Aliases("t", "leaderboard")]
            public async Task TopAsync(CommandContext ctx)
            {
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                IReadOnlyList<GameStats> topStats = await gss.GetTopDuelStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildDuelStatsString());
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "fmt-game-duel-top", top);
            }
            #endregion
        }
    }
}
