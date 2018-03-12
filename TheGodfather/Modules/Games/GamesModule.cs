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


        #region COMMAND_GAMES_DUEL
        [Command("duel")]
        [Description("Starts a duel which I will commentate.")]
        [Aliases("fight", "vs", "d")]
        [UsageExample("!game duel @Someone")]
        public async Task DuelAsync(CommandContext ctx,
                                   [Description("Who to fight with?")] DiscordUser opponent)
        {
            if (Game.RunningInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Another game is already running in the current channel!");

            if (opponent.Id == ctx.User.Id)
                throw new CommandFailedException("You can't duel yourself...");

            if (opponent.Id == ctx.Client.CurrentUser.Id) {
                await ctx.RespondAsync(
                    $"{ctx.User.Mention} {string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":black_large_square:"), 5))} :crossed_swords: " +
                    $"{string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":white_large_square:"), 5))} {opponent.Mention}" +
                    $"\n{ctx.Client.CurrentUser.Mention} {DiscordEmoji.FromName(ctx.Client, ":zap:")} {ctx.User.Mention}"
                ).ConfigureAwait(false);
                await ctx.RespondAsync($"{ctx.Client.CurrentUser.Mention} wins!")
                    .ConfigureAwait(false);
                return;
            }

            var duel = new Duel(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent);
            Game.RegisterGameInChannel(duel, ctx.Channel.Id);

            try {
                await duel.RunAsync()
                    .ConfigureAwait(false);

                await ctx.RespondAsync($"{duel.Winner.Username} {duel.FinishingMove ?? "wins"}!")
                    .ConfigureAwait(false);

                await Database.UpdateUserStatsAsync(duel.Winner.Id, "duels_won")
                    .ConfigureAwait(false);
                await Database.UpdateUserStatsAsync(duel.Winner.Id == ctx.User.Id ? opponent.Id : ctx.User.Id, "duels_lost")
                    .ConfigureAwait(false);
            } finally {
                Game.UnregisterGameInChannel(ctx.Channel.Id);
            }
        }
        #endregion

        #region COMMAND_GAMES_HANGMAN
        [Command("hangman")]
        [Description("Starts a hangman game.")]
        [Aliases("h", "hang")]
        [UsageExample("!game hangman")]
        public async Task HangmanAsync(CommandContext ctx)
        {
            if (Game.RunningInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Another game is already running in the current channel!");

            var dm = await ctx.Client.CreateDmChannelAsync(ctx.User.Id)
                .ConfigureAwait(false);
            if (dm == null)
                throw new CommandFailedException("Please enable direct messages, so I can ask you about the word to guess.");
            await dm.SendMessageAsync("What is the secret word?")
                .ConfigureAwait(false);
            await ctx.RespondAsync(ctx.User.Mention + ", check your DM. When you give me the word, the game will start.")
                .ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();
            var msg = await interactivity.WaitForMessageAsync(
                xm => xm.Channel == dm && xm.Author.Id == ctx.User.Id,
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (msg == null) {
                await ctx.RespondAsync("I didn't get the word, so I will abort the game.")
                    .ConfigureAwait(false);
                return;
            } else {
                await dm.SendMessageAsync("Alright! The word is: " + Formatter.Bold(msg.Message.Content))
                    .ConfigureAwait(false);
            }

            var hangman = new Hangman(ctx.Client.GetInteractivity(), ctx.Channel, msg.Message.Content);
            Game.RegisterGameInChannel(hangman, ctx.Channel.Id);
            try {
                await hangman.RunAsync()
                    .ConfigureAwait(false);
                if (hangman.Winner != null)
                    await Database.UpdateUserStatsAsync(hangman.Winner.Id, "hangman_won")
                        .ConfigureAwait(false);
            } finally {
                Game.UnregisterGameInChannel(ctx.Channel.Id);
            }
        }
        #endregion

        #region COMMAND_GAMES_LEADERBOARD
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

        #region COMMAND_GAMES_RPS
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

        #region COMMAND_GAMES_STATS
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

        #region COMMAND_GAMES_TYPING
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
                    await ctx.RespondWithIconEmbedAsync($"The winner is {game.Winner?.Mention ?? "<unknown>"}!", ":trophy:")
                        .ConfigureAwait(false);
                }
            } finally {
                Game.UnregisterGameInChannel(ctx.Channel.Id);
            }
        }
        #endregion
    }
}
