using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Currency.Common
{
    public class LotteryGame : BaseChannelGame
    {
        public const int MaxParticipants = 10;
        public const int MaxNumber = 15;
        public const int DrawCount = 3;
        public const int TicketPrice = 250;
        public static readonly ImmutableArray<int> Prizes = new[] {
            0, 2500, 50000, 1000000
        }.ToImmutableArray();

        public bool Started { get; private set; }
        public int ParticipantCount => this.participants.Count;
        public IReadOnlyList<Participant> Winners
            => this.participants.Where(p => p.WinAmount > 0).ToList().AsReadOnly();

        private readonly ConcurrentQueue<Participant> participants;


        public LotteryGame(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            this.Started = false;
            this.participants = new ConcurrentQueue<Participant>();
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            this.Started = true;

            DiscordMessage msg = await this.Channel.EmbedAsync(lcs.GetString(this.Channel.GuildId, "str-casino-lottery-starting"));

            IEnumerable<int> drawn = Enumerable.Range(1, MaxNumber + 1).Shuffle().Take(3);

            for (int i = 0; i < DrawCount; i++) {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await this.PrintGameAsync(lcs, msg, drawn, i + 1);
            }

            foreach (Participant participant in this.participants) {
                int guessed = participant.Numbers.Intersect(drawn).Count();
                participant.WinAmount = Prizes[guessed];
            }
        }

        public void AddParticipant(DiscordUser user, int[] numbers)
        {
            if (this.IsParticipating(user))
                return;
            this.participants.Enqueue(new Participant(user, numbers));
        }

        public bool IsParticipating(DiscordUser user)
            => this.participants.Any(p => p.Id == user.Id);

        private Task PrintGameAsync(LocalizationService lcs, DiscordMessage msg, IEnumerable<int> numbers, int step)
        {
            var sb = new StringBuilder();

            sb.Append(Formatter.Bold(lcs.GetString(this.Channel.GuildId, "str-casino-lottery-drawn"))).Append(' ');
            sb.AppendLine(Formatter.Bold(numbers.Take(step).JoinWith(" "))).AppendLine();

            foreach (Participant participant in this.participants) {
                sb.Append(participant.User.Mention).Append(" | ");
                sb.AppendLine(Formatter.Bold(participant.Numbers.JoinWith(" ")));
                sb.AppendLine();
            }

            var emb = new LocalizedEmbedBuilder(lcs, this.Channel.GuildId);
            emb.WithLocalizedTitle("fmt-casino-lottery", Emojis.MoneyBag, Emojis.MoneyBag);
            emb.WithColor(DiscordColor.DarkGreen);
            emb.WithDescription(sb);

            return msg.ModifyAsync(embed: emb.Build());
        }


        public sealed class Participant
        {
            public DiscordUser User { get; }
            public int[] Numbers { get; }
            public ulong Id => this.User.Id;
            public int WinAmount { get; set; }

            public Participant(DiscordUser user, int[] numbers)
            {
                this.User = user;
                this.Numbers = numbers;
            }
        }
    }
}
