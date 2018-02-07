#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
#endregion

namespace TheGodfather.Modules.Games
{
    public sealed class Connect4 : BoardGame
    {
        private static string _header = string.Join("", EmojiUtil.Numbers.Take(9));


        public Connect4(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser player1, DiscordUser player2)
            : base(interactivity, channel, player1, player2, 9, 7) { }


        protected override async Task AdvanceAsync()
        {
            int column = 0;
            bool player1plays = _move % 2 == 0;
            var mctx = await _interactivity.WaitForMessageAsync(
                xm => {
                    if (xm.Channel.Id != _channel.Id) return false;
                    if (player1plays && (xm.Author.Id != _p1.Id)) return false;
                    if (!player1plays && (xm.Author.Id != _p2.Id)) return false;
                    if (!int.TryParse(xm.Content, out column)) return false;
                    return column > 0 && column <= BOARD_SIZE_X;
                },
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (column == 0) {
                NoReply = true;
                return;
            }

            if (TryPlayMove(player1plays ? 1 : 2, 1, column - 1)) {
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

        protected override bool GameOver()
        {
            // left - right
            for (int i = 0; i < BOARD_SIZE_Y; i++) {
                for (int j = 0; j < BOARD_SIZE_X - 3; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i, j + 1] && _board[i, j] == _board[i, j + 2] && _board[i, j] == _board[i, j + 3])
                        return true;
                }
            }

            // up - down
            for (int i = 0; i < BOARD_SIZE_Y - 3; i++) {
                for (int j = 0; j < BOARD_SIZE_X; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j] && _board[i, j] == _board[i + 2, j] && _board[i, j] == _board[i + 3, j])
                        return true;
                }
            }

            // diagonal - right
            for (int i = 0; i < BOARD_SIZE_Y - 3; i++) {
                for (int j = 0; j < BOARD_SIZE_X - 3; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j + 1] && _board[i, j] == _board[i + 2, j + 2] && _board[i, j] == _board[i + 3, j + 3])
                        return true;
                }
            }

            // diagonal - left 
            for (int i = 0; i < BOARD_SIZE_Y - 3; i++) {
                for (int j = 3; j < BOARD_SIZE_X; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j - 1] && _board[i, j] == _board[i + 2, j - 2] && _board[i, j] == _board[i + 3, j - 3])
                        return true;
                }
            }

            return false;
        }

        protected override bool TryPlayMove(int val, int row, int col)
        {
            if (_board[0, col] != 0)
                return false;
            while (row < 7 && _board[row, col] == 0)
                row++;
            _board[row - 1, col] = val;
            return true;
        }

        protected override async Task UpdateBoardAsync()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(_header);
            for (int i = 0; i < 7; i++) {
                for (int j = 0; j < 9; j++)
                    switch (_board[i, j]) {
                        case 0: sb.Append(EmojiUtil.BoardSquare); break;
                        case 1: sb.Append(EmojiUtil.BoardPieceBlueCircle); break;
                        case 2: sb.Append(EmojiUtil.BoardPieceRedCircle); break;
                    }
                sb.AppendLine();
            }

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                Description = sb.ToString()
            }.Build()).ConfigureAwait(false);
        }
    }
}


