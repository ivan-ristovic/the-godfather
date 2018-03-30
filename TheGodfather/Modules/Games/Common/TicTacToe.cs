#region USING_DIRECTIVES
using System;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class TicTacToe : BoardGame
    {
        public TicTacToe(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser p1, DiscordUser p2, TimeSpan? movetime = null)
            : base(interactivity, channel, p1, p2, 3, 3, movetime) { }


        protected override async Task AdvanceAsync()
        {
            int field = 0;
            bool player1plays = _move % 2 == 0;
            var mctx = await _interactivity.WaitForMessageAsync(
                xm => {
                    if (xm.Channel.Id != _channel.Id) return false;
                    if (player1plays && (xm.Author.Id != _p1.Id)) return false;
                    if (!player1plays && (xm.Author.Id != _p2.Id)) return false;
                    if (!int.TryParse(xm.Content, out field)) return false;
                    return field > 0 && field < 10;
                },
                _movetime
            ).ConfigureAwait(false);
            if (mctx == null) {
                NoReply = true;
                Winner = player1plays ? _p2 : _p1;
                return;
            }

            if (TryPlayMove(player1plays ? 1 : 2, (field - 1) / BOARD_SIZE_Y, (field - 1) % BOARD_SIZE_X)) {
                _move++;
                if (!_deletefailed) {
                    try {
                        await mctx.Message.DeleteAsync()
                            .ConfigureAwait(false);
                    } catch {
                        await _channel.SendMessageAsync("Consider giving me the permissions to delete messages so that I can clean up the move posts and keep the board at the bottom.")
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
            for (int i = 0; i < BOARD_SIZE_Y; i++) {
                if (_board[i, 0] != 0 && _board[i, 0] == _board[i, 1] && _board[i, 1] == _board[i, 2])
                    return true;
                if (_board[0, i] != 0 && _board[0, i] == _board[1, i] && _board[1, i] == _board[2, i])
                    return true;
            }
            if (_board[0, 0] != 0 && _board[0, 0] == _board[1, 1] && _board[1, 1] == _board[2, 2])
                return true;
            if (_board[0, 2] != 0 && _board[0, 2] == _board[1, 1] && _board[1, 1] == _board[2, 0])
                return true;
            return false;
        }

        protected override async Task UpdateBoardAsync()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < BOARD_SIZE_Y; i++) {
                for (int j = 0; j < BOARD_SIZE_X; j++)
                    switch (_board[i, j]) {
                        case 0: sb.Append(StaticDiscordEmoji.BoardSquare); break;
                        case 1: sb.Append(StaticDiscordEmoji.BoardPieceX); break;
                        case 2: sb.Append(StaticDiscordEmoji.BoardPieceO); break;
                    }
                sb.AppendLine();
            }

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                Description = sb.ToString()
            }.Build()).ConfigureAwait(false);
        }
    }
}


