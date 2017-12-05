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
#endregion

namespace TheGodfather.Commands.Games
{
    public class TicTacToe
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
        private int[,] _board = new int[3, 3];
        private int _move = 0;
        #endregion


        public TicTacToe(DiscordClient client, ulong cid, DiscordUser p1, DiscordUser p2)
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
            _msg = await channel.SendMessageAsync("Game begins!")
                .ConfigureAwait(false);

            TTTInitializeBoard();

            while (NoReply == false && _move < 9 && !TTTGameOver()) {
                await TTTUpdateBoardAsync()
                    .ConfigureAwait(false);

                await AdvanceAsync(channel)
                    .ConfigureAwait(false);
            }

            if (TTTGameOver()) {
                if (_move % 2 == 0)
                    Winner = _p2;
                else
                    Winner = _p1;
            } else {
                Winner = null;
            }

            await TTTUpdateBoardAsync()
                .ConfigureAwait(false);
            _channels.TryRemove(_cid);
        }

        private async Task AdvanceAsync(DiscordChannel channel)
        {
            int field = 0;
            bool player1plays = _move % 2 == 0;
            var t = await _client.GetInteractivityModule().WaitForMessageAsync(
                xm => {
                    if (xm.Channel.Id != _cid) return false;
                    if (player1plays && (xm.Author.Id != _p1.Id)) return false;
                    if (!player1plays && (xm.Author.Id != _p2.Id)) return false;
                    if (!int.TryParse(xm.Content, out field)) return false;
                    return field > 0 && field < 10;
                },
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (field == 0) {
                await channel.SendMessageAsync("No reply, aborting...")
                    .ConfigureAwait(false);
                NoReply = true;
                return;
            }

            if (TTTPlaySuccessful(player1plays ? 1 : 2, field))
                _move++;
            else
                await channel.SendMessageAsync("Invalid move.").ConfigureAwait(false);
        }

        private bool TTTGameOver()
        {
            for (int i = 0; i < 3; i++) {
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

        private void TTTInitializeBoard()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    _board[i, j] = 0;
        }

        private bool TTTPlaySuccessful(int v, int index)
        {
            index--;
            if (_board[index / 3, index % 3] != 0)
                return false;
            _board[index / 3, index % 3] = v;
            return true;
        }

        private async Task TTTUpdateBoardAsync()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++)
                    switch (_board[i, j]) {
                        case 0: sb.Append(DiscordEmoji.FromName(_client, ":white_medium_square:")); break;
                        case 1: sb.Append(DiscordEmoji.FromName(_client, ":x:")); break;
                        case 2: sb.Append(DiscordEmoji.FromName(_client, ":o:")); break;
                    }
                sb.AppendLine();
            }

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() { Description = sb.ToString() })
                .ConfigureAwait(false);
        }
    }
}


