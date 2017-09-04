#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfatherBot
{
    [Group("games", CanInvokeWithoutSubcommand = false)]
    [Description("Starts a game for you to play!")]
    [Aliases("game", "g")]
    public class CommandsGames
    {
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
                    (xm.Author.Id != ctx.User.Id) &&
                    (xm.Content.ToLower().StartsWith("me") || xm.Content.ToLower().StartsWith("i ")),
                TimeSpan.FromMinutes(1)
            );
            if (msg == null || msg.Content.StartsWith("no")) {
                await ctx.RespondAsync($"{ctx.User.Mention} right now: http://i0.kym-cdn.com/entries/icons/mobile/000/003/619/ForeverAlone.jpg");
                return;
            }

            await ctx.RespondAsync($"Game between {ctx.User.Mention} and {msg.Author.Mention} begins!");

            int[,] board = new int[3, 3];
            TTTInitializeBoard(board);

            bool player1plays = true;
            while (!TTTGameOver(board)) {
                int field = 0;
                var move = await interactivity.WaitForMessageAsync(
                    xm => {
                        if (player1plays && (xm.Author.Id != ctx.User.Id)) return false;
                        if (!player1plays && (xm.Author.Id != msg.Author.Id)) return false;
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
                    await TTTPrintBoard(ctx, board);
                } else
                    await ctx.RespondAsync("Invalid move.");
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

        private async Task TTTPrintBoard(CommandContext ctx, int[,] board)
        {
            string line = "";
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++)
                    switch (board[i, j]) {
                        case 0: line += $"{DiscordEmoji.FromName(ctx.Client, ":white_medium_square:")}"; break;
                        case 1: line += $"{DiscordEmoji.FromName(ctx.Client, ":x:")}"; break;
                        case 2: line += $"{DiscordEmoji.FromName(ctx.Client, ":o:")}"; break;
                    }
                line += '\n';
            }
            var embed = new DiscordEmbed() {
                Description = line
            };
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion

        #endregion

        #region COMMAND_GAMES_QUIZ
        [Command("quiz")]
        [Description("Starts a quiz.")]
        [Aliases("trivia")]
        public async Task Quiz(CommandContext ctx)
        {
            Dictionary<string, string> answers = LoadQuestionsAndAnswers();
            var questions = new List<string>(answers.Keys);
            var participants = new SortedDictionary<string, int>();

            var rnd = new Random();
            for (int i = 0; i < 5; i++) {
                string question = questions[rnd.Next(questions.Count)];
                await ctx.RespondAsync(question);

                var interactivity = ctx.Client.GetInteractivityModule();
                var msg = await interactivity.WaitForMessageAsync(
                    xm => xm.Content.ToLower() == answers[question],
                    TimeSpan.FromSeconds(10)
                );
                if (msg == null) {
                    await ctx.RespondAsync($"Time is out! The correct answer was: {answers[question]}");
                } else {
                    await ctx.RespondAsync($"GG {msg.Author.Mention}, you got it right!");
                    if (participants.ContainsKey(msg.Author.Username))
                        participants[msg.Author.Username]++;
                    else
                        participants.Add(msg.Author.Username, 1);
                }
                questions.Remove(question);
                await Task.Delay(2000);
            }

            var em = new DiscordEmbed() {
                Title = "Results"
            };
            foreach (var participant in participants) {
                var emf = new DiscordEmbedField() {
                    Name = participant.Key,
                    Value = participant.Value.ToString(),
                    Inline = true
                };
                em.Fields.Add(emf);
            }
            await ctx.RespondAsync("", embed: em);
        }

        #region HELPER_FUNCTIONS
        private Dictionary<string, string> LoadQuestionsAndAnswers()
        {
            var qna = new Dictionary<string, string>();
            qna.Add("test 1?", "1");
            qna.Add("test 2?", "2");
            qna.Add("test 3?", "3");
            qna.Add("test 4?", "4");
            qna.Add("test 5?", "5");
            return qna;
        }
        #endregion
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
                await ctx.RespondAsync("Please enable direct messages, so I can ask you about the word to guess.");
                return;
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
                await dm.SendMessageAsync("Alright! The word is: " + msg.Content);
            }
            
            int lives = 7;
            string word = msg.Content.ToLower();
            char[] guess = new char[word.Length];
            for (int i = 0; i < guess.Length; i++)
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

                if (word.IndexOf(m.Content[0]) != -1) {
                    for (int i = 0; i < word.Length; i++)
                        if (word[i] == m.Content[0])
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
    }
}
