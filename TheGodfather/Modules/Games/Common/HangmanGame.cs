#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Games
{
    public class HangmanGame : ChannelEvent
    {
        private DiscordMessage msgHandle;
        private int lives;
        private bool gameOver;
        private readonly DiscordUser initiator;
        private readonly string word;
        private readonly SortedSet<char> guesses;
        private readonly char[] hidden;


        public HangmanGame(InteractivityExtension interactivity, DiscordChannel channel, string word, DiscordUser initiator)
            : base(interactivity, channel)
        {
            this.gameOver = false;
            this.guesses = new SortedSet<char>();
            this.hidden = new string('?', word.Length).ToCharArray();
            this.initiator = initiator;
            this.lives = 6;
            this.word = word.ToLowerInvariant();
        }


        public override async Task RunAsync()
        {
            this.msgHandle = await this.Channel.EmbedAsync("Game starts!", StaticDiscordEmoji.Joystick);

            await this.UpdateHangmanAsync();

            while (!this.gameOver && this.lives > 0)
                await this.AdvanceAsync();

            if (this.IsTimeoutReached) {
                this.Winner = null;
                await this.Channel.EmbedAsync($"Nobody replies so I am stopping the game... The word was: {Formatter.Bold(this.word)}", StaticDiscordEmoji.Joystick);
                return;
            }

            if (this.lives > 0) {
                await this.Channel.EmbedAsync($"{this.Winner.Mention} won the game!", StaticDiscordEmoji.Joystick);
            } else {
                this.Winner = this.initiator;
                await this.Channel.EmbedAsync($"Nobody guessed the word so {this.Winner.Mention} won the game! The word was: {Formatter.Bold(this.word)}", StaticDiscordEmoji.Joystick);
            }
        }

        private async Task AdvanceAsync()
        {
            MessageContext mctx = await this.Interactivity.WaitForMessageAsync(xm => {
                if (xm.Channel.Id != this.Channel.Id || xm.Author.IsBot) return false;
                if (xm.Author.Id == this.initiator.Id) return false;
                if (xm.Content.ToLowerInvariant() == this.word) return true;
                if (xm.Content.Length != 1 || !Char.IsLetterOrDigit(xm.Content[0])) return false;
                if (!this.guesses.Contains(Char.ToLowerInvariant(xm.Content[0]))) return true;
                return false;
            });
            if (mctx == null) {
                this.gameOver = true;
                this.IsTimeoutReached = true;
                return;
            }

            if (mctx.Message.Content.ToLowerInvariant() == this.word) {
                this.Winner = mctx.User;
                this.gameOver = true;
            }

            char guess = Char.ToLowerInvariant(mctx.Message.Content[0]);
            if (this.word.IndexOf(guess) != -1) {
                for (int i = 0; i < this.word.Length; i++)
                    if (this.word[i] == guess)
                        this.hidden[i] = Char.ToUpper(this.word[i]);
                if (Array.IndexOf(this.hidden, '?') == -1) {
                    this.Winner = mctx.User;
                    this.gameOver = true;
                }
            } else {
                this.lives--;
                this.guesses.Add(guess);
            }

            await this.UpdateHangmanAsync();
        }

        private Task UpdateHangmanAsync()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = string.Join(" ", this.hidden),
                Description = $@". ┌─────┐
.┃...............┋
.┃...............┋
.┃{(this.lives < 6 ? ".............😲" : "")}
.┃{(this.lives < 5 ? "............./" : "")} {(this.lives < 4 ? "|" : "")} {(this.lives < 3 ? "\\" : "")}
.┃{(this.lives < 2 ? "............../" : "")} {(this.lives < 1 ? "\\" : "")}
/-\"
            };
            emb.WithFooter(string.Join(", ", this.guesses));

            return this.msgHandle.ModifyAsync(embed: emb.Build());
        }
    }
}


