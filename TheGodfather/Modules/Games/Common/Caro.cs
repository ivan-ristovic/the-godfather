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
        #region PRIVATE_FIELDS
        private DiscordUser _p1;
        private DiscordUser _p2;
        private DiscordMessage _msg;
        private const int board_size = 10;
        private int[,] _board = new int[board_size, board_size];
        private int _move = 0;
        private bool _delWarnIssued = false;
        #endregion


        public Caro(DiscordClient client, DiscordChannel channel, DiscordUser player1, DiscordUser player2)
        {
            _client = client;
            _channel = channel;
            _p1 = player1;
            _p2 = player2;
        }


        public async Task PlayAsync()
        {
            _msg = await _channel.SendMessageAsync("Game begins!")
                .ConfigureAwait(false);

            while (NoReply == false && _move < board_size * board_size && !GameOver()) {
                await UpdateBoardAsync()
                    .ConfigureAwait(false);

                await AdvanceAsync(_channel)
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

        private async Task AdvanceAsync(DiscordChannel channel)
        {
            int row = 0, column = 0;
            bool player1plays = (_move % 2 == 0);
            var t = await _client.GetInteractivity().WaitForMessageAsync(
                xm => {
                    if (xm.Channel.Id != _channel.Id) return false;
                    if (player1plays && (xm.Author.Id != _p1.Id)) return false;
                    if (!player1plays && (xm.Author.Id != _p2.Id)) return false;
                    var split = xm.Content.Split(' ');
                    if (split.Length < 2) return false;
                    if (!int.TryParse(split[0], out row)) return false;
                    if (!int.TryParse(split[1], out column)) return false;
                    return row > 0 && row < 11 && column > 0 && column < 11;
                },
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (row == 0 || column == 0) {
                NoReply = true;
                return;
            }

            if (PlaySuccessful(player1plays ? 1 : 2, row - 1, column - 1)) {
                _move++;
                try {
                    await t.Message.DeleteAsync()
                        .ConfigureAwait(false);
                } catch (UnauthorizedException) {
                    if (!_delWarnIssued) {
                        await channel.SendMessageAsync("Consider giving me the delete message permissions so I can clean up the move posts.")
                            .ConfigureAwait(false);
                        _delWarnIssued = true;
                    }
                }
            } else {
                await channel.SendMessageAsync("Invalid move.")
                    .ConfigureAwait(false);
            }
        }

        private bool GameOver()
        {
            // left - right
            for (int i = 0; i < board_size; i++) {
                for (int j = 0; j < board_size - 4; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i, j + 1] && _board[i, j] == _board[i, j + 2] && _board[i, j] == _board[i, j + 3] && _board[i, j] == _board[i, j + 4])
                        return true;
                }
            }

            // up - down
            for (int i = 0; i < board_size - 4; i++) {
                for (int j = 0; j < board_size; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j] && _board[i, j] == _board[i + 2, j] && _board[i, j] == _board[i + 3, j] && _board[i, j] == _board[i + 4, j])
                        return true;
                }
            }

            // diagonal - right
            for (int i = 0; i < board_size - 4; i++) {
                for (int j = 0; j < board_size - 4; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j + 1] && _board[i, j] == _board[i + 2, j + 2] && _board[i, j] == _board[i + 3, j + 3] && _board[i, j] == _board[i + 4, j + 4])
                        return true;
                }
            }

            // diagonal - left 
            for (int i = 0; i < board_size - 4; i++) {
                for (int j = 3; j < board_size; j++) {
                    if (_board[i, j] == 0)
                        continue;
                    if (_board[i, j] == _board[i + 1, j - 1] && _board[i, j] == _board[i + 2, j - 2] && _board[i, j] == _board[i + 3, j - 3] && _board[i, j] == _board[i + 4, j - 4])
                        return true;
                }
            }

            return false;
        }

        private bool PlaySuccessful(int v, int r, int c)
        {
            if (_board[r, c] != 0)
                return false;

            _board[r, c] = v;
            return true;
        }

        private async Task UpdateBoardAsync()
        {
            var numbers = new string[] {
                DiscordEmoji.FromName(_client, ":one:").ToString(),
                DiscordEmoji.FromName(_client, ":two:").ToString(),
                DiscordEmoji.FromName(_client, ":three:").ToString(),
                DiscordEmoji.FromName(_client, ":four:").ToString(),
                DiscordEmoji.FromName(_client, ":five:").ToString(),
                DiscordEmoji.FromName(_client, ":six:").ToString(),
                DiscordEmoji.FromName(_client, ":seven:").ToString(),
                DiscordEmoji.FromName(_client, ":eight:").ToString(),
                DiscordEmoji.FromName(_client, ":nine:").ToString(),
                DiscordEmoji.FromName(_client, ":ten:").ToString()
            };

            StringBuilder sb = new StringBuilder();
            sb.Append(DiscordEmoji.FromName(_client, ":up:") + string.Join("", numbers)).AppendLine();
            for (int i = 0; i < board_size; i++) {
                sb.Append(numbers[i]);
                for (int j = 0; j < board_size; j++)
                    switch (_board[i, j]) {
                        case 0: sb.Append(DiscordEmoji.FromName(_client, ":white_medium_square:")); break;
                        case 1: sb.Append(DiscordEmoji.FromName(_client, ":x:")); break;
                        case 2: sb.Append(DiscordEmoji.FromName(_client, ":o:")); break;
                    }
                sb.AppendLine();
            }

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                Description = sb.ToString()
            }.Build()).ConfigureAwait(false);
        }
    }
}


