#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

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
    public partial class CommandsGames
    {
        #region COMMAND_DUEL
        [Command("duel")]
        [Description("Starts a duel which I will commentate.")]
        [Aliases("fight", "vs")]
        public async Task DuelAsync(CommandContext ctx,
                                   [Description("Who to fight with?")] DiscordUser u)
        {
            if (u.Id == ctx.User.Id)
                throw new CommandFailedException("You can't duel yourself...");

            string[] weapons = { ":hammer:", ":dagger:", ":pick:", ":bomb:", ":guitar:", ":fire:" };

            int hp1 = 10, hp2 = 10;
            var rnd = new Random();
            string feed = "";

            var hp1bar = string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":white_large_square:"), hp1)) + string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":black_large_square:"), 10 - hp1));
            var hp2bar = string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":black_large_square:"), 10 - hp2)) + string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":white_large_square:"), hp2));
            var m = await ctx.RespondAsync($"{ctx.User.Mention} {hp1bar} :crossed_swords: {hp2bar} {u.Mention}")
                .ConfigureAwait(false);

            while (hp1 > 0 && hp2 > 0) {
                int damage = rnd.Next(1, 3);
                if (rnd.Next() % 2 == 0) {
                    feed += $"\n{ctx.User.Mention} {weapons[rnd.Next(weapons.Length)]} {u.Mention}"; 
                    hp2 -= damage;
                } else {
                    feed += $"\n{u.Mention} {weapons[rnd.Next(weapons.Length)]} {ctx.User.Mention}";
                    hp1 -= damage;
                }
                hp1bar = string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":white_large_square:"), hp1)) + string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":black_large_square:"), 10 - hp1));
                hp2bar = string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":black_large_square:"), 10 - hp2)) + string.Join("", Enumerable.Repeat(DiscordEmoji.FromName(ctx.Client, ":white_large_square:"), hp2));
                m = await m.ModifyAsync($"{ctx.User.Mention} {hp1bar} :crossed_swords: {hp2bar} {u.Mention}" + feed)
                    .ConfigureAwait(false);

                await Task.Delay(2000)
                    .ConfigureAwait(false);
            }
            if (hp1 <= 0) {
                await ctx.RespondAsync($"{u.Mention} wins!")
                    .ConfigureAwait(false);
            } else {
                await ctx.RespondAsync($"{ctx.User.Mention} wins!")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_GAMES_HANGMAN
        [Command("hangman")]
        [Description("Starts a hangman game.")]
        public async Task HangmanAsync(CommandContext ctx)
        {
            DiscordDmChannel dm;
            try {
                dm = await ctx.Client.CreateDmAsync(ctx.User)
                    .ConfigureAwait(false);
                await dm.SendMessageAsync("What is the secret word?")
                    .ConfigureAwait(false);
            } catch {
                throw new Exception("Please enable direct messages, so I can ask you about the word to guess.");
            }
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm => xm.Channel == dm && xm.Author.Id == ctx.User.Id,
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (msg == null) {
                await ctx.RespondAsync("Ok, nvm...")
                    .ConfigureAwait(false);
                return;
            } else {
                await dm.SendMessageAsync("Alright! The word is: " + Formatter.Bold(msg.Message.Content))
                    .ConfigureAwait(false);
            }

            int lives = 7;
            string word = msg.Message.Content.ToLower();
            char[] guess_str = word.Select(c => (c == ' ') ? ' ' : '?').ToArray();

            await DrawHangmanAsync(ctx, guess_str, lives)
                .ConfigureAwait(false);
            while (lives > 0 && Array.IndexOf(guess_str, '?') != -1) {
                var m = await interactivity.WaitForMessageAsync(
                    xm => xm.Channel == ctx.Channel && !xm.Author.IsBot && xm.Content.Length == 1,
                    TimeSpan.FromMinutes(1)
                ).ConfigureAwait(false);
                if (m == null) {
                    await ctx.RespondAsync("Ok, nvm, aborting game...")
                        .ConfigureAwait(false);
                    return;
                }

                char guess_char = Char.ToLower(m.Message.Content[0]);
                if (word.IndexOf(guess_char) != -1) {
                    for (int i = 0; i < word.Length; i++)
                        if (word[i] == guess_char)
                            guess_str[i] = Char.ToUpper(word[i]);
                } else {
                    lives--;
                }
                await DrawHangmanAsync(ctx, guess_str, lives)
                    .ConfigureAwait(false);
            }
            await ctx.RespondAsync("Game over! The word was : " + Formatter.Bold(word))
                .ConfigureAwait(false);
        }

        #region HELPER_FUNCTIONS
        private async Task DrawHangmanAsync(CommandContext ctx, char[] word, int lives)
        {
            string s = "\n-|-\n";
            if (lives < 7) {
                s += " O\n";
                if (lives < 6) {
                    s += "/";
                    if (lives < 5) {
                        s += "|";
                        if (lives < 4) {
                            s += "\\\n";
                            if (lives < 3) {
                                s += "/";
                                if (lives < 2) {
                                    s += "|";
                                    if (lives < 1) {
                                        s += "\\\n";
                                    }
                                }
                            }
                        }
                    }
                }
            }

            await ctx.RespondAsync("WORD: " + new string(word) + s);
        }
        #endregion
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

        #region COMMAND_GAMES_TICTACTOE
        [Command("tictactoe")]
        [Description("Starts a game of tic-tac-toe.")]
        [Aliases("ttt")]
        public async Task TicTacToeAsync(CommandContext ctx)
        {
            await ctx.RespondAsync($"Who wants to play tic-tac-toe with {ctx.User.Username}?")
                .ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm => (xm.Author.Id != ctx.User.Id) && (xm.Channel.Id == ctx.Channel.Id) &&
                      (xm.Content.ToLower().StartsWith("me") || xm.Content.ToLower().StartsWith("i "))
            ).ConfigureAwait(false);
            if (msg == null) {
                await ctx.RespondAsync($"{ctx.User.Mention} right now: http://i0.kym-cdn.com/entries/icons/mobile/000/003/619/ForeverAlone.jpg")
                    .ConfigureAwait(false);
                return;
            }

            var m = await ctx.RespondAsync($"Game between {ctx.User.Mention} and {msg.User.Mention} begins!")
                .ConfigureAwait(false);

            int[,] board = new int[3, 3];
            TTTInitializeBoard(board);

            bool player1plays = true;
            int moves = 0;
            while (moves < 9 && !TTTGameOver(board)) {
                int field = 0;
                var t = await interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.Channel.Id != ctx.Channel.Id) return false;
                        if (player1plays && (xm.Author.Id != ctx.User.Id)) return false;
                        if (!player1plays && (xm.Author.Id != msg.User.Id)) return false;
                        try {
                            field = int.Parse(xm.Content);
                            if (field < 1 || field > 10)
                                return false;
                        } catch {
                            return false;
                        }
                        return true;
                    },
                    TimeSpan.FromMinutes(1)
                ).ConfigureAwait(false);
                if (field == 0) {
                    await ctx.RespondAsync("No reply, aborting...");
                    return;
                }

                if (TTTPlaySuccessful(player1plays ? 1 : 2, board, field)) {
                    player1plays = !player1plays;
                    await TTTPrintBoard(ctx, board, m)
                        .ConfigureAwait(false);
                } else {
                    await ctx.RespondAsync("Invalid move.")
                        .ConfigureAwait(false);
                }
                moves++;
            }

            await ctx.RespondAsync("GG")
                .ConfigureAwait(false);
        }

        #region HELPER_FUNCTIONS
        private bool TTTGameOver(int[,] board)
        {
            for (int i = 0; i < 3; i++) {
                if (board[i, 0] != 0 && board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2])
                    return true;
                if (board[0, i] != 0 && board[0, i] == board[1, i] && board[1, i] == board[2, i])
                    return true;
            }
            if (board[0, 0] != 0 && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
                return true;
            if (board[0, 2] != 0 && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
                return true;
            return false;
        }

        private void TTTInitializeBoard(int[,] board)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    board[i, j] = 0;
        }

        private bool TTTPlaySuccessful(int v, int[,] board, int index)
        {
            index--;
            if (board[index / 3, index % 3] != 0)
                return false;
            board[index / 3, index % 3] = v;
            return true;
        }

        private async Task TTTPrintBoard(CommandContext ctx, int[,] board, DiscordMessage m)
        {
            string s = "";
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++)
                    switch (board[i, j]) {
                        case 0: s += $"{DiscordEmoji.FromName(ctx.Client, ":white_medium_square:")}"; break;
                        case 1: s += $"{DiscordEmoji.FromName(ctx.Client, ":x:")}"; break;
                        case 2: s += $"{DiscordEmoji.FromName(ctx.Client, ":o:")}"; break;
                    }
                s += '\n';
            }
            await m.ModifyAsync(embed: new DiscordEmbedBuilder() { Description = s });
        }
        #endregion

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
                    .ConfigureAwait(false); ;
            } else {
                await ctx.RespondAsync("ROFL what a nabs...")
                    .ConfigureAwait(false); ;
            }
        }
        #endregion
    }
}
