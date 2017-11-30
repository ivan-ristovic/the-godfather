#region USING_DIRECTIVES
using System;
using System.IO;
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
#endregion

namespace TheGodfather.Commands.Games
{
    public class Race
    {
        #region STATIC_FIELDS
        public static bool GameExistsInChannel(ulong cid) => _channels.Contains(cid);
        private static ConcurrentHashSet<ulong> _channels = new ConcurrentHashSet<ulong>();
        #endregion

        #region PUBLIC_FIELDS
        public bool GameRunning { get; private set; }
        public int ParticipantCount => _participants.Count;
        public DiscordUser Winner { get; private set; }
        #endregion

        #region PRIVATE_FIELDS
        private ConcurrentQueue<ulong> _participants = new ConcurrentQueue<ulong>();
        private ConcurrentDictionary<ulong, string> _emojis = new ConcurrentDictionary<ulong, string>();
        private Dictionary<ulong, int> _progress = new Dictionary<ulong, int>();
        private ConcurrentBag<string> _animals = new ConcurrentBag<string> {
            ":dog:", ":cat:", ":mouse:", ":hamster:", ":rabbit:", ":bear:", ":pig:", ":cow:", ":koala:", ":tiger:"
        };
        private DiscordClient _client;
        private ulong _cid;
        #endregion


        public Race(DiscordClient client, ulong cid)
        {
            _client = client;
            _cid = cid;
            _channels.Add(_cid);
            GameRunning = false;
        }


        public async Task StartRaceAsync()
        {
            GameRunning = true;

            var chn = await _client.GetChannelAsync(_cid)
                .ConfigureAwait(false);

            foreach (var p in _participants)
                _progress.Add(p, 0);

            var msg = await chn.SendMessageAsync("Race starting...")
                .ConfigureAwait(false);
            var rnd = new Random((int)DateTime.Now.Ticks);
            while (!_progress.Any(e => e.Value >= 100)) {
                await PrintRaceAsync(msg)
                    .ConfigureAwait(false);

                foreach (var id in _participants) {
                    _progress[id] += rnd.Next(2, 7);
                    if (_progress[id] > 100)
                        _progress[id] = 100;
                }

                await Task.Delay(2000)
                    .ConfigureAwait(false);
            }
            await PrintRaceAsync(msg)
                .ConfigureAwait(false);

            await chn.SendMessageAsync("Race ended!")
                .ConfigureAwait(false);

        }

        public void StopRace()
        {
            _channels.TryRemove(_cid);
            GameRunning = false;
        }

        public DiscordEmoji AddParticipant(ulong uid)
        {
            if (_participants.Contains(uid))
                return null;

            string emoji;
            if (!_animals.TryTake(out emoji))
                return null;
            _participants.Enqueue(uid);
            if (!_emojis.TryAdd(uid, emoji))
                return null;

            return DiscordEmoji.FromName(_client, emoji);
        }

        private async Task PrintRaceAsync(DiscordMessage msg)
        {
            string s = "LIVE RACING BROADCAST\n| 🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🔚\n";
            foreach (var id in _participants) {
                var participant = await _client.GetUserAsync(id)
                    .ConfigureAwait(false);
                s += "|";
                for (int p = _progress[id]; p > 0; p--)
                    s += "‣";
                s += DiscordEmoji.FromName(_client, _emojis[id]);
                for (int p = 100 - _progress[id]; p > 0; p--)
                    s += "‣";
                s += "| " + participant.Mention;
                if (_progress[id] == 100)
                    s += " " + DiscordEmoji.FromName(_client, ":trophy:");
                s += '\n';
            }
            await msg.ModifyAsync(s)
                .ConfigureAwait(false);
        }
    }
}
