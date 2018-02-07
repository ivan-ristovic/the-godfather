#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Extensions.Collections;
using TheGodfather.Modules.Games.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
#endregion

namespace TheGodfather.Modules.Games
{
    public class Connect4 : Game
    {
        #region STATIC_FIELDS
        private static string[] _numbers = new string[] {
            DiscordEmoji.FromUnicode("1\u20e3"),
            DiscordEmoji.FromUnicode("2\u20e3"),
            DiscordEmoji.FromUnicode("3\u20e3"),
            DiscordEmoji.FromUnicode("4\u20e3"),
            DiscordEmoji.FromUnicode("5\u20e3"),
            DiscordEmoji.FromUnicode("6\u20e3"),
            DiscordEmoji.FromUnicode("7\u20e3"),
            DiscordEmoji.FromUnicode("8\u20e3"),
            DiscordEmoji.FromUnicode("9\u20e3")
        };
        private static string _header = string.Join("", _numbers);
        private string _square = DiscordEmoji.FromUnicode("\u25fb");
        private string _blue = DiscordEmoji.FromUnicode("\U0001f535");
        private string _red = DiscordEmoji.FromUnicode("\U0001f534");
        #endregion

        #region PRIVATE_FIELDS
        private DiscordUser _p1;
        private DiscordUser _p2;
        private DiscordMessage _msg;
        private int[,] _board = new int[7, 9];
        private int _move = 0;
        private bool _deletefailed = false;
        #endregion


        public Connect4(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser player1, DiscordUser player2)
            : base(interactivity, channel)
        {
            _p1 = player1;
            _p2 = player2;
        }


        public async Task StartAsync()
        {
            _msg = await _channel.SendMessageAsync($"{_p1.Mention} vs {_p2.Mention}")
                .ConfigureAwait(false);

            while (NoReply == false && _move < 7 * 9 && !GameOver()) {
                await UpdateBoardAsync()
                    .ConfigureAwait(false);
                await AdvanceAsync()
                    .ConfigureAwait(false);
            }

            if (GameOver()) {
                if (_move % 2 == 0)
                    Winner = _p2;
                else
                    Winner = _p1;
            } else {
                Winner = null;
            }

            await UpdateBoardAsync()
                .ConfigureAwait(false);
        }

        private async Task AdvanceAsync()
        {
            int column = 0;
            bool player1plays = _move % 2 == 0;
            var mctx = await _interactivity.WaitForMessageAsync(
                xm => {
                    if (xm.Channel.Id != _channel.Id) return false;
                    if (player1plays && (xm.Author.Id != _p1.Id)) return false;
                    if (!player1plays && (xm.Author.Id != _p2.Id)) return false;
                    if (!int.TryParse(xm.Content, out column)) return false;
                    return column > 0 && column < 10;
                },
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (column == 0) {
                NoReply = true;
                return;
            }

            if (TryPlayMove(player1plays ? 1 : 2, column - 1)) {
                _move++;
                if (!_deletefailed) {
                    try {
                        await mctx.Message.DeleteAsync()
                            .ConfigureAwait(false);
                    } catch (UnauthorizedException) {
                        await _channel.SendMessageAsync("Consider giving me the permissions to delete messages so that I can clean up the move posts.")
                            .ConfigureAwait(false);
                        _deletefailed = true;
                    }
                }
            } else {
                await _channel.SendMessageAsync("Invalid move.")
                    .ConfigureAwait(false);
            }
        }

        private bool GameOver()
        {
            // left - right
            for (int i = 0; i < 7; i++) {
                for (int j = 0; j < 7; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i, j + 1] && _board[i, j] == _board[i, j + 2] && _board[i, j] == _board[i, j + 3])
                        return true;
                }
            }

            // up - down
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 9; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j] && _board[i, j] == _board[i + 2, j] && _board[i, j] == _board[i + 3, j])
                        return true;
                }
            }

            // diagonal - right
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 7; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j + 1] && _board[i, j] == _board[i + 2, j + 2] && _board[i, j] == _board[i + 3, j + 3])
                        return true;
                }
            }

            // diagonal - left 
            for (int i = 0; i < 4; i++) {
                for (int j = 3; j < 9; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j - 1] && _board[i, j] == _board[i + 2, j - 2] && _board[i, j] == _board[i + 3, j - 3])
                        return true;
                }
            }

            return false;
        }

        private bool TryPlayMove(int val, int col)
        {
            if (_board[0, col] != 0)
                return false;
            int row = 1;
            while (row < 7 && _board[row, col] == 0)
                row++;
            _board[row - 1, col] = val;
            return true;
        }

        private async Task UpdateBoardAsync()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(_header);
            for (int i = 0; i < 7; i++) {
                for (int j = 0; j < 9; j++)
                    switch (_board[i, j]) {
                        case 0: sb.Append(_square); break;
                        case 1: sb.Append(_blue); break;
                        case 2: sb.Append(_red); break;
                    }
                sb.AppendLine();
            }

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                Description = sb.ToString()
            }.Build()).ConfigureAwait(false);
        }
    }
}


