#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TheGodfather.Exceptions;
using TheGodfather.Helpers.Collections;

using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Games.Common
{
    public class Nunchi
    {
        #region STATIC_FIELDS
        public static bool GameExistsInChannel(ulong cid) => _channels.Contains(cid);
        private static ConcurrentHashSet<ulong> _channels = new ConcurrentHashSet<ulong>();
        #endregion

        #region PUBLIC_FIELDS
        public int ParticipantCount => _participants.Count();
        public bool GameRunning { get; private set; }
        #endregion

        #region PRIVATE_FIELDS
        private List<ulong> _participants = new List<ulong>();
        private ulong _cid;
        DiscordClient _client;
        #endregion


        public Nunchi(DiscordClient client, ulong cid)
        {
            _client = client;
            _cid = cid;
            GameRunning = false;
            _channels.Add(_cid);
        }


        public bool AddParticipant(ulong uid)
        {
            if (_participants.Contains(uid))
                return false;
            _participants.Add(uid);
            return true;
        }

        public async Task PlayAsync()
        {
            GameRunning = true;
            var chn = await _client.GetChannelAsync(_cid)
                .ConfigureAwait(false);

            int num = new Random().Next(1000);
            await chn.SendMessageAsync(num.ToString())
                .ConfigureAwait(false);

            var interactivity = _client.GetInteractivityModule();
            DiscordUser winner = null;
            while (_participants.Count > 1) {
                int n = 0;
                var msg = await interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.Channel.Id != _cid || xm.Author.IsBot) return false;
                        if (!_participants.Contains(xm.Author.Id)) return false;
                        return int.TryParse(xm.Content, out n);
                    },
                    TimeSpan.FromSeconds(20)
                ).ConfigureAwait(false);

                if (msg == null || n == 0) {
                    if (winner == null) {
                        await chn.SendMessageAsync("No replies, aborting...")
                            .ConfigureAwait(false);
                    } else {
                        await chn.SendMessageAsync($"{winner.Mention} won due to no replies from other users!")
                            .ConfigureAwait(false);
                    }
                    Stop();
                    return;
                } else if (n == num + 1) {
                    num++;
                    winner = msg.User;
                } else {
                    await chn.SendMessageAsync(msg.User.Mention + " lost!")
                        .ConfigureAwait(false);
                    if (winner != null && winner.Id == msg.User.Id)
                        winner = null;
                    _participants.Remove(msg.User.Id);
                }
            }

            await chn.SendMessageAsync("Game over! Winner: " + winner.Mention)
                .ConfigureAwait(false);
            Stop();
        }

        public void Stop()
        {
            _channels.TryRemove(_cid);
        }
    }
}
