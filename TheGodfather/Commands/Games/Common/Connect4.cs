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
    public class Connect4
    {
        #region PUBLIC_FIELDS
        public DiscordUser Winner { get; private set; }
        #endregion

        #region STATIC_FIELDS
        public static bool GameExistsInChannel(ulong cid) => _channels.Contains(cid);
        private static ConcurrentHashSet<ulong> _channels = new ConcurrentHashSet<ulong>();
        #endregion

        #region PRIVATE_FIELDS
        private DiscordClient _client;
        private ulong _cid;
        private ulong _p1Id;
        private ulong _p2Id;
        private DiscordMessage _msg;
        private int[,] _board = new int[7, 9];
        private int _move = 0;
        #endregion


        public Connect4(DiscordClient client, ulong cid, ulong p1Id, ulong p2Id)
        {
            _channels.Add(_cid);
            _client = client;
            _cid = cid;
            _p1Id = p1Id;
            _p2Id = p2Id;
        }


        public async Task PlayAsync()
        {
            var channel = await _client.GetChannelAsync(_cid)
                .ConfigureAwait(false);
            _msg = await channel.SendMessageAsync("Game begins!")
                .ConfigureAwait(false);

            C4InitializeBoard();

            while (_move < 7*9 && !C4GameOver()) {
                await C4UpdateBoardAsync()
                    .ConfigureAwait(false);

                await AdvanceAsync(channel)
                    .ConfigureAwait(false);

                _move++;
            }

            if (C4GameOver()) {
                if (_move % 2 == 0)
                    Winner = await _client.GetUserAsync(_p2Id).ConfigureAwait(false);
                else
                    Winner = await _client.GetUserAsync(_p1Id).ConfigureAwait(false);
            } else {
                Winner = null;
            }

            await C4UpdateBoardAsync()
                .ConfigureAwait(false);
            _channels.TryRemove(_cid);
        }

        private async Task AdvanceAsync(DiscordChannel channel)
        {
            int column = 0;
            bool player1plays = _move % 2 == 0;
            var t = await _client.GetInteractivityModule().WaitForMessageAsync(
                xm => {
                    if (xm.Channel.Id != _cid) return false;
                    if (player1plays && (xm.Author.Id != _p1Id)) return false;
                    if (!player1plays && (xm.Author.Id != _p2Id)) return false;
                    try {
                        column = int.Parse(xm.Content);
                        if (column < 1 || column > 10)
                            return false;
                    } catch {
                        return false;
                    }
                    return true;
                },
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (column == 0) {
                await channel.SendMessageAsync("No reply, aborting...")
                    .ConfigureAwait(false);
                _move = 7*9;
                return;
            }

            if (C4PlaySuccessful(player1plays ? 1 : 2, column - 1))
                player1plays = !player1plays;
            else
                await channel.SendMessageAsync("Invalid move.").ConfigureAwait(false);
        }

        private bool C4GameOver()
        {
            // TODO
            return false;
        }

        private void C4InitializeBoard()
        {
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 9; j++)
                    _board[i, j] = 0;
        }

        private bool C4PlaySuccessful(int v, int c)
        {
            if (_board[0, c] != 0)
                return false;
            int r = 1;
            while (r < 6 && _board[r, c] == 0)
                r++;
            _board[r, c] = v;
            return true;
        }

        private async Task C4UpdateBoardAsync()
        {
            string s = "";
            for (int i = 0; i < 7; i++) {
                for (int j = 0; j < 9; j++)
                    switch (_board[i, j]) {
                        case 0: s += $"{DiscordEmoji.FromName(_client, ":white_medium_square:")}"; break;
                        case 1: s += $"{DiscordEmoji.FromName(_client, ":large_blue_circle:")}"; break;
                        case 2: s += $"{DiscordEmoji.FromName(_client, ":red_circle:")}"; break;
                    }
                s += '\n';
            }

            await _msg.ModifyAsync(embed: new DiscordEmbedBuilder() { Description = s })
                .ConfigureAwait(false);
        }
    }
}


