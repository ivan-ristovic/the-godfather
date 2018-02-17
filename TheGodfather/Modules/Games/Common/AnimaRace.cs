#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Modules.Games.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games
{
    public class AnimalRace : Game
    {
        #region PUBLIC_FIELDS
        public bool Started { get; private set; }
        public int ParticipantCount => _participants.Count;
        public IEnumerable<ulong> WinnerIds { get; private set; }
        #endregion

        #region PRIVATE_FIELDS
        private ConcurrentQueue<RaceParticipant> _participants = new ConcurrentQueue<RaceParticipant>();
        private ConcurrentBag<string> _animals = new ConcurrentBag<string> {
            ":dog:", ":cat:", ":mouse:", ":hamster:", ":rabbit:", ":bear:", ":pig:", ":cow:", ":koala:", ":tiger:"
        };
        private DiscordClient _client { get; }
        #endregion


        public AnimalRace(DiscordClient client, InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            _client = client;
            Started = false;
        }


        public override async Task RunAsync()
        {
            Started = true;

            var msg = await _channel.SendMessageAsync("Race starting...")
                .ConfigureAwait(false);
            var rnd = new Random();
            while (!_participants.Any(p => p.Progress >= 100)) {
                await PrintRaceAsync(msg)
                    .ConfigureAwait(false);

                foreach (var participant in _participants) {
                    participant.Progress += rnd.Next(2, 7);
                    if (participant.Progress > 100)
                        participant.Progress = 100;
                }

                await Task.Delay(TimeSpan.FromSeconds(2))
                    .ConfigureAwait(false);
            }

            await PrintRaceAsync(msg)
                .ConfigureAwait(false);

            WinnerIds = _participants.Where(p => p.Progress >= 100).Select(p => p.Id);
        }

        public bool AddParticipant(ulong uid, out DiscordEmoji emoji)
        {
            emoji = null;
            if (_participants.Any(p => p.Id == uid))
                return false;

            if (!_animals.TryTake(out string emojistr))
                return false;
            _participants.Enqueue(new RaceParticipant {
                Id = uid,
                Emoji = emojistr,
                Progress = 0
            });

            emoji = DiscordEmoji.FromName(_client, emojistr);
            return true;
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
                sb.AppendLine();
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
