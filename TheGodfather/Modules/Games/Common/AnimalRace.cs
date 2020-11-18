#region USING_DIRECTIVES
using DSharpPlus.Entities;
using DSharpPlus.Interactivity; using DSharpPlus.Interactivity.Extensions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class AnimalRace : ChannelEvent
    {
        private sealed class AnimalRaceParticipant
        {
            public DiscordEmoji Emoji { get; internal set; }
            public DiscordUser User { get; internal set; }
            public int Progress { get; internal set; }
            public ulong Id => this.User.Id;
        }

        private static readonly int _TrackSize = 50;
        
        public bool Started { get; private set; }
        public IReadOnlyList<ulong> WinnerIds { get; private set; }
        public int ParticipantCount => this.participants.Count;

        private readonly ConcurrentQueue<AnimalRaceParticipant> participants;
        private readonly ConcurrentBag<DiscordEmoji> animals;


        public AnimalRace(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            this.Started = false;
            this.participants = new ConcurrentQueue<AnimalRaceParticipant>();
            this.animals = new ConcurrentBag<DiscordEmoji>(StaticDiscordEmoji.Animals);
        }


        public override async Task RunAsync()
        {
            this.Started = true;

            DiscordMessage msg = await this.Channel.EmbedAsync("Race starting...");
            while (this.participants.All(p => p.Progress < _TrackSize)) {
                await this.PrintRaceAsync(msg);

                foreach (AnimalRaceParticipant participant in this.participants) {
                    participant.Progress += GFRandom.Generator.Next(2, 7);
                    if (participant.Progress > _TrackSize)
                        participant.Progress = _TrackSize;
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            await this.PrintRaceAsync(msg);

            this.WinnerIds = this.participants
                .Where(p => p.Progress >= _TrackSize)
                .Select(p => p.Id)
                .ToList()
                .AsReadOnly();
        }

        public bool AddParticipant(DiscordUser user, out DiscordEmoji emoji)
        {
            emoji = null;
            if (this.participants.Any(p => p.Id == user.Id))
                return false;

            if (!this.animals.TryTake(out emoji))
                return false;

            this.participants.Enqueue(new AnimalRaceParticipant {
                User = user,
                Emoji = emoji,
                Progress = 0
            });
            return true;
        }

        private Task PrintRaceAsync(DiscordMessage msg)
        {
            var sb = new StringBuilder("🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🏁🔚\n");
            foreach (AnimalRaceParticipant participant in this.participants) {
                sb.Append("|");
                sb.Append('‣', participant.Progress);
                sb.Append(participant.Emoji);
                sb.Append('‣', _TrackSize - participant.Progress);
                sb.Append("| " + participant.User.Mention);
                if (participant.Progress == _TrackSize)
                    sb.Append(" " + StaticDiscordEmoji.Trophy);
                sb.AppendLine();
            }

            return msg.ModifyAsync(embed: new DiscordEmbedBuilder {
                Title = "LIVE RACING BROADCAST",
                Description = sb.ToString()
            }.Build());
        }
    }
}
