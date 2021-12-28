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
        [Group("caro")]
        [Aliases("c", "gomoku", "gobang")]
        [RequireGuild]
        public sealed class CaroModule : TheGodfatherServiceModule<ChannelEventService>
        {
            #region game caro
            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description(TranslationKey.desc_game_movetime)] TimeSpan? moveTime = null)
            {
                if (moveTime?.TotalSeconds is < 2 or > 120)
                    throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_game_movetime(2, 120));

                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_evt_dup);

                DiscordUser? opponent = await ctx.WaitForGameOpponentAsync();
                if (opponent is null)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_op_none(ctx.User.Mention));

                var game = new CaroGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, moveTime);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await game.RunAsync(this.Localization);

                    if (game.Winner is { }) {
                        if (game.IsTimeoutReached)
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, TranslationKey.str_game_timeout(game.Winner.Mention));
                        else
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, TranslationKey.fmt_winners(game.Winner.Mention));

                        GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                        await gss.UpdateStatsAsync(game.Winner.Id, s => s.CaroWon++);
                        await gss.UpdateStatsAsync(game.Winner == ctx.User ? opponent.Id : ctx.User.Id, s => s.CaroLost++);
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Joystick, TranslationKey.str_game_draw);
                    }

                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region game caro rules
            [Command("rules")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
                => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, TranslationKey.str_game_caro);
            #endregion

            #region game caro stats
            [Command("stats"), Priority(1)]
            [Aliases("s")]
            public Task StatsAsync(CommandContext ctx,
                                  [Description(TranslationKey.desc_member)] DiscordMember? member = null)
                => this.StatsAsync(ctx, member as DiscordUser);

            [Command("stats"), Priority(0)]
            public async Task StatsAsync(CommandContext ctx,
                                        [Description(TranslationKey.desc_user)] DiscordUser? user = null)
            {
                user ??= ctx.User;
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();

                GameStats? stats = await gss.GetAsync(user.Id);
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle(TranslationKey.fmt_game_stats(user.ToDiscriminatorString()));
                    emb.WithColor(this.ModuleColor);
                    emb.WithThumbnail(user.AvatarUrl);
                    if (stats is null)
                        emb.WithLocalizedDescription(TranslationKey.str_game_stats_none);
                    else
                        emb.WithDescription(stats.BuildCaroStatsString());
                });
            }
            #endregion

            #region game caro top
            [Command("top")]
            [Aliases("t", "leaderboard")]
            public async Task TopAsync(CommandContext ctx)
            {
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                IReadOnlyList<GameStats> topStats = await gss.GetTopCaroStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildCaroStatsString());
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, TranslationKey.fmt_game_caro_top(top));
            }
            #endregion
        }
    }
}
