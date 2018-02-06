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
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class Caro : Game
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
            DiscordEmoji.FromUnicode("9\u20e3"),
            DiscordEmoji.FromUnicode("\U0001f51f")
        };
        private static string _header = DiscordEmoji.FromUnicode("\U0001f199") + string.Join("", _numbers);
        private string _square = DiscordEmoji.FromUnicode("\u25fb");
        private string _x = DiscordEmoji.FromUnicode("\u274c");
        private string _o = DiscordEmoji.FromUnicode("\u2b55");
        #endregion

        #region PRIVATE_FIELDS
        private DiscordUser _p1;
        private DiscordUser _p2;
        private DiscordMessage _msg;
        private const int BOARD_SIZE = 10;
        private int[,] _board = new int[BOARD_SIZE, BOARD_SIZE];
        private int _move = 0;
        private bool _delWarnIssued = false;
        #endregion


        public Caro(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser player1, DiscordUser player2)
        {
            _interactivity = interactivity;
            _channel = channel;
            _p1 = player1;
            _p2 = player2;
        }


        public async Task StartAsync()
        {
            _msg = await _channel.SendMessageAsync($"{_p1.Mention} vs {_p2.Mention}")
                .ConfigureAwait(false);

            while (NoReply == false && _move < BOARD_SIZE * BOARD_SIZE && !GameOver()) {
                await UpdateBoardAsync()
                    .ConfigureAwait(false);
                await AdvanceAsync()
                    .ConfigureAwait(false);
            }

            if (GameOver())
                Winner = (_move % 2 == 0) ? _p2 : _p1;
            else
                Winner = null;

            await UpdateBoardAsync()
                .ConfigureAwait(false);
        }


        private async Task AdvanceAsync()
        {
            int row = 0, col = 0;
            bool player1plays = (_move % 2 == 0);
            var mctx = await _interactivity.WaitForMessageAsync(
                xm => {
                    if (xm.Channel.Id != _channel.Id) return false;
                    if (player1plays && (xm.Author.Id != _p1.Id)) return false;
                    if (!player1plays && (xm.Author.Id != _p2.Id)) return false;
                    var split = xm.Content.Split(' ');
                    if (split.Length < 2) return false;
                    if (!int.TryParse(split[0], out row)) return false;
                    if (!int.TryParse(split[1], out col)) return false;
                    return row > 0 && row < 11 && col > 0 && col < 11;
                },
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (mctx == null) {
                NoReply = true;
                return;
            }

            if (TryPlayMove(player1plays ? 1 : 2, row - 1, col - 1)) {
                _move++;
                try {
                    await mctx.Message.DeleteAsync()
                        .ConfigureAwait(false);
                } catch (UnauthorizedException) {
                    if (!_delWarnIssued) {
                        await _channel.SendMessageAsync("Consider giving me the permissions to delete messages so that I can clean up the move posts.")
                            .ConfigureAwait(false);
                        _delWarnIssued = true;
                    }
                }
            } else {
                await _channel.SendMessageAsync($"Move [{row} {col}] is invalid.")
                    .ConfigureAwait(false);
            }
        }

        private bool GameOver()
        {
            // left - right
            for (int i = 0; i < BOARD_SIZE; i++) {
                for (int j = 0; j < BOARD_SIZE - 4; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i, j + 1] && _board[i, j] == _board[i, j + 2] && _board[i, j] == _board[i, j + 3] && _board[i, j] == _board[i, j + 4])
                        return true;
                }
            }

            // up - down
            for (int i = 0; i < BOARD_SIZE - 4; i++) {
                for (int j = 0; j < BOARD_SIZE; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j] && _board[i, j] == _board[i + 2, j] && _board[i, j] == _board[i + 3, j] && _board[i, j] == _board[i + 4, j])
                        return true;
                }
            }

            // diagonal - right
            for (int i = 0; i < BOARD_SIZE - 4; i++) {
                for (int j = 0; j < BOARD_SIZE - 4; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j + 1] && _board[i, j] == _board[i + 2, j + 2] && _board[i, j] == _board[i + 3, j + 3] && _board[i, j] == _board[i + 4, j + 4])
                        return true;
                }
            }

            // diagonal - left 
            for (int i = 0; i < BOARD_SIZE - 4; i++) {
                for (int j = 3; j < BOARD_SIZE; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j - 1] && _board[i, j] == _board[i + 2, j - 2] && _board[i, j] == _board[i + 3, j - 3] && _board[i, j] == _board[i + 4, j - 4])
                        return true;
                }
            }

            return false;
        }

        private bool TryPlayMove(int val, int row, int col)
        {
            if (_board[row, col] != 0)
                return false;

            _board[row, col] = val;
            return true;
        }

        private async Task UpdateBoardAsync()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(_header);
            for (int i = 0; i < BOARD_SIZE; i++) {
                sb.Append(_numbers[i]);
                for (int j = 0; j < BOARD_SIZE; j++)
                    switch (_board[i, j]) {
                        case 0: sb.Append(_square); break;
                        case 1: sb.Append(_x); break;
                        case 2: sb.Append(_o); break;
                    }
                sb.AppendLine();
            }

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                Description = sb.ToString()
            }.Build()).ConfigureAwait(false);
        }
    }
}


