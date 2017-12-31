#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Helpers.Collections;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
#endregion

namespace TheGodfather.Modules.Games
{
    public class Hangman
    {
        #region STATIC_FIELDS
        public static bool GameExistsInChannel(ulong cid) => _channels.Contains(cid);
        private static ConcurrentHashSet<ulong> _channels = new ConcurrentHashSet<ulong>();
        #endregion

        #region PUBLIC_FIELDS
        public DiscordUser Winner { get; private set; }
        #endregion

        #region PRIVATE_FIELDS
        private DiscordClient _client;
        private ulong _cid;
        private string _word;
        private char[] _hidden;
        private DiscordMessage _msg;
        private int _lives = 6;
        private bool _gameOver = false;
        private List<char> _badguesses = new List<char>();
        #endregion


        public Hangman(DiscordClient client, ulong cid, string word)
        {
            _client = client;
            _cid = cid;
            _word = word.ToLower();
            _hidden = word.Select(c => (c == ' ') ? '.' : '?').ToArray();
        }
        

        public async Task PlayAsync()
        {
            _channels.Add(_cid);
            var channel = await _client.GetChannelAsync(_cid)
                .ConfigureAwait(false);

            _msg = await channel.SendMessageAsync("Game starts!")
                .ConfigureAwait(false);

            await UpdateHangmanAsync()
                .ConfigureAwait(false);
            while (!_gameOver && _lives > 0)
                await AdvanceAsync().ConfigureAwait(false);

            await channel.SendMessageAsync("Game over! The word was: " + Formatter.Bold(_word))
                .ConfigureAwait(false);

            _channels.TryRemove(_cid);
        }

        private async Task AdvanceAsync()
        {
            var interactivity = _client.GetInteractivityModule();
            var m = await interactivity.WaitForMessageAsync(
                    xm => (xm.Channel.Id == _cid && 
                          !xm.Author.IsBot && 
                          xm.Content.Length == 1 && 
                          Char.IsLetterOrDigit(xm.Content[0]) &&
                          !_badguesses.Contains(xm.Content[0])) || xm.Content.ToLower() == _word
                    , TimeSpan.FromMinutes(1)
                ).ConfigureAwait(false);
            if (m == null) {
                _gameOver = true;
                return;
            }

            if (m.Message.Content.ToLower() == _word) {
                Winner = m.User;
                _gameOver = true;
            }

            char guess_char = Char.ToLower(m.Message.Content[0]);
            if (_word.IndexOf(guess_char) != -1) {
                for (int i = 0; i < _word.Length; i++)
                    if (_word[i] == guess_char)
                        _hidden[i] = Char.ToUpper(_word[i]);
                if (Array.IndexOf(_hidden, '?') == -1) {
                    Winner = m.User;
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
            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                Title = string.Join(" ", _hidden),
                Description = $@". ┌─────┐
.┃...............┋
.┃...............┋
.┃{(_lives < 6 ? ".............😲" : "")}
.┃{(_lives < 5 ? "............./" : "")} {(_lives < 4 ? "|" : "")} {(_lives < 3 ? "\\" : "")}
.┃{(_lives < 2 ? "............../" : "")} {(_lives < 1 ? "\\" : "")}
/-\"
            }.WithFooter(string.Join(", ", _badguesses)))
            .ConfigureAwait(false);
        }
    }
}


