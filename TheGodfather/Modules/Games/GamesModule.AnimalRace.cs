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
        [Group("animalrace")]
        [Aliases("animr", "arace", "ar", "animalr", "race")]
        [RequireGuild]
        public sealed class AnimalRaceModule : TheGodfatherServiceModule<ChannelEventService>
        {
            #region game animalrace
            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Service.GetEventInChannel(ctx.Channel.Id) is AnimalRace)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException(ctx, "cmd-err-evt-dup");
                    return;
                }

                var game = new AnimalRace(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, "str-game-ar-start", AnimalRace.MaxParticipants);
                    await this.JoinAsync(ctx);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (game.ParticipantCount > 1) {
                        GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                        await game.RunAsync(this.Localization);

                        if (game.WinnerIds is { })
                            await Task.WhenAll(game.WinnerIds.Select(w => gss.UpdateStatsAsync(w, s => s.AnimalRacesWon++)));
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.AlarmClock, "str-game-ar-none");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region game animalrace join
            [Command("join")]
            [Aliases("+", "compete", "enter", "j", "<<", "<")]
            public Task JoinAsync(CommandContext ctx)
            {
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out AnimalRace? game) || game is null)
                    throw new CommandFailedException(ctx, "cmd-err-game-ar-none");

                if (game.Started)
                    throw new CommandFailedException(ctx, "cmd-err-game-ar-started");

                if (game.ParticipantCount >= AnimalRace.MaxParticipants)
                    throw new CommandFailedException(ctx, "cmd-err-game-ar-full", AnimalRace.MaxParticipants);

                if (!game.AddParticipant(ctx.User, out DiscordEmoji? emoji))
                    throw new CommandFailedException(ctx, "cmd-err-game-ar-dup");

                return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Bicyclist, "fmt-game-ar-join", ctx.User.Mention, emoji);
            }
            #endregion

            #region game animalrace stats
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
                        emb.WithDescription(stats.BuildAnimalRaceStatsString());
                });
            }
            #endregion

            #region game animalrace top
            [Command("top")]
            [Aliases("t", "leaderboard")]
            public async Task TopAsync(CommandContext ctx)
            {
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                IReadOnlyList<GameStats> topStats = await gss.GetTopAnimalRaceStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildAnimalRaceStatsString());
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "fmt-game-ar-top", top);
            }
            #endregion
        }
    }
}
