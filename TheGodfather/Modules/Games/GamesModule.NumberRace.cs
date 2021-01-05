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
        [Group("numberrace")]
        [Aliases("nr", "n", "nunchi", "numbers", "numbersrace")]
        [RequireGuild]
        public sealed class NumberRaceModule : TheGodfatherServiceModule<ChannelEventService>
        {
            #region game numberrace
            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Service.GetEventInChannel(ctx.Channel.Id) is NumberRace)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException(ctx, "cmd-err-evt-dup");
                    return;
                }

                var game = new NumberRace(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, "str-game-nr-start", NumberRace.MaxParticipants);
                    await this.JoinAsync(ctx);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (game.ParticipantCount > 1) {
                        GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                        await game.RunAsync(this.Localization);

                        if (game.Winner is { }) {
                            if (game.IsTimeoutReached)
                                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "cmd-err-game-timeout-w", game.Winner.Mention);
                            else
                                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "fmt-winners", game.Winner.Mention);
                            await gss.UpdateStatsAsync(game.Winner.Id, s => s.NumberRacesWon++);
                        } else {
                            await ctx.FailAsync("cmd-err-game-timeout");
                        }
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.AlarmClock, "str-game-nr-none");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region game numberrace join
            [Command("join")]
            [Aliases("+", "compete", "enter", "j", "<<", "<")]
            public Task JoinAsync(CommandContext ctx)
            {
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out NumberRace? game) || game is null)
                    throw new CommandFailedException(ctx, "cmd-err-game-nr-none");

                if (game.Started)
                    throw new CommandFailedException(ctx, "cmd-err-game-nr-started");

                if (game.ParticipantCount >= NumberRace.MaxParticipants)
                    throw new CommandFailedException(ctx, "cmd-err-game-nr-full", NumberRace.MaxParticipants);

                if (!game.AddParticipant(ctx.User))
                    throw new CommandFailedException(ctx, "cmd-err-game-nr-dup");

                return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Numbers.Get(1), "fmt-game-nr-join", ctx.User.Mention);
            }
            #endregion

            #region game numberrace rules
            [Command("rules")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
                => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, "str-game-nr");
            #endregion

            #region game numberrace stats
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
                        emb.WithDescription(stats.BuildNumberRaceStatsString());
                });
            }
            #endregion

            #region game numberrace top
            [Command("top")]
            [Aliases("t", "leaderboard")]
            public async Task TopAsync(CommandContext ctx)
            {
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                IReadOnlyList<GameStats> topStats = await gss.GetTopNumberRaceStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildNumberRaceStatsString());
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "fmt-game-nr-top", topStats);
            }
            #endregion
        }
    }
}