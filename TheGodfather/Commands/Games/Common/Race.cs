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
        #region PUBLIC_FIELDS
        public bool GameRunning { get; private set; }
        public int ParticipantCount => _participants.Count;
        public IEnumerable<ulong> WinnerIds { get; private set; }
        #endregion

        #region PRIVATE_FIELDS
        private ConcurrentQueue<RaceParticipant> _participants = new ConcurrentQueue<RaceParticipant>();
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
            GameRunning = false;
        }


        public async Task StartRaceAsync()
        {
            GameRunning = true;

            var chn = await _client.GetChannelAsync(_cid)
                .ConfigureAwait(false);

            var msg = await chn.SendMessageAsync("Race starting...")
                .ConfigureAwait(false);
            var rnd = new Random((int)DateTime.Now.Ticks);
            while (!_participants.Any(p => p.Progress >= 100)) {
                await PrintRaceAsync(msg)
                    .ConfigureAwait(false);

                foreach (var participant in _participants) {
                    participant.Progress += rnd.Next(2, 7);
                    if (participant.Progress > 100)
                        participant.Progress = 100;
                }

                await Task.Delay(2000)
                    .ConfigureAwait(false);
            }

            await PrintRaceAsync(msg)
                .ConfigureAwait(false);

            WinnerIds = _participants.Where(p => p.Progress >= 100).Select(p => p.Id);
        }

        public DiscordEmoji AddParticipant(ulong uid)
        {
            if (_participants.Any(p => p.Id == uid))
                return null;

            string emoji;
            if (!_animals.TryTake(out emoji))
                return null;
            _participants.Enqueue(new RaceParticipant {
                Id = uid,
                Emoji = emoji,
                Progress = 0
            });

            return DiscordEmoji.FromName(_client, emoji);
        }

        private async Task PrintRaceAsync(DiscordMessage msg)
        {
            StringBuilder sb = new StringBuilder("LIVE RACING BROADCAST\n🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🔚\n");
            foreach (var participant in _participants) {
                var u = await _client.GetUserAsync(participant.Id)
                    .ConfigureAwait(false);
                sb.Append("|");
                sb.Append(new string('‣', participant.Progress));
                sb.Append(DiscordEmoji.FromName(_client, participant.Emoji));
                sb.Append(new string('‣', 100 - participant.Progress));
                sb.Append("| " + u.Mention);
                if (participant.Progress == 100)
                    sb.Append(" " + DiscordEmoji.FromName(_client, ":trophy:"));
                sb.Append('\n');
            }
            await msg.ModifyAsync(sb.ToString())
                .ConfigureAwait(false);
        }
        

        private sealed class RaceParticipant
        {
            public ulong Id { get; internal set; }
            public int Progress { get; internal set; }
            public string Emoji { get; internal set; }
        }
    }
}
