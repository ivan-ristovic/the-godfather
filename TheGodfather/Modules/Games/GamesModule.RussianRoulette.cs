using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Modules.Games.Services;

namespace TheGodfather.Modules.Games;

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
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_evt_dup);
                return;
            }

            var game = new RussianRouletteGame(ctx.Client.GetInteractivity(), ctx.Channel);
            this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
            try {
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, TranslationKey.str_game_rr_start(RussianRouletteGame.MaxParticipants));
                await this.JoinAsync(ctx);
                await Task.Delay(TimeSpan.FromSeconds(30));

                if (game.ParticipantCount > 1) {
                    GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                    await game.RunAsync(this.Localization);

                    if (game.Survivors.Any()) {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, TranslationKey.fmt_winners(game.Survivors.Select(u => u.Mention).JoinWith(", ")));
                        await Task.WhenAll(game.Survivors.Select(u => gss.UpdateStatsAsync(u.Id, s => s.RussianRoulettesWon++)));
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dead, TranslationKey.str_game_rr_alldead);
                    }
                } else {
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.AlarmClock, TranslationKey.str_game_rr_none);
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
            if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out RussianRouletteGame game))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_rr_none);

            if (game.Started)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_rr_started);

            if (game.ParticipantCount >= RussianRouletteGame.MaxParticipants)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_rr_full(RussianRouletteGame.MaxParticipants));

            if (!game.AddParticipant(ctx.User))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_rr_dup);

            return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Gun, TranslationKey.fmt_game_rr_join(ctx.User.Mention));
        }
        #endregion

        #region game russianroulette stats
        [Command("stats")][Priority(1)]
        [Aliases("s")]
        public Task StatsAsync(CommandContext ctx,
            [Description(TranslationKey.desc_member)] DiscordMember? member = null)
            => this.StatsAsync(ctx, member as DiscordUser);

        [Command("stats")][Priority(0)]
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
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, TranslationKey.fmt_game_rr_top(top));
        }
        #endregion
    }
}