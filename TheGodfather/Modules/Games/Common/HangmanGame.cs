using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games
{
    public sealed class HangmanGame : BaseChannelGame
    {
        public const int StartingLives = 6;

        private DiscordMessage? msgHandle;
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
            this.lives = StartingLives;
            this.word = word.ToLowerInvariant();
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            this.msgHandle = await this.Channel.LocalizedEmbedAsync(lcs, Emojis.Joystick, null, "str-game-hm-starting");

            await this.UpdateHangmanAsync();

            while (!this.gameOver && this.lives > 0)
                await this.AdvanceAsync();

            if (this.IsTimeoutReached) {
                this.Winner = this.initiator;
                await this.Channel.LocalizedEmbedAsync(lcs, Emojis.X, null, "cmd-err-game-timeout-w", this.Winner.Mention);
                await this.Channel.LocalizedEmbedAsync(lcs, Emojis.Trophy, null, "fmt-game-hm-w", this.word);
                return;
            }

            if (this.lives > 0 && this.Winner is { }) {
                await this.Channel.LocalizedEmbedAsync(lcs, Emojis.X, null, "fmt-winners", this.Winner.Mention);
            } else {
                this.Winner = this.initiator;
                await this.Channel.LocalizedEmbedAsync(lcs, Emojis.Trophy, null, "fmt-game-hm-win", this.Winner.Mention);
                await this.Channel.LocalizedEmbedAsync(lcs, Emojis.Joystick, null, "fmt-game-hm-w", this.word);
            }
        }

        private async Task AdvanceAsync()
        {
            InteractivityResult<DiscordMessage> mctx = await this.Interactivity.WaitForMessageAsync(xm => {
                if (xm.Channel.Id != this.Channel.Id || xm.Author.IsBot) return false;
                if (xm.Author.Id == this.initiator.Id) return false;
                if (xm.Content.ToLowerInvariant() == this.word) return true;
                if (xm.Content.Length != 1 || !char.IsLetter(xm.Content[0])) return false;
                if (!this.guesses.Contains(char.ToLowerInvariant(xm.Content[0]))) return true;
                return false;
            });
            if (mctx.TimedOut) {
                this.gameOver = true;
                this.IsTimeoutReached = true;
                return;
            }

            if (mctx.Result.Content.ToLowerInvariant() == this.word) {
                this.Winner = mctx.Result.Author;
                this.gameOver = true;
            }

            char guess = char.ToLowerInvariant(mctx.Result.Content[0]);
            if (this.word.IndexOf(guess) != -1) {
                for (int i = 0; i < this.word.Length; i++)
                    if (this.word[i] == guess)
                        this.hidden[i] = char.ToUpperInvariant(this.word[i]);
                if (Array.IndexOf(this.hidden, '?') == -1) {
                    this.Winner = mctx.Result.Author;
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
            var emb = new DiscordEmbedBuilder {
                Title = this.hidden.JoinWith(""),
                Description = $@". ┌─────┐
.┃...............┋
.┃...............┋
.┃{(this.lives < 6 ? ".............😲" : "")}
.┃{(this.lives < 5 ? "............./" : "")} {(this.lives < 4 ? "|" : "")} {(this.lives < 3 ? "\\" : "")}
.┃{(this.lives < 2 ? "............../" : "")} {(this.lives < 1 ? "\\" : "")}
/-\"
            };
            emb.WithFooter(string.Join(", ", this.guesses));

            return this.msgHandle.ModifyOrResendAsync(this.Channel, emb.Build());
        }
    }
}


