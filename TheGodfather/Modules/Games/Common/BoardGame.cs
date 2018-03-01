#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public abstract class BoardGame : Game
    {
        protected DiscordUser _p1 { get; }
        protected DiscordUser _p2 { get; }
        protected int BOARD_SIZE_X { get; }
        protected int BOARD_SIZE_Y { get; }

        protected DiscordMessage _msg;
        protected int[,] _board;
        protected int _move = 0;
        protected bool _deletefailed = false;

        protected int BoardElementAt(int row, int col)
        {
            if (col >= 0 && col < BOARD_SIZE_X && row >= 0 && row < BOARD_SIZE_Y)
                return _board[row, col];
            else
                return -1;
        }


        protected BoardGame(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser p1, DiscordUser p2, int size_x, int size_y)
            : base(interactivity, channel)
        {
            BOARD_SIZE_X = size_x;
            BOARD_SIZE_Y = size_y;
            _board = new int[BOARD_SIZE_Y, BOARD_SIZE_X];
            _p1 = p1;
            _p2 = p2;
        }


        public sealed override async Task RunAsync()
        {
            _msg = await _channel.SendMessageAsync($"{_p1.Mention} vs {_p2.Mention}")
                .ConfigureAwait(false);

            while (NoReply == false && _move < BOARD_SIZE_X * BOARD_SIZE_Y && !GameOver()) {
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

        protected virtual bool TryPlayMove(int val, int row, int col)
        {
            if (_board[row, col] != 0)
                return false;
            _board[row, col] = val;
            return true;
        }

        protected virtual async Task AdvanceAsync()
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
                    } catch {
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


        protected abstract bool GameOver();
        protected abstract Task UpdateBoardAsync();
    }
}
