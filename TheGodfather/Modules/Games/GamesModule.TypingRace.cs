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
                if (this.Service.GetEventInChannel(ctx.Channel.Id) is TypingRaceGame)
                    await this.JoinAsync(ctx);
                else
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_evt_dup);
                return;
            }

            var game = new TypingRaceGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.Services.GetRequiredService<FontsService>());
            this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
            try {
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, TranslationKey.str_game_tr_start(TypingRaceGame.MaxParticipants));
                await this.JoinAsync(ctx);
                await Task.Delay(TimeSpan.FromSeconds(30));

                if (game.ParticipantCount > 1) {
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, TranslationKey.str_game_tr_starting(TypingRaceGame.MistakeThreshold));
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    await game.RunAsync(this.Localization);

                    GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                    if (game.Winner is not null) {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, TranslationKey.fmt_winners(game.Winner.Mention));
                        await gss.UpdateStatsAsync(game.Winner.Id, s => s.TypingRacesWon++);
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, TranslationKey.str_game_tr_fail);
                    }
                } else {
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.AlarmClock, TranslationKey.str_game_tr_none);
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
            if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out TypingRaceGame game))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_tr_none);

            if (game.Started)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_tr_started);

            if (game.ParticipantCount >= TypingRaceGame.MaxParticipants)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_tr_full(TypingRaceGame.MaxParticipants));

            if (!game.AddParticipant(ctx.User))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_game_tr_dup);

            return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Gun, TranslationKey.fmt_game_tr_join(ctx.User.Mention));
        }
        #endregion

        #region game typingrace stats
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
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, TranslationKey.fmt_game_tr_top(top));
        }
        #endregion
    }
}