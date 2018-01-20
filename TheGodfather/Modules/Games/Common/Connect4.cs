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
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
#endregion

namespace TheGodfather.Modules.Games
{
    public class Connect4
    {
        #region PUBLIC_FIELDS
        public DiscordUser Winner { get; private set; }
        public bool NoReply { get; private set; }
        #endregion

        #region STATIC_FIELDS
        public static bool GameExistsInChannel(ulong cid) => _channels.Contains(cid);
        private static ConcurrentHashSet<ulong> _channels = new ConcurrentHashSet<ulong>();
        #endregion

        #region PRIVATE_FIELDS
        private DiscordClient _client;
        private ulong _cid;
        private DiscordUser _p1;
        private DiscordUser _p2;
        private DiscordMessage _msg;
        private int[,] _board = new int[7, 9];
        private int _move = 0;
        private bool _delWarnIssued = false;
        #endregion


        public Connect4(DiscordClient client, ulong cid, DiscordUser p1, DiscordUser p2)
        {
            _channels.Add(_cid);
            _client = client;
            _cid = cid;
            _p1 = p1;
            _p2 = p2;
        }


        public async Task PlayAsync()
        {
            var channel = await _client.GetChannelAsync(_cid)
                .ConfigureAwait(false);
            _msg = await channel.SendMessageAsync($"{_p1.Mention} vs {_p2.Mention}")
                .ConfigureAwait(false);

            while (NoReply == false && _move < 7 * 9 && !GameOver()) {
                await UpdateBoardAsync()
                    .ConfigureAwait(false);

                await AdvanceAsync(channel)
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
            _channels.TryRemove(_cid);
        }

        private async Task AdvanceAsync(DiscordChannel channel)
        {
            int column = 0;
            bool player1plays = _move % 2 == 0;
            var t = await _client.GetInteractivity().WaitForMessageAsync(
                xm => {
                    if (xm.Channel.Id != _cid) return false;
                    if (player1plays && (xm.Author.Id != _p1.Id)) return false;
                    if (!player1plays && (xm.Author.Id != _p2.Id)) return false;
                    if (!int.TryParse(xm.Content, out column)) return false;
                    return column > 0 && column < 10;
                },
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (column == 0) {
                await channel.SendMessageAsync("No reply, aborting...")
                    .ConfigureAwait(false);
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

        private bool PlaySuccessful(int v, int c)
        {
            if (_board[0, c] != 0)
                return false;
            int r = 1;
            while (r < 7 && _board[r, c] == 0)
                r++;
            _board[r - 1, c] = v;
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

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() { Description = sb.ToString() })
                .ConfigureAwait(false);
        }
    }
}


