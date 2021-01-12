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
        [Group("typingrace")]
        [Aliases("tr", "trace", "typerace", "typing", "typingr")]
        [RequireGuild]
        public sealed class TypingRaceModule : TheGodfatherServiceModule<ChannelEventService>
        {
            #region game typingrace
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

                var game = new TypingRaceGame(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, "str-game-tr-start", TypingRaceGame.MaxParticipants);
                    await this.JoinAsync(ctx);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (game.ParticipantCount > 1) {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, "str-game-tr-starting", TypingRaceGame.MistakeThreshold);
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        await game.RunAsync(this.Localization);

                        GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                        if (game.Winner is { }) {
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "fmt-winners", game.Winner.Mention);
                            await gss.UpdateStatsAsync(game.Winner.Id, s => s.TypingRacesWon++);
                        } else {
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, "str-game-tr-fail");
                        }
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.AlarmClock, "str-game-tr-none");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region game typingrace join
            [Command("join")]
            [Aliases("+", "compete", "enter", "j", "<<", "<")]
            public Task JoinAsync(CommandContext ctx)
            {
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out TypingRaceGame? game) || game is null)
                    throw new CommandFailedException(ctx, "cmd-err-game-tr-none");

                if (game.Started)
                    throw new CommandFailedException(ctx, "cmd-err-game-tr-started");

                if (game.ParticipantCount >= TypingRaceGame.MaxParticipants)
                    throw new CommandFailedException(ctx, "cmd-err-game-tr-full", TypingRaceGame.MaxParticipants);

                if (!game.AddParticipant(ctx.User))
                    throw new CommandFailedException(ctx, "cmd-err-game-tr-dup");

                return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Gun, "fmt-game-tr-join", ctx.User.Mention);
            }
            #endregion

            #region game typingrace stats
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
                        emb.WithDescription(stats.BuildTypingRaceStatsString());
                });
            }
            #endregion

            #region game typingrace top
            [Command("top")]
            [Aliases("t", "leaderboard")]
            public async Task TopAsync(CommandContext ctx)
            {
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                IReadOnlyList<GameStats> topStats = await gss.GetTopTypingRaceStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildTypingRaceStatsString());
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "fmt-game-tr-top", top);
            }
            #endregion
        }
    }
}
