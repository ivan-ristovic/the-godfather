using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games.Common
{
    public sealed class AnimalRace : BaseChannelGame
    {
        public const int MaxParticipants = 10;
        private const int TrackSize = 50;
        private const int StepDelay = 2;

        public bool Started { get; private set; }
        public IReadOnlyList<ulong>? WinnerIds { get; private set; }
        public int ParticipantCount => this.participants.Count;

        private readonly ConcurrentQueue<AnimalRaceParticipant> participants;
        private readonly ConcurrentBag<DiscordEmoji> animals;


        public AnimalRace(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            this.Started = false;
            this.participants = new ConcurrentQueue<AnimalRaceParticipant>();
            this.animals = new ConcurrentBag<DiscordEmoji>(Emojis.Animals.All);
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            this.Started = true;
            DiscordMessage msg = await this.Channel.EmbedAsync(lcs.GetString(this.Channel.GuildId, TranslationKey.str_game_ar_starting));

            var rng = new SecureRandom();
            while (this.participants.All(p => p.Progress < TrackSize)) {
                await this.PrintRaceAsync(msg);

                foreach (AnimalRaceParticipant participant in this.participants) {
                    participant.Progress += rng.Next(2, 7);
                    if (participant.Progress > TrackSize)
                        participant.Progress = TrackSize;
                }

                await Task.Delay(TimeSpan.FromSeconds(StepDelay));
            }

            await this.PrintRaceAsync(msg);

            this.WinnerIds = this.participants
                .Where(p => p.Progress >= TrackSize)
                .Select(p => p.Id)
                .ToList()
                .AsReadOnly();
        }

        public bool AddParticipant(DiscordUser user, out DiscordEmoji? emoji)
        {
            emoji = null;
            if (this.participants.Count >= MaxParticipants || this.participants.Any(p => p.Id == user.Id))
                return false;

            if (!this.animals.TryTake(out emoji))
                return false;

            this.participants.Enqueue(new AnimalRaceParticipant(user, emoji));
            return true;
        }


        private Task PrintRaceAsync(DiscordMessage msg)
        {
            var sb = new StringBuilder("⏩⏩⏩⏩⏩⏩⏩⏩⏩⏩⏩⏩🏁\n");
            foreach (AnimalRaceParticipant participant in this.participants) {
                sb.Append('|');
                sb.Append('‣', participant.Progress);
                sb.Append(participant.Emoji);
                sb.Append('‣', TrackSize - participant.Progress);
                sb.Append("| " + participant.User.Mention);
                if (participant.Progress == TrackSize)
                    sb.Append(" " + Emojis.Trophy);
                sb.AppendLine();
            }

            return msg.ModifyOrResendAsync(this.Channel, new DiscordEmbedBuilder {
                Description = sb.ToString()
            }.Build());
        }


        private sealed class AnimalRaceParticipant
        {
            public DiscordEmoji Emoji { get; }
            public DiscordUser User { get; }
            public int Progress { get; internal set; }
            public ulong Id => this.User.Id;


            public AnimalRaceParticipant(DiscordUser user, DiscordEmoji emoji)
            {
                this.User = user;
                this.Emoji = emoji;
            }
        }
    }
}
