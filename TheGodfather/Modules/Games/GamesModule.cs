#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Games.Extensions;
#endregion

namespace TheGodfather.Modules.Games
{
    [Group("game"), Module(ModuleType.Games), NotBlocked]
    [Description("Starts a game for you to play!")]
    [Aliases("games", "gm")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class GamesModule : TheGodfatherModule
    {

        public GamesModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Teal;
        }
        

        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
        {
            var sb = new StringBuilder();

            sb.AppendLine().AppendLine();
            sb.Append(StaticDiscordEmoji.SmallBlueDiamond).AppendLine(" animalrace");
            sb.Append(StaticDiscordEmoji.SmallBlueDiamond).AppendLine(" caro");
            sb.Append(StaticDiscordEmoji.SmallBlueDiamond).AppendLine(" connect4");
            sb.Append(StaticDiscordEmoji.SmallBlueDiamond).AppendLine(" duel");
            sb.Append(StaticDiscordEmoji.SmallBlueDiamond).AppendLine(" hangman");
            sb.Append(StaticDiscordEmoji.SmallBlueDiamond).AppendLine(" numberrace");
            sb.Append(StaticDiscordEmoji.SmallBlueDiamond).AppendLine(" othello");
            sb.Append(StaticDiscordEmoji.SmallBlueDiamond).AppendLine(" quiz");
            sb.Append(StaticDiscordEmoji.SmallBlueDiamond).AppendLine(" rps");
            sb.Append(StaticDiscordEmoji.SmallBlueDiamond).AppendLine(" russianroulette");
            sb.Append(StaticDiscordEmoji.SmallBlueDiamond).AppendLine(" tictactoe");
            sb.Append(StaticDiscordEmoji.SmallBlueDiamond).AppendLine(" typingrace");

            return ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = "Available games:",
                Description = sb.ToString(),
                Color = this.ModuleColor,
            }.WithFooter("Start a game by typing: game <game name>").Build());
        }


        #region COMMAND_GAME_LEADERBOARD
        [Command("leaderboard")]
        [Description("View the global game leaderboard.")]
        [Aliases("globalstats")]
        [UsageExamples("!game leaderboard")]
        public async Task LeaderboardAsync(CommandContext ctx)
        {
            var emb = new DiscordEmbedBuilder {
                Title = $"{StaticDiscordEmoji.Trophy} HALL OF FAME {StaticDiscordEmoji.Trophy}",
                Color = DiscordColor.Chartreuse
            };

            IReadOnlyList<DatabaseGameStats> topStats;
            string top;

            topStats = await this.Database.GetTopAnimalRaceStatsAsync();
            top = await DatabaseGameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildAnimalRaceStatsString());
            emb.AddField("Top players in Animal Race", top, inline: true);

            topStats = await this.Database.GetTopCaroStatsAsync();
            top = await DatabaseGameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildCaroStatsString());
            emb.AddField("Top players in Caro", top, inline: true);

            topStats = await this.Database.GetTopChain4StatsAsync();
            top = await DatabaseGameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildChain4StatsString());
            emb.AddField("Top players in Connect4", top, inline: true);

            topStats = await this.Database.GetTopDuelStatsAsync();
            top = await DatabaseGameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildDuelStatsString());
            emb.AddField("Top players in Duel", top, inline: true);

            topStats = await this.Database.GetTopHangmanStatsAsync();
            top = await DatabaseGameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildHangmanStatsString());
            emb.AddField("Top players in Hangman", top, inline: true);

            topStats = await this.Database.GetTopNumberRaceStatsAsync();
            top = await DatabaseGameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildNumberRaceStatsString());
            emb.AddField("Top players in Number Race", top, inline: true);

            topStats = await this.Database.GetTopOthelloStatsAsync();
            top = await DatabaseGameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildOthelloStatsString());
            emb.AddField("Top players in Othello", top, inline: true);

            topStats = await this.Database.GetTopQuizStatsAsync();
            top = await DatabaseGameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildQuizStatsString());
            emb.AddField("Top players in Quiz", top, inline: true);

            topStats = await this.Database.GetTopTicTacToeStatsAsync();
            top = await DatabaseGameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildTicTacToeStatsString());
            emb.AddField("Top players in TicTacToe", top, inline: true);

            await ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region COMMAND_GAME_RPS
        [Command("rps")]
        [Description("Rock, paper, scissors game against TheGodfather")]
        [Aliases("rockpaperscissors")]
        [UsageExamples("!game rps scissors")]
        public async Task RpsAsync(CommandContext ctx,
                                  [Description("rock/paper/scissors")] string rps)
        {
            if (string.IsNullOrWhiteSpace(rps))
                throw new CommandFailedException("Missing your pick!");

            DiscordEmoji userPick;
            if (string.Compare(rps, "rock", true) == 0 || string.Compare(rps, "r", true) == 0)
                userPick = DiscordEmoji.FromName(ctx.Client, ":new_moon:");
            else if (string.Compare(rps, "paper", true) == 0 || string.Compare(rps, "p", true) == 0)
                userPick = DiscordEmoji.FromName(ctx.Client, ":newspaper:");
            else if (string.Compare(rps, "scissors", true) == 0 || string.Compare(rps, "s", true) == 0)
                userPick = DiscordEmoji.FromName(ctx.Client, ":scissors:");
            else
                throw new CommandFailedException("Invalid pick. Must be rock, paper or scissors.");

            DiscordEmoji gfPick;
            switch (GFRandom.Generator.Next(3)) {
                case 0:
                    gfPick = DiscordEmoji.FromName(ctx.Client, ":new_moon:");
                    break;
                case 1:
                    gfPick = DiscordEmoji.FromName(ctx.Client, ":newspaper:");
                    break;
                default:
                    gfPick = DiscordEmoji.FromName(ctx.Client, ":scissors:");
                    break;
            }
            await this.InformAsync(ctx, StaticDiscordEmoji.Joystick, $"{ctx.User.Mention} {userPick} {gfPick} {ctx.Client.CurrentUser.Mention}");
        }
        #endregion

        #region COMMAND_GAME_STATS
        [Command("stats")]
        [Description("Print game stats for given user.")]
        [Aliases("s", "st")]
        [UsageExamples("!game stats",
                       "!game stats @Someone")]
        public async Task StatsAsync(CommandContext ctx,
                                    [Description("User.")] DiscordUser user = null)
        {
            user = user ?? ctx.User;

            using (DatabaseContext db = this.Database.CreateContext()) {
                DatabaseGameStats stats = await db.GameStats.FindAsync((long)user.Id);
                if (stats is null) {
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                        Title = $"Stats for {user.Username}",
                        Description = "No games played yet!",
                        ThumbnailUrl = user.AvatarUrl,
                        Color = this.ModuleColor
                    }.Build());
                    return;
                } else {
                    await ctx.RespondAsync(embed: stats.ToDiscordEmbed(user));
                }
            }
        }
        #endregion
    }
}
