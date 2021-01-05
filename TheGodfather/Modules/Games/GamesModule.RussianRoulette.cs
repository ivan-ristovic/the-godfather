using System;
using System.Collections.Generic;
using System.Linq;
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
        [Group("russianroulette")]
        [Aliases("rr", "roulette", "russianr")]
        [RequireGuild]
        public sealed class RussianRouletteModule : TheGodfatherServiceModule<ChannelEventService>
        {
            #region game russianroulette
            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Service.GetEventInChannel(ctx.Channel.Id) is RussianRouletteGame)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException(ctx, "cmd-err-evt-dup");
                    return;
                }

                var game = new RussianRouletteGame(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, "str-game-rr-start", RussianRouletteGame.MaxParticipants);
                    await this.JoinAsync(ctx);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (game.ParticipantCount > 1) {
                        GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                        await game.RunAsync(this.Localization);

                        if (game.Survivors.Any()) {
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "fmt-winners", game.Survivors.Select(u => u.Mention).JoinWith(", "));
                            await Task.WhenAll(game.Survivors.Select(u => gss.UpdateStatsAsync(u.Id, s => s.RussianRoulettesWon++)));
                        } else {
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dead, "str-game-rr-alldead");
                        }
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.AlarmClock, "str-game-rr-none");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region game russianroulette join
            [Command("join")]
            [Aliases("+", "compete", "enter", "j", "<<", "<")]
            public Task JoinAsync(CommandContext ctx)
            {
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out RussianRouletteGame? game) || game is null)
                    throw new CommandFailedException(ctx, "cmd-err-game-rr-none");

                if (game.Started)
                    throw new CommandFailedException(ctx, "cmd-err-game-rr-started");

                if (game.ParticipantCount >= RussianRouletteGame.MaxParticipants)
                    throw new CommandFailedException(ctx, "cmd-err-game-rr-full", RussianRouletteGame.MaxParticipants);

                if (!game.AddParticipant(ctx.User))
                    throw new CommandFailedException(ctx, "cmd-err-game-rr-dup");

                return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Gun, "fmt-game-rr-join", ctx.User.Mention);
            }
            #endregion

            #region game russianroulette stats
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
                        emb.WithDescription(stats.BuildRussianRouletteStatsString());
                });
            }
            #endregion

            #region game russianroulette top
            [Command("top")]
            [Aliases("t", "leaderboard")]
            public async Task TopAsync(CommandContext ctx)
            {
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                IReadOnlyList<GameStats> topStats = await gss.GetTopRussianRouletteStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildRussianRouletteStatsString());
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "fmt-game-rr-top", topStats);
            }
            #endregion
        }
    }
}
