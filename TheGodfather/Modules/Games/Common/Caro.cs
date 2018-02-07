#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public sealed class Caro : BoardGame
    {
        private static string _header = DiscordEmoji.FromUnicode("\U0001f199") + string.Join("", EmojiUtil.Numbers);


        public Caro(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser player1, DiscordUser player2)
            : base(interactivity, channel, player1, player2, 10, 10) { }


        protected override async Task AdvanceAsync()
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
                    return row > 0 && row <= BOARD_SIZE_Y && col > 0 && col <= BOARD_SIZE_X;
                },
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (mctx == null) {
                NoReply = true;
                return;
            }

            if (TryPlayMove(player1plays ? 1 : 2, row - 1, col - 1)) {
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
                await _channel.SendMessageAsync($"Move [{row} {col}] is invalid.")
                    .ConfigureAwait(false);
            }
        }

        protected override bool GameOver()
        {
            // left - right
            for (int i = 0; i < BOARD_SIZE_Y; i++) {
                for (int j = 0; j < BOARD_SIZE_X - 4; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i, j + 1] && _board[i, j] == _board[i, j + 2] && _board[i, j] == _board[i, j + 3] && _board[i, j] == _board[i, j + 4])
                        return true;
                }
            }

            // up - down
            for (int i = 0; i < BOARD_SIZE_Y - 4; i++) {
                for (int j = 0; j < BOARD_SIZE_X; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j] && _board[i, j] == _board[i + 2, j] && _board[i, j] == _board[i + 3, j] && _board[i, j] == _board[i + 4, j])
                        return true;
                }
            }

            // diagonal - right
            for (int i = 0; i < BOARD_SIZE_Y - 4; i++) {
                for (int j = 0; j < BOARD_SIZE_X - 4; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j + 1] && _board[i, j] == _board[i + 2, j + 2] && _board[i, j] == _board[i + 3, j + 3] && _board[i, j] == _board[i + 4, j + 4])
                        return true;
                }
            }

            // diagonal - left 
            for (int i = 0; i < BOARD_SIZE_Y - 4; i++) {
                for (int j = 4; j < BOARD_SIZE_X; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j - 1] && _board[i, j] == _board[i + 2, j - 2] && _board[i, j] == _board[i + 3, j - 3] && _board[i, j] == _board[i + 4, j - 4])
                        return true;
                }
            }

            return false;
        }

        protected override bool TryPlayMove(int val, int row, int col)
        {
            if (_board[row, col] != 0)
                return false;

            _board[row, col] = val;
            return true;
        }

        protected override async Task UpdateBoardAsync()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(_header);
            for (int i = 0; i < BOARD_SIZE_Y; i++) {
                sb.Append(EmojiUtil.Numbers[i]);
                for (int j = 0; j < BOARD_SIZE_X; j++)
                    switch (_board[i, j]) {
                        case 0: sb.Append(EmojiUtil.BoardSquare); break;
                        case 1: sb.Append(EmojiUtil.BoardPieceX); break;
                        case 2: sb.Append(EmojiUtil.BoardPieceO); break;
                    }
                sb.AppendLine();
            }

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                Description = sb.ToString()
            }.Build()).ConfigureAwait(false);
        }
    }
}


