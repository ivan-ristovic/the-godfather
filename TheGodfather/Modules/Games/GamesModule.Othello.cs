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
using TheGodfather.Modules.Games.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("othello")]
        [Aliases("reversi", "oth", "rev")]
        [RequireGuild]
        public sealed class OthelloModule : TheGodfatherServiceModule<ChannelEventService>
        {
            #region game othello
            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-game-movetime")] TimeSpan? moveTime = null)
            {
                if (moveTime?.TotalSeconds is < 2 or > 120)
                    throw new InvalidCommandUsageException(ctx, "cmd-err-game-movetime", 2, 120);

                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException(ctx, "cmd-err-evt-dup");

                DiscordUser? opponent = await ctx.WaitForGameOpponentAsync();
                if (opponent is null)
                    throw new CommandFailedException(ctx, "cmd-err-game-op-none", ctx.User.Mention);

                var game = new OthelloGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, moveTime);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await game.RunAsync(this.Localization);

                    if (game.Winner is { }) {
                        if (game.IsTimeoutReached)
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "str-game-timeout", game.Winner.Mention);
                        else
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "fmt-winners", game.Winner.Mention);

                        GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                        await gss.UpdateStatsAsync(game.Winner.Id, s => s.OthelloWon++);
                        await gss.UpdateStatsAsync(game.Winner == ctx.User ? opponent.Id : ctx.User.Id, s => s.OthelloLost++);
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Joystick, "str-game-draw");
                    }

                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region game othello rules
            [Command("rules")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
                => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, "str-game-ot");
            #endregion

            #region game othello stats
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
                        emb.WithDescription(stats.BuildOthelloStatsString());
                });
            }
            #endregion

            #region game othello top
            [Command("top")]
            [Aliases("t", "leaderboard")]
            public async Task TopAsync(CommandContext ctx)
            {
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                IReadOnlyList<GameStats> topStats = await gss.GetTopOthelloStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildOthelloStatsString());
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "fmt-game-ot-top", topStats);
            }
            #endregion
        }
    }
}
