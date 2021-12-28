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
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_evt_dup);
                return;
            }

            var game = new NumberRace(ctx.Client.GetInteractivity(), ctx.Channel);
            this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
            try {
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, TranslationKey.str_game_nr_start(NumberRace.MaxParticipants));
                await this.JoinAsync(ctx);
                await Task.Delay(TimeSpan.FromSeconds(30));

                if (game.ParticipantCount > 1) {
                    GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                    await game.RunAsync(this.Localization);

                    if (game.Winner is { }) {
                        if (game.IsTimeoutReached)
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, TranslationKey.cmd_err_game_timeout_w(game.Winner.Mention));
                        else
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, TranslationKey.fmt_winners(game.Winner.Mention));
                        await gss.UpdateStatsAsync(game.Winner.Id, s => s.NumberRacesWon++);
                    } else {
                        await ctx.FailAsync(TranslationKey.cmd_err_game_timeout);
                    }
                } else {
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.AlarmClock, TranslationKey.str_game_nr_none);
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
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_nr_none);

            if (game.Started)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_nr_started);

            if (game.ParticipantCount >= NumberRace.MaxParticipants)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_nr_full(NumberRace.MaxParticipants));

            if (!game.AddParticipant(ctx.User))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_nr_dup);

            return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Numbers.Get(1), TranslationKey.fmt_game_nr_join(ctx.User.Mention));
        }
        #endregion

        #region game numberrace rules
        [Command("rules")]
        [Aliases("help", "h", "ruling", "rule")]
        public Task RulesAsync(CommandContext ctx)
            => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, TranslationKey.str_game_nr);
        #endregion

        #region game numberrace stats
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
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, TranslationKey.fmt_game_nr_top(top));
        }
        #endregion
    }
}