#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Modules.Games
{
    [Group("games", CanInvokeWithoutSubcommand = false)]
    [Description("Starts a game for you to play!")]
    [Aliases("game", "g")]
    public class CommandsGames
    {
        #region COMMAND_DUEL
        [Command("duel")]
        [Description("Starts a duel which I will commentate.")]
        [Aliases("fight", "vs")]
        public async Task Duel(CommandContext ctx, [Description("Who to fight")] DiscordUser u)
        {
            if (u.Id == ctx.User.Id)
                throw new ArgumentException("You can't duel yourself...");

            string[] weapons = { "sword", "axe", "keyboard", "stone", "cheeseburger", "belt from yo momma" };

            var m = await ctx.RespondAsync($"{ctx.User.Mention} VS {u.Mention}");

            int hp1 = 100, hp2 = 100;
            var rnd = new Random();
            while (hp1 > 0 && hp2 > 0) {
                int damage = rnd.Next(20, 40);
                if (rnd.Next() % 2 == 0) {
                    m = await m.ModifyAsync(m.Content + $"\n**{ctx.User.Username}** ({hp1}) hits **{u.Username}** ({hp2}) with a {weapons[rnd.Next(0, weapons.Length)]} for **{damage}** damage!");
                    hp2 -= damage;
                } else {
                    m = await m.ModifyAsync(m.Content + $"\n**{u.Username}** ({hp2}) hits **{ctx.User.Username}** ({hp1}) with a {weapons[rnd.Next(0, weapons.Length)]} for **{damage}** damage!");
                    hp1 -= damage;
                }
                await Task.Delay(2000);
            }
            if (hp1 < 0)
                await ctx.RespondAsync($"{u.Mention} wins!");
            else
                await ctx.RespondAsync($"{ctx.User.Mention} wins!");
        }
        #endregion

        #region COMMAND_GAMES_HANGMAN
        [Command("hangman")]
        [Description("Starts a hangman game.")]
        public async Task Hangman(CommandContext ctx)
        {
            DiscordDmChannel dm;
            try {
                dm = await ctx.Client.CreateDmAsync(ctx.User);
                await dm.SendMessageAsync("What is the secret word?");
            } catch {
                throw new Exception("Please enable direct messages, so I can ask you about the word to guess.");
            }
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm => xm.Channel == dm && xm.Author.Id == ctx.User.Id,
                TimeSpan.FromMinutes(1)
            );
            if (msg == null) {
                await ctx.RespondAsync("Ok, nvm...");
                return;
            } else {
                await dm.SendMessageAsync("Alright! The word is: " + msg.Message.Content);
            }

            int lives = 7;
            string word = msg.Message.Content.ToLower();
            char[] guess = new char[word.Length];
            for (int i = 0; i < guess.Length; i++)
                if (word[i] == ' ')
                    guess[i] = ' ';
                else
                    guess[i] = '?';

            await DrawHangman(ctx, guess, lives);
            while (lives > 0 && Array.IndexOf(guess, '?') != -1) {
                var m = await interactivity.WaitForMessageAsync(
                    xm => xm.Channel == ctx.Channel && !xm.Author.IsBot && xm.Content.Length == 1,
                    TimeSpan.FromMinutes(1)
                );
                if (m == null) {
                    await ctx.RespondAsync("Ok, nvm, aborting game...");
                    return;
                }

                char guess_char = Char.ToLower(m.Message.Content[0]);
                if (word.IndexOf(guess_char) != -1) {
                    for (int i = 0; i < word.Length; i++)
                        if (word[i] == guess_char)
                            guess[i] = Char.ToUpper(word[i]);
                } else {
                    lives--;
                }
                await DrawHangman(ctx, guess, lives);
            }
            await ctx.RespondAsync("Game over! The word was : " + word);
        }

        #region HELPER_FUNCTIONS
        private async Task DrawHangman(CommandContext ctx, char[] word, int lives)
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
        public async Task RPS(CommandContext ctx)
        {
            await ctx.RespondAsync("Get ready!");
            for (int i = 3; i > 0; i--) {
                await ctx.RespondAsync(i + "...");
                await Task.Delay(1000);
            }

            var rnd = new Random();
            switch (rnd.Next(0, 3)) {
                case 0: await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":new_moon:")}"); break;
                case 1: await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":newspaper:")}"); break;
                case 2: await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":scissors:")}"); break;
            }
        }
        #endregion

        #region COMMAND_GAMES_TICTACTOE
        [Command("tictactoe")]
        [Description("Starts a game of tic-tac-toe.")]
        [Aliases("ttt")]
        public async Task TicTacToe(CommandContext ctx)
        {
            await ctx.RespondAsync($"Who wants to play tic-tac-toe with {ctx.User.Username}?");

            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm =>
                    (xm.Author.Id != ctx.User.Id) && (xm.Channel.Id == ctx.Channel.Id) &&
                    (xm.Content.ToLower().StartsWith("me") || xm.Content.ToLower().StartsWith("i ")),
                TimeSpan.FromMinutes(1)
            );
            if (msg == null || msg.Message.Content.StartsWith("no")) {
                await ctx.RespondAsync($"{ctx.User.Mention} right now: http://i0.kym-cdn.com/entries/icons/mobile/000/003/619/ForeverAlone.jpg");
                return;
            }

            var m = await ctx.RespondAsync($"Game between {ctx.User.Mention} and {msg.User.Mention} begins!");

            int[,] board = new int[3, 3];
            TTTInitializeBoard(board);

            bool player1plays = true;
            int moves = 0;
            while (moves < 9 && !TTTGameOver(board)) {
                int field = 0;
                var move = await interactivity.WaitForMessageAsync(
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
                );
                if (move == null || field == 0) {
                    await ctx.RespondAsync("No reply, aborting...");
                    return;
                }

                if (TTTPlaySuccessful(player1plays ? 1 : 2, board, field)) {
                    player1plays = !player1plays;
                    await TTTPrintBoard(ctx, board, m);
                } else
                    await ctx.RespondAsync("Invalid move.");
                moves++;
            }

            await ctx.RespondAsync("GG");
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
            await m.ModifyAsync("", embed: new DiscordEmbedBuilder() { Description = s } );
        }
        #endregion

        #endregion
    }
}
