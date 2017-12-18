#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Exceptions;
using TheGodfather.Helpers.DataManagers;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Games
{
    [Group("games", CanInvokeWithoutSubcommand = false)]
    [Description("Starts a game for you to play!")]
    [Aliases("game", "gm")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public partial class CommandsGames
    {
        #region COMMAND_GAMES_CARO
        [Command("caro")]
        [Description("Starts a caro game.")]
        [Aliases("c")]
        public async Task CaroAsync(CommandContext ctx)
        {
            if (Caro.GameExistsInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Caro game is already running in the current channel!");

            await ctx.RespondAsync($"Who wants to play caro with {ctx.User.Username}?")
                .ConfigureAwait(false);

            var msg = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                xm => CheckIfValidOpponent(xm, ctx.User.Id, ctx.Channel.Id)
            ).ConfigureAwait(false);
            if (msg == null) {
                await ctx.RespondAsync($"{ctx.User.Mention} right now: http://i0.kym-cdn.com/entries/icons/mobile/000/003/619/ForeverAlone.jpg")
                    .ConfigureAwait(false);
                return;
            }

            var caro = new Caro(ctx.Client, ctx.Channel.Id, ctx.User, msg.User);
            await caro.PlayAsync()
                .ConfigureAwait(false);

            if (caro.Winner != null) {
                await ctx.RespondAsync($"The winner is: {caro.Winner.Mention}!")
                    .ConfigureAwait(false);

                var gsm = ctx.Dependencies.GetDependency<GameStatsManager>();
                await gsm.UpdateStatAsync(caro.Winner.Id, "caro_won").ConfigureAwait(false);
                if (caro.Winner.Id == ctx.User.Id)
                    await gsm.UpdateStatAsync(msg.User.Id, "caro_lost").ConfigureAwait(false);
                else
                    await gsm.UpdateStatAsync(ctx.User.Id, "caro_lost").ConfigureAwait(false);
            } else if (caro.NoReply == false) {
                await ctx.RespondAsync("A draw... Pathetic...")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_GAMES_CONNECTFOUR
        [Command("connectfour")]
        [Description("Starts a \"Connect4\" game. Play by posting a number from 1 to 9 corresponding to the column you wish to place your move on.")]
        [Aliases("connect4", "chain4", "chainfour", "c4")]
        public async Task Connect4Async(CommandContext ctx)
        {
            if (TicTacToe.GameExistsInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Connect4 game is already running in the current channel!");

            await ctx.RespondAsync($"Who wants to play Connect4 with {ctx.User.Username}?")
                .ConfigureAwait(false);

            var msg = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                xm => CheckIfValidOpponent(xm, ctx.User.Id, ctx.Channel.Id)
            ).ConfigureAwait(false);
            if (msg == null) {
                await ctx.RespondAsync($"{ctx.User.Mention} right now: http://i0.kym-cdn.com/entries/icons/mobile/000/003/619/ForeverAlone.jpg")
                    .ConfigureAwait(false);
                return;
            }

            var c4 = new Connect4(ctx.Client, ctx.Channel.Id, ctx.User, msg.User);
            await c4.PlayAsync()
                .ConfigureAwait(false);

            if (c4.Winner != null) {
                await ctx.RespondAsync($"The winner is: {c4.Winner.Mention}!")
                    .ConfigureAwait(false);

                var gsm = ctx.Dependencies.GetDependency<GameStatsManager>();
                await gsm.UpdateStatAsync(c4.Winner.Id, "chain4_won").ConfigureAwait(false);
                if (c4.Winner.Id == ctx.User.Id)
                    await gsm.UpdateStatAsync(msg.User.Id, "chain4_lost").ConfigureAwait(false);
                else
                    await gsm.UpdateStatAsync(ctx.User.Id, "chain4_lost").ConfigureAwait(false);
            } else if (c4.NoReply == false) {
                await ctx.RespondAsync("A draw... Pathetic...")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_GAMES_DUEL
        [Command("duel")]
        [Description("Starts a duel which I will commentate.")]
        [Aliases("fight", "vs", "d")]
        public async Task DuelAsync(CommandContext ctx,
                                   [Description("Who to fight with?")] DiscordUser u)
        {
            if (u.Id == ctx.User.Id)
                throw new CommandFailedException("You can't duel yourself...");
            if (Duel.GameExistsInChannel(ctx.Channel.Id))
                throw new CommandFailedException("A duel is already running in the current channel!");

            if (u.Id == ctx.Client.CurrentUser.Id) {
                await ctx.RespondAsync(
                    $"{ctx.User.Mention} {string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":black_large_square:"), 5))} :crossed_swords: " +
                    $"{string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":white_large_square:"), 5))} {u.Mention}" +
                    $"\n{ctx.Client.CurrentUser.Mention} {DiscordEmoji.FromName(ctx.Client, ":zap:")} {ctx.User.Mention}"
                ).ConfigureAwait(false);
                await ctx.RespondAsync($"{ctx.Client.CurrentUser.Mention} wins!")
                    .ConfigureAwait(false);
                return;
            }

            var duel = new Duel(ctx.Client, ctx.Channel.Id, ctx.User, u);
            await duel.PlayAsync()
                .ConfigureAwait(false);

            await ctx.RespondAsync($"{duel.Winner.Username} {(string.IsNullOrWhiteSpace(duel.FinishingMove) ? "wins" : duel.FinishingMove)}!")
                .ConfigureAwait(false);

            var gsm = ctx.Dependencies.GetDependency<GameStatsManager>();
            await gsm.UpdateStatAsync(duel.Winner.Id, "duels_won")
                .ConfigureAwait(false);
            await gsm.UpdateStatAsync(duel.Winner.Id == ctx.User.Id ? u.Id : ctx.User.Id, "duels_lost")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GAMES_HANGMAN
        [Command("hangman")]
        [Description("Starts a hangman game.")]
        [Aliases("h", "hang")]
        public async Task HangmanAsync(CommandContext ctx)
        {
            if (Hangman.GameExistsInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Hangman game is already running in the current channel!");

            DiscordDmChannel dm;
            try {
                dm = await ctx.Client.CreateDmAsync(ctx.User)
                    .ConfigureAwait(false);
                await dm.SendMessageAsync("What is the secret word?")
                    .ConfigureAwait(false);
                await ctx.RespondAsync(ctx.User.Mention + ", check your DM. When you give me the word, the game will start.");
            } catch {
                throw new CommandFailedException("Please enable direct messages, so I can ask you about the word to guess.");
            }
            var interactivity = ctx.Client.GetInteractivityModule();
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

            var hangman = new Hangman(ctx.Client, ctx.Channel.Id, msg.Message.Content);
            await hangman.PlayAsync()
                .ConfigureAwait(false);
            if (hangman.Winner != null)
                await ctx.Dependencies.GetDependency<GameStatsManager>().UpdateStatAsync(hangman.Winner.Id, "hangman_won")
                    .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GAMES_LEADERBOARD
        [Command("leaderboard")]
        [Description("Starts a hangman game.")]
        [Aliases("globalstats")]
        public async Task LeaderboardAsync(CommandContext ctx)
        {
            var em = await ctx.Dependencies.GetDependency<GameStatsManager>().GetLeaderboardAsync(ctx.Client)
                .ConfigureAwait(false);
            await ctx.RespondAsync(embed: em)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GAMES_RPS
        [Command("rps")]
        [Description("Rock, paper, scissors game.")]
        [Aliases("rockpaperscissors")]
        public async Task RpsAsync(CommandContext ctx)
        {
            var msg = await ctx.RespondAsync("Get ready!")
                .ConfigureAwait(false);
            for (int i = 3; i > 0; i--) {
                await msg.ModifyAsync(i + "...")
                    .ConfigureAwait(false);
                await Task.Delay(1000)
                    .ConfigureAwait(false);
            }
            await msg.ModifyAsync("GO!")
                .ConfigureAwait(false);

            switch (new Random().Next(0, 3)) {
                case 0:
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":new_moon:")}")
                        .ConfigureAwait(false);
                    break;
                case 1:
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":newspaper:")}")
                        .ConfigureAwait(false);
                    break;
                case 2:
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":scissors:")}")
                        .ConfigureAwait(false);
                    break;
            }
        }
        #endregion

        #region COMMAND_GAMES_STATS
        [Command("stats")]
        [Description("Print game stats for given user.")]
        public async Task StatsAsync(CommandContext ctx,
                                    [Description("User.")] DiscordUser u = null)
        {
            if (u == null)
                u = ctx.User;

            var e = await ctx.Dependencies.GetDependency<GameStatsManager>().GetEmbeddedStatsForUserAsync(u)
                .ConfigureAwait(false);
            await ctx.RespondAsync(embed: e)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GAMES_TICTACTOE
        [Command("tictactoe")]
        [Description("Starts a game of tic-tac-toe. Play by posting a number from 1 to 9 corresponding to field you wish to place your move on.")]
        [Aliases("ttt")]
        public async Task TicTacToeAsync(CommandContext ctx)
        {
            if (TicTacToe.GameExistsInChannel(ctx.Channel.Id))
                throw new CommandFailedException("TicTacToe game is already running in the current channel!");

            await ctx.RespondAsync($"Who wants to play tic-tac-toe with {ctx.User.Username}?")
                .ConfigureAwait(false);
            
            var msg = await ctx.Client.GetInteractivityModule().WaitForMessageAsync(
                xm => CheckIfValidOpponent(xm, ctx.User.Id, ctx.Channel.Id)
            ).ConfigureAwait(false);
            if (msg == null) {
                await ctx.RespondAsync($"{ctx.User.Mention} right now: http://i0.kym-cdn.com/entries/icons/mobile/000/003/619/ForeverAlone.jpg")
                    .ConfigureAwait(false);
                return;
            }

            var ttt = new TicTacToe(ctx.Client, ctx.Channel.Id, ctx.User, msg.User);
            await ttt.PlayAsync()
                .ConfigureAwait(false);

            if (ttt.Winner != null) {
                var gsm = ctx.Dependencies.GetDependency<GameStatsManager>();
                await ctx.RespondAsync($"The winner is: {ttt.Winner.Mention}!")
                    .ConfigureAwait(false);
                await gsm.UpdateStatAsync(ttt.Winner.Id, "ttt_won").ConfigureAwait(false);
                if (ttt.Winner.Id == ctx.User.Id)
                    await gsm.UpdateStatAsync(msg.User.Id, "ttt_lost").ConfigureAwait(false);
                else
                    await gsm.UpdateStatAsync(ctx.User.Id, "ttt_lost").ConfigureAwait(false);
            } else if (ttt.NoReply == false) {
                await ctx.RespondAsync("A draw... Pathetic...")
                    .ConfigureAwait(false);
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

            var interactivity = ctx.Client.GetInteractivityModule();
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


        #region HELPER_COMMANDS
        private bool CheckIfValidOpponent(DiscordMessage m, ulong uid, ulong cid)
        {
            if (m.Author.Id == uid || m.Channel.Id != cid)
                return false;

            var word = m.Content.ToLower().Split(' ')[0];
            return word == "me" || word == "i";
        }
        #endregion
    }
}
