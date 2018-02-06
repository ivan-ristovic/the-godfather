#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Games
{
    public class Nunchi
    {
        #region PUBLIC_FIELDS
        public int ParticipantCount => _participants.Count();
        public bool GameRunning { get; private set; }
        public DiscordUser Winner { get; private set; }
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

            var interactivity = _client.GetInteractivity();
            Winner = null;
            while (ParticipantCount > 1) {
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
                    if (Winner == null) {
                        await chn.SendMessageAsync("No replies, aborting...")
                            .ConfigureAwait(false);
                    } else {
                        await chn.SendMessageAsync($"{Winner.Mention} won due to no replies from other users!")
                            .ConfigureAwait(false);
                    }
                    return;
                } else if (n == num + 1) {
                    num++;
                    Winner = msg.User;
                } else {
                    await chn.SendMessageAsync(msg.User.Mention + " lost!")
                        .ConfigureAwait(false);
                    if (Winner != null && Winner.Id == msg.User.Id)
                        Winner = null;
                    _participants.Remove(msg.User.Id);
                }
            }

            Winner = await _client.GetUserAsync(_participants[0])
                .ConfigureAwait(false);
            await chn.SendMessageAsync("Game over! Winner: " + Winner.Mention)
                .ConfigureAwait(false);
        }
    }
}
