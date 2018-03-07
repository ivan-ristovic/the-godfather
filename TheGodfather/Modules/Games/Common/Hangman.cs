#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games
{
    public class Hangman : Game
    {
        #region PRIVATE_FIELDS
        private DiscordMessage _msg;
        private string _word;
        private char[] _hidden;
        private int _lives = 6;
        private bool _gameOver = false;
        private SortedSet<char> _badguesses = new SortedSet<char>();
        #endregion


        public Hangman(InteractivityExtension interactivity, DiscordChannel channel, string word)
            : base(interactivity, channel)
        {
            _word = word.ToLower();
            _hidden = new string('?', _word.Length).ToCharArray();
        }


        public override async Task RunAsync()
        {
            _msg = await _channel.SendIconEmbedAsync("Game starts!", EmojiUtil.Joystick)
                .ConfigureAwait(false);

            await UpdateHangmanAsync()
                .ConfigureAwait(false);
            while (!_gameOver && _lives > 0) {
                await AdvanceAsync()
                    .ConfigureAwait(false);
            }

            await _channel.SendIconEmbedAsync($"Game over! The word was: {Formatter.Bold(_word)}", EmojiUtil.Joystick)
                .ConfigureAwait(false);
        }

        private async Task AdvanceAsync()
        {
            var mctx = await _interactivity.WaitForMessageAsync(
                    xm => (xm.Channel.Id == _channel.Id && 
                          !xm.Author.IsBot && 
                          xm.Content.Length == 1 && 
                          Char.IsLetterOrDigit(xm.Content[0]) &&
                          !_badguesses.Contains(Char.ToLowerInvariant(xm.Content[0]))) || xm.Content.ToLowerInvariant() == _word
                    , TimeSpan.FromMinutes(1)
                ).ConfigureAwait(false);
            if (mctx == null) {
                _gameOver = true;
                return;
            }

            if (mctx.Message.Content.ToLower() == _word) {
                Winner = mctx.User;
                _gameOver = true;
            }

            char guess_char = Char.ToLowerInvariant(mctx.Message.Content[0]);
            if (_word.IndexOf(guess_char) != -1) {
                for (int i = 0; i < _word.Length; i++)
                    if (_word[i] == guess_char)
                        _hidden[i] = Char.ToUpper(_word[i]);
                if (Array.IndexOf(_hidden, '?') == -1) {
                    Winner = mctx.User;
                    _gameOver = true;
                }
            } else {
                _lives--;
                _badguesses.Add(guess_char);
            }
            await UpdateHangmanAsync()
                .ConfigureAwait(false);
        }

        private async Task UpdateHangmanAsync()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = string.Join(" ", _hidden),
                Description = $@". ┌─────┐
.┃...............┋
.┃...............┋
.┃{(_lives < 6 ? ".............😲" : "")}
.┃{(_lives < 5 ? "............./" : "")} {(_lives < 4 ? "|" : "")} {(_lives < 3 ? "\\" : "")}
.┃{(_lives < 2 ? "............../" : "")} {(_lives < 1 ? "\\" : "")}
/-\"
            };
            emb.WithFooter(string.Join(", ", _badguesses));

            await _msg.ModifyAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
    }
}


