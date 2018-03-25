#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games
{
    [Group("game")]
    [Description("Starts a game for you to play!")]
    [Aliases("games", "gm")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public partial class GamesModule : TheGodfatherBaseModule
    {

        public GamesModule(DBService db) : base(db: db) { }
        

        #region COMMAND_GAME_LEADERBOARD
        [Command("leaderboard")]
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
        [Command("rps")]
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
            switch (new Random().Next(0, 3)) {
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
        [Command("stats")]
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

        #region COMMAND_GAME_TYPING
        [Command("typingrace")]
        [Description("Typing race.")]
        [Aliases("type", "typerace", "typing")]
        [UsageExample("!game typingrace")]
        public async Task TypingRaceAsync(CommandContext ctx)
        {
            if (Game.RunningInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Another game is already running in the current channel!");

            var game = new TypingRace(ctx.Client.GetInteractivity(), ctx.Channel);
            Game.RegisterGameInChannel(game, ctx.Channel.Id);
            try {
                await ctx.RespondWithIconEmbedAsync("I will send a text in 5s. First one to types it wins. FOCUS!", ":clock1:")
                    .ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(5))
                    .ConfigureAwait(false);

                await game.RunAsync()
                    .ConfigureAwait(false);

                if (game.NoReply == true) {
                    await ctx.RespondWithIconEmbedAsync("ROFL what a nabs...", ":alarm_clock:")
                        .ConfigureAwait(false);
                } else {
                    await ctx.RespondWithIconEmbedAsync(EmojiUtil.Trophy, $"The winner is {game.Winner?.Mention ?? "<unknown>"}!")
                        .ConfigureAwait(false);
                }
            } finally {
                Game.UnregisterGameInChannel(ctx.Channel.Id);
            }
        }
        #endregion
    }
}
