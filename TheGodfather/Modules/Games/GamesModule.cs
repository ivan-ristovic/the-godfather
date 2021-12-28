using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Modules.Games.Services;

namespace TheGodfather.Modules.Games
{
    [Group("game"), Module(ModuleType.Games), NotBlocked]
    [Aliases("games", "gm")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed partial class GamesModule : TheGodfatherServiceModule<GameStatsService>
    {
        #region game
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
        {
            var sb = new StringBuilder();
            sb.AppendLine().AppendLine();
            sb.Append(Emojis.SmallBlueDiamond).AppendLine(" animalrace");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine(" caro");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine(" connect4");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine(" duel");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine(" hangman");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine(" numberrace");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine(" othello");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine(" quiz");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine(" rps");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine(" russianroulette");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine(" tictactoe");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine(" typingrace");

            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.str_games);
                emb.WithDescription(sb.ToString());
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedFooter(TranslationKey.str_games_help, null);
            });
        }
        #endregion

        #region game leaderboard
        [Command("leaderboard")]
        [Aliases("globalstats")]
        public Task LeaderboardAsync(CommandContext ctx)
        {
            return ctx.RespondWithLocalizedEmbedAsync(async emb => {
                emb.WithLocalizedTitle(TranslationKey.str_hall_of_fame(Emojis.Trophy, Emojis.Trophy));
                emb.WithColor(this.ModuleColor);

                IReadOnlyList<GameStats> topStats;
                string top;

                topStats = await this.Service.GetTopAnimalRaceStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildAnimalRaceStatsString());
                emb.AddLocalizedField(TranslationKey.str_game_top_ar, top, inline: true);

                topStats = await this.Service.GetTopCaroStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildCaroStatsString());
                emb.AddLocalizedField(TranslationKey.str_game_top_caro, top, inline: true);

                topStats = await this.Service.GetTopConnect4StatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildConnect4StatsString());
                emb.AddLocalizedField(TranslationKey.str_game_top_c4, top, inline: true);

                topStats = await this.Service.GetTopDuelStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildDuelStatsString());
                emb.AddLocalizedField(TranslationKey.str_game_top_duel, top, inline: true);

                topStats = await this.Service.GetTopHangmanStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildHangmanStatsString());
                emb.AddLocalizedField(TranslationKey.str_game_top_hm, top, inline: true);

                topStats = await this.Service.GetTopNumberRaceStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildNumberRaceStatsString());
                emb.AddLocalizedField(TranslationKey.str_game_top_nr, top, inline: true);

                topStats = await this.Service.GetTopTypingRaceStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildTypingRaceStatsString());
                emb.AddLocalizedField(TranslationKey.str_game_top_tr, top, inline: true);

                topStats = await this.Service.GetTopRussianRouletteStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildRussianRouletteStatsString());
                emb.AddLocalizedField(TranslationKey.str_game_top_rr, top, inline: true);

                topStats = await this.Service.GetTopOthelloStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildOthelloStatsString());
                emb.AddLocalizedField(TranslationKey.str_game_top_ot, top, inline: true);

                topStats = await this.Service.GetTopQuizStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildQuizStatsString());
                emb.AddLocalizedField(TranslationKey.str_game_top_quiz, top, inline: true);

                topStats = await this.Service.GetTopTicTacToeStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildTicTacToeStatsString());
                emb.AddLocalizedField(TranslationKey.str_game_top_ttt, top, inline: true);
            });
        }
        #endregion

        #region game stats
        [Command("stats"), Priority(1)]
        [Aliases("s", "st")]
        public Task StatsAsync(CommandContext ctx,
                              [Description(TranslationKey.desc_member)] DiscordMember? member = null)
            => this.StatsAsync(ctx, member as DiscordUser);

        [Command("stats"), Priority(0)]
        public async Task StatsAsync(CommandContext ctx,
                                    [Description(TranslationKey.desc_user)] DiscordUser? user = null)
        {
            user ??= ctx.User;

            GameStats? stats = await this.Service.GetAsync(user.Id);
            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.fmt_game_stats(user.ToDiscriminatorString()));
                emb.WithColor(this.ModuleColor);
                emb.WithThumbnail(user.AvatarUrl);
                if (stats is null) {
                    emb.WithLocalizedDescription(TranslationKey.str_game_stats_none);
                } else {
                    emb.AddLocalizedField(TranslationKey.str_game_stats_duel, stats.BuildDuelStatsString());
                    emb.AddLocalizedField(TranslationKey.str_game_stats_ttt, stats.BuildTicTacToeStatsString());
                    emb.AddLocalizedField(TranslationKey.str_game_stats_c4, stats.BuildConnect4StatsString());
                    emb.AddLocalizedField(TranslationKey.str_game_stats_caro, stats.BuildCaroStatsString());
                    emb.AddLocalizedField(TranslationKey.str_game_stats_ot, stats.BuildOthelloStatsString());
                    emb.AddLocalizedField(TranslationKey.str_game_stats_nr, stats.BuildNumberRaceStatsString(), inline: true);
                    emb.AddLocalizedField(TranslationKey.str_game_stats_quiz, stats.BuildQuizStatsString(), inline: true);
                    emb.AddLocalizedField(TranslationKey.str_game_stats_ar, stats.BuildAnimalRaceStatsString(), inline: true);
                    emb.AddLocalizedField(TranslationKey.str_game_stats_rr, stats.BuildRussianRouletteStatsString(), inline: true);
                    emb.AddLocalizedField(TranslationKey.str_game_stats_tr, stats.BuildTypingRaceStatsString(), inline: true);
                    emb.AddLocalizedField(TranslationKey.str_game_stats_hm, stats.BuildHangmanStatsString(), inline: true);
                }
            });
        }
        #endregion
    }
}
