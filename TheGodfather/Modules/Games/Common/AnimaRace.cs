#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class AnimalRace : Game
    {
        private static readonly int TRACK_SIZE = 50;

        #region PUBLIC_FIELDS
        public bool Started { get; private set; }
        public int ParticipantCount => _participants.Count;
        public IEnumerable<ulong> WinnerIds { get; private set; }
        #endregion

        #region PRIVATE_FIELDS
        private ConcurrentQueue<AnimalRaceParticipant> _participants = new ConcurrentQueue<AnimalRaceParticipant>();
        private ConcurrentBag<DiscordEmoji> _animals = new ConcurrentBag<DiscordEmoji>(StaticDiscordEmoji.Animals);
        #endregion


        public AnimalRace(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            Started = false;
        }


        public override async Task RunAsync()
        {
            Started = true;

            var msg = await _channel.SendIconEmbedAsync("Race starting...")
                .ConfigureAwait(false);
            while (!_participants.Any(p => p.Progress >= TRACK_SIZE)) {
                await PrintRaceAsync(msg)
                    .ConfigureAwait(false);

                foreach (var participant in _participants) {
                    participant.Progress += GFRandom.Generator.Next(2, 7);
                    if (participant.Progress > TRACK_SIZE)
                        participant.Progress = TRACK_SIZE;
                }

                await Task.Delay(TimeSpan.FromSeconds(2))
                    .ConfigureAwait(false);
            }

            await PrintRaceAsync(msg)
                .ConfigureAwait(false);

            WinnerIds = _participants.Where(p => p.Progress >= TRACK_SIZE).Select(p => p.Id);
        }

        public bool AddParticipant(DiscordUser user, out DiscordEmoji emoji)
        {
            emoji = null;
            if (_participants.Any(p => p.Id == user.Id))
                return false;

            if (!_animals.TryTake(out emoji))
                return false;

            _participants.Enqueue(new AnimalRaceParticipant {
                User = user,
                Emoji = emoji,
                Progress = 0
            });
            return true;
        }

        private async Task PrintRaceAsync(DiscordMessage msg)
        {
            StringBuilder sb = new StringBuilder("🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🔚\n");
            foreach (var participant in _participants) {
                sb.Append("|");
                sb.Append('‣', participant.Progress);
                sb.Append(participant.Emoji);
                sb.Append('‣', TRACK_SIZE - participant.Progress);
                sb.Append("| " + participant.User.Mention);
                if (participant.Progress == TRACK_SIZE)
                    sb.Append(" " + StaticDiscordEmoji.Trophy);
                sb.AppendLine();
            }
            await msg.ModifyAsync(embed: new DiscordEmbedBuilder() {
                Title = "LIVE RACING BROADCAST",
                Description = sb.ToString()
            }.Build()).ConfigureAwait(false);
        }
        

        private sealed class AnimalRaceParticipant
        {
            public DiscordUser User { get; internal set; }
            public int Progress { get; internal set; }
            public DiscordEmoji Emoji { get; internal set; }
            public ulong Id => User.Id;
        }
    }
}
