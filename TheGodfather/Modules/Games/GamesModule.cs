#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Games
{
    [Group("game"), Module(ModuleType.Games)]
    [Description("Starts a game for you to play!")]
    [Aliases("games", "gm")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public partial class GamesModule : TheGodfatherBaseModule
    {

        public GamesModule(DBService db) : base(db: db) { }
        

        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
        {
            return ctx.RespondWithIconEmbedAsync(
                Formatter.Bold("Games:\n\n") +
                "animalrace, caro, connect4, duel, hangman, leaderboard, numberrace, othello, quiz, rps, russianroulette, stats, tictactoe, typingrace"
            );
        }


        #region COMMAND_GAME_LEADERBOARD
        [Command("leaderboard"), Module(ModuleType.Games)]
        [Description("View the global game leaderboard.")]
        [Aliases("globalstats")]
        [UsageExample("!game leaderboard")]
        public async Task LeaderboardAsync(CommandContext ctx)
        {
            var em = await Database.GetStatsLeaderboardAsync(ctx.Client)
                .ConfigureAwait(false);
            await ctx.RespondAsync(embed: em)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GAME_RPS
        [Command("rps"), Module(ModuleType.Games)]
        [Description("Rock, paper, scissors game against TheGodfather")]
        [Aliases("rockpaperscissors")]
        [UsageExample("!game rps scissors")]
        public async Task RpsAsync(CommandContext ctx,
                                  [Description("rock/paper/scissors")] string rps)
        {
            if (string.IsNullOrWhiteSpace(rps))
                throw new CommandFailedException("Missing your pick!");

            DiscordEmoji usre;
            if (string.Compare(rps, "rock", true) == 0 || string.Compare(rps, "r", true) == 0)
                usre = DiscordEmoji.FromName(ctx.Client, ":new_moon:");
            else if (string.Compare(rps, "paper", true) == 0 || string.Compare(rps, "p", true) == 0)
                usre = DiscordEmoji.FromName(ctx.Client, ":newspaper:");
            else if (string.Compare(rps, "scissors", true) == 0 || string.Compare(rps, "s", true) == 0)
                usre = DiscordEmoji.FromName(ctx.Client, ":scissors:");
            else
                throw new CommandFailedException("Invalid pick. Must be rock, paper or scissors.");

            DiscordEmoji gfe;
            switch (GFRandom.Generator.Next(3)) {
                case 0:
                    gfe = DiscordEmoji.FromName(ctx.Client, ":new_moon:");
                    break;
                case 1:
                    gfe = DiscordEmoji.FromName(ctx.Client, ":newspaper:");
                    break;
                default:
                    gfe = DiscordEmoji.FromName(ctx.Client, ":scissors:");
                    break;
            }
            await ctx.RespondWithIconEmbedAsync($"{ctx.User.Mention} {usre} {gfe} {ctx.Client.CurrentUser.Mention}", null)
                 .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GAME_STATS
        [Command("stats"), Module(ModuleType.Games)]
        [Description("Print game stats for given user.")]
        [Aliases("s", "st")]
        [UsageExample("!game stats")]
        [UsageExample("!game stats @Someone")]
        public async Task StatsAsync(CommandContext ctx,
                                    [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            var em = await Database.GetEmbeddedStatsForUserAsync(user)
                .ConfigureAwait(false);
            await ctx.RespondAsync(embed: em)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
