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

namespace TheGodfather.Commands.Games
{
    public class Hangman
    {
        public static bool GameExistsInChannel(ulong cid) => _channels.Contains(cid);
        private static ConcurrentHashSet<ulong> _channels = new ConcurrentHashSet<ulong>();

        private DiscordClient _client;
        private ulong _cid;
        private string _word;
        private char[] _hidden;
        private DiscordMessage _msg;
        private int lives = 7;
        private bool _gameOver = false;


        public Hangman(DiscordClient client, ulong cid, string word)
        {
            _channels.Add(_cid);
            _client = client;
            _cid = cid;
            _word = word.ToLower();
            _hidden = word.Select(c => (c == ' ') ? ' ' : '?').ToArray();
        }


        public async Task Play()
        {
            var channel = await _client.GetChannelAsync(_cid)
                .ConfigureAwait(false);

            _msg = await channel.SendMessageAsync("Game starts!")
                .ConfigureAwait(false);

            await UpdateHangman().ConfigureAwait(false);
            while (!_gameOver && lives > 0 && Array.IndexOf(_hidden, '?') != -1)
                await Advance().ConfigureAwait(false);

            await channel.SendMessageAsync("Game over! The word was : " + Formatter.Bold(_word))
                .ConfigureAwait(false);
            _channels.TryRemove(_cid);
        }

        private async Task Advance()
        {
            var interactivity = _client.GetInteractivityModule();
            var m = await interactivity.WaitForMessageAsync(
                    xm => xm.Channel.Id == _cid && !xm.Author.IsBot && xm.Content.Length == 1 && Char.IsLetterOrDigit(xm.Content[0]),
                    TimeSpan.FromMinutes(1)
                ).ConfigureAwait(false);
            if (m == null) {
                _gameOver = true;
            }

            char guess_char = Char.ToLower(m.Message.Content[0]);
            if (_word.IndexOf(guess_char) != -1) {
                for (int i = 0; i < _word.Length; i++)
                    if (_word[i] == guess_char)
                        _hidden[i] = Char.ToUpper(_word[i]);
            } else {
                lives--;
            }
            await UpdateHangman()
                .ConfigureAwait(false);
        }

        private async Task UpdateHangman()
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

            await _msg.ModifyAsync("WORD: " + new string(_hidden) + s)
                .ConfigureAwait(false);
        }
    }
}


