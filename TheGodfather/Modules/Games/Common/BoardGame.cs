using System;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System.Threading.Tasks;

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


        protected BoardGame(InteractivityExtension interactivity, DiscordChannel channel, DiscordUser p1, DiscordUser p2, int size_x, int size_y) 
            : base(interactivity, channel)
        {
            BOARD_SIZE_X = size_x;
            BOARD_SIZE_Y = size_y;
            _board = new int[BOARD_SIZE_Y, BOARD_SIZE_X];
            _p1 = p1;
            _p2 = p2;
        }


        public async Task StartAsync()
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


        protected abstract Task AdvanceAsync();
        protected abstract bool GameOver();
        protected abstract bool TryPlayMove(int val, int row, int col);
        protected abstract Task UpdateBoardAsync();
    }
}
