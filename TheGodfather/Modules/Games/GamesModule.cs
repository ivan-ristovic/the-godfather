#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Games
{
    [Group("game")]
    [Description("Starts a game for you to play!")]
    [Aliases("games", "gm")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public partial class GamesModule : GodfatherBaseModule
    {

        public GamesModule(DatabaseService db) : base(db: db) { }


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

                await DatabaseService.UpdateUserStatsAsync(duel.Winner.Id, "duels_won")
                    .ConfigureAwait(false);
                await DatabaseService.UpdateUserStatsAsync(duel.Winner.Id == ctx.User.Id ? opponent.Id : ctx.User.Id, "duels_lost")
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

            var dm = await InteractivityUtil.CreateDmChannelAsync(ctx.Client, ctx.User.Id)
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
                    await DatabaseService.UpdateUserStatsAsync(hangman.Winner.Id, "hangman_won")
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
            var em = await DatabaseService.GetStatsLeaderboardAsync(ctx.Client)
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
            await ReplySuccessAsync(ctx, $"{ctx.User.Mention} {usre} {gfe} {ctx.Client.CurrentUser.Mention}", null)
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

            var em = await DatabaseService.GetEmbeddedStatsForUserAsync(user)
                .ConfigureAwait(false);
            await ctx.RespondAsync(embed: em)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GAMES_TICTACTOE
        [Command("tictactoe")]
        [Description("Starts a game of tic-tac-toe. Play by posting a number from 1 to 9 corresponding to field you wish to place your move on.")]
        [Aliases("ttt")]
        [UsageExample("!game tictactoe")]
        public async Task TicTacToeAsync(CommandContext ctx)
        {
            if (Game.RunningInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Another game is already running in the current channel!");

            await ctx.RespondAsync($"Who wants to play tic-tac-toe with {ctx.User.Username}?")
                .ConfigureAwait(false);
            var opponent = await InteractivityUtil.WaitForGameOpponentAsync(ctx)
                .ConfigureAwait(false);
            if (opponent == null)
                return;

            var ttt = new TicTacToe(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent);
            Game.RegisterGameInChannel(ttt, ctx.Channel.Id);
            try {
                await ttt.RunAsync()
                    .ConfigureAwait(false);

                if (ttt.Winner != null) {
                    await ctx.RespondAsync($"The winner is: {ttt.Winner.Mention}!")
                        .ConfigureAwait(false);

                    await DatabaseService.UpdateUserStatsAsync(ttt.Winner.Id, "ttt_won")
                        .ConfigureAwait(false);
                    if (ttt.Winner.Id == ctx.User.Id)
                        await DatabaseService.UpdateUserStatsAsync(opponent.Id, "ttt_lost").ConfigureAwait(false);
                    else
                        await DatabaseService.UpdateUserStatsAsync(ctx.User.Id, "ttt_lost").ConfigureAwait(false);
                } else if (ttt.NoReply == false) {
                    await ctx.RespondAsync("A draw... Pathetic...")
                        .ConfigureAwait(false);
                } else {
                    await ctx.RespondAsync("No reply, aborting TicTacToe game...")
                        .ConfigureAwait(false);
                }
            } finally {
                Game.UnregisterGameInChannel(ctx.Channel.Id);
            }
        }
        #endregion

        #region COMMAND_GAMES_TYPING
        [Command("typing")]
        [Description("Typing race.")]
        [Aliases("type", "typerace", "typingrace")]
        public async Task TypingRaceAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("I will send a random string in 5s. First one to types it wins. FOCUS!")
                .ConfigureAwait(false);
            await Task.Delay(5000)
                .ConfigureAwait(false);

            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var rnd = new Random();
            var msg = new string(Enumerable.Repeat(chars, 20).Select(s => s[rnd.Next(s.Length)]).ToArray());
            await ctx.RespondAsync(Formatter.Bold(msg) + " (you have 60s)")
                .ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();
            var response = await interactivity.WaitForMessageAsync(
                m => m.ChannelId == ctx.Channel.Id && m.Content == msg,
                TimeSpan.FromSeconds(60)
            ).ConfigureAwait(false);

            if (response != null) {
                await ctx.RespondAsync($"And the winner is {response.User.Mention}!")
                    .ConfigureAwait(false);
            } else {
                await ctx.RespondAsync("ROFL what a nabs...")
                    .ConfigureAwait(false); ;
            }
        }
        #endregion
    }
}
