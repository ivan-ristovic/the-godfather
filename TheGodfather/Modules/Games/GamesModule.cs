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
                emb.WithLocalizedTitle("str-games");
                emb.WithDescription(sb.ToString());
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedFooter("str-games-help", null);
            });
        }
        #endregion

        #region game leaderboard
        [Command("leaderboard")]
        [Aliases("globalstats")]
        public Task LeaderboardAsync(CommandContext ctx)
        {
            return ctx.RespondWithLocalizedEmbedAsync(async emb => {
                emb.WithLocalizedTitle("str-hall-of-fame", Emojis.Trophy, Emojis.Trophy);
                emb.WithColor(this.ModuleColor);

                IReadOnlyList<GameStats> topStats;
                string top;

                topStats = await this.Service.GetTopAnimalRaceStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildAnimalRaceStatsString());
                emb.AddLocalizedTitleField("str-game-top-ar", top, inline: true);

                topStats = await this.Service.GetTopCaroStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildCaroStatsString());
                emb.AddLocalizedTitleField("str-game-top-caro", top, inline: true);

                topStats = await this.Service.GetTopConnect4StatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildConnect4StatsString());
                emb.AddLocalizedTitleField("str-game-top-c4", top, inline: true);

                topStats = await this.Service.GetTopDuelStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildDuelStatsString());
                emb.AddLocalizedTitleField("str-game-top-duel", top, inline: true);

                topStats = await this.Service.GetTopHangmanStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildHangmanStatsString());
                emb.AddLocalizedTitleField("str-game-top-hm", top, inline: true);

                topStats = await this.Service.GetTopNumberRaceStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildNumberRaceStatsString());
                emb.AddLocalizedTitleField("str-game-top-nr", top, inline: true);

                topStats = await this.Service.GetTopRussianRouletteStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildRussianRouletteStatsString());
                emb.AddLocalizedTitleField("str-game-top-rr", top, inline: true);

                topStats = await this.Service.GetTopOthelloStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildOthelloStatsString());
                emb.AddLocalizedTitleField("str-game-top-ot", top, inline: true);

                topStats = await this.Service.GetTopQuizStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildQuizStatsString());
                emb.AddLocalizedTitleField("str-game-top-quiz", top, inline: true);

                topStats = await this.Service.GetTopTicTacToeStatsAsync();
                top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildTicTacToeStatsString());
                emb.AddLocalizedTitleField("str-game-top-ttt", top, inline: true);
            });
        }
        #endregion

        #region game stats
        [Command("stats"), Priority(1)]
        [Aliases("s", "st")]
        public Task StatsAsync(CommandContext ctx,
                              [Description("desc-member")] DiscordMember? member = null)
            => this.StatsAsync(ctx, member as DiscordUser);

        [Command("stats"), Priority(0)]
        public async Task StatsAsync(CommandContext ctx,
                                    [Description("desc-user")] DiscordUser? user = null)
        {
            user ??= ctx.User;

            GameStats? stats = await this.Service.GetAsync(user.Id);
            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle("fmt-game-stats", user.ToDiscriminatorString());
                emb.WithColor(this.ModuleColor);
                emb.WithThumbnail(user.AvatarUrl);
                if (stats is null) {
                    emb.WithLocalizedDescription("str-game-stats-none");
                } else {
                    emb.AddLocalizedTitleField("str-game-stats-duel", stats.BuildDuelStatsString());
                    emb.AddLocalizedTitleField("str-game-stats-ttt", stats.BuildTicTacToeStatsString());
                    emb.AddLocalizedTitleField("str-game-stats-c4", stats.BuildConnect4StatsString());
                    emb.AddLocalizedTitleField("str-game-stats-caro", stats.BuildCaroStatsString());
                    emb.AddLocalizedTitleField("str-game-stats-ot", stats.BuildOthelloStatsString());
                    emb.AddLocalizedTitleField("str-game-stats-nr", stats.BuildNumberRaceStatsString(), inline: true);
                    emb.AddLocalizedTitleField("str-game-stats-quiz", stats.BuildQuizStatsString(), inline: true);
                    emb.AddLocalizedTitleField("str-game-stats-ar", stats.BuildAnimalRaceStatsString(), inline: true);
                    emb.AddLocalizedTitleField("str-game-stats-rr", stats.BuildRussianRouletteStatsString(), inline: true);
                    emb.AddLocalizedTitleField("str-game-stats-hm", stats.BuildHangmanStatsString(), inline: true);
                }
            });
        }
        #endregion
    }
}
