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
        #region PRIVATE_FIELDS
        private DiscordClient _client;
        private DiscordUser _p1;
        private DiscordUser _p2;
        private DiscordMessage _msg;
        private int[,] _board = new int[7, 9];
        private int _move = 0;
        private bool _delWarnIssued = false;
        #endregion


        public Connect4(DiscordClient client, DiscordChannel channel, DiscordUser p1, DiscordUser p2)
        {
            _client = client;
            _channel = channel;
            _p1 = p1;
            _p2 = p2;
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
            var t = await _client.GetInteractivity().WaitForMessageAsync(
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

            if (PlaySuccessful(player1plays ? 1 : 2, column - 1)) {
                _move++;
                try {
                    await t.Message.DeleteAsync()
                        .ConfigureAwait(false);
                } catch (UnauthorizedException) {
                    if (!_delWarnIssued) {
                        await _channel.SendMessageAsync("Consider giving me the delete message permissions so I can clean up the move posts.")
                            .ConfigureAwait(false);
                        _delWarnIssued = true;
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

        private bool PlaySuccessful(int v, int col)
        {
            if (_board[0, col] != 0)
                return false;
            int r = 1;
            while (r < 7 && _board[r, col] == 0)
                r++;
            _board[r - 1, col] = v;
            return true;
        }

        private async Task UpdateBoardAsync()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 7; i++) {
                for (int j = 0; j < 9; j++)
                    switch (_board[i, j]) {
                        case 0: sb.Append(DiscordEmoji.FromName(_client, ":white_medium_square:")); break;
                        case 1: sb.Append(DiscordEmoji.FromName(_client, ":large_blue_circle:")); break;
                        case 2: sb.Append(DiscordEmoji.FromName(_client, ":red_circle:")); break;
                    }
                sb.AppendLine();
            }

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                Description = sb.ToString()
            }.Build()).ConfigureAwait(false);
        }
    }
}


