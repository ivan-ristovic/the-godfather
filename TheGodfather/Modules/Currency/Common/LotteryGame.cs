#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Currency.Common
{
    public class LotteryGame : BaseChannelGame
    {
        public static readonly int MaxNumber = 15;
        public static readonly int DrawCount = 3;
        public static readonly int TicketPrice = 250;
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


        public override async Task RunAsync()
        {
            this.Started = true;

            DiscordMessage msg = await this.Channel.EmbedAsync("Drawing lottery numbers in 5s...", Emojis.MoneyBag);

            IEnumerable<int> drawn = Enumerable.Range(1, MaxNumber + 1).Shuffle().Take(3);

            for (int i = 0; i < DrawCount; i++) {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await this.PrintGameAsync(msg, drawn, i + 1);
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

            this.participants.Enqueue(new Participant {
                User = user,
                Numbers = numbers
            });
        }

        public bool IsParticipating(DiscordUser user) 
            => this.participants.Any(p => p.Id == user.Id);

        private Task PrintGameAsync(DiscordMessage msg, IEnumerable<int> numbers, int step)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Formatter.Bold($"Drawn numbers:"));
            sb.AppendLine(Formatter.Bold(string.Join(" ", numbers.Take(step)))).AppendLine();

            foreach (Participant participant in this.participants) {
                sb.Append(participant.User.Mention).Append(" | ");
                sb.AppendLine(Formatter.Bold(string.Join(" ", participant.Numbers)));
                sb.AppendLine();
            }

            var emb = new DiscordEmbedBuilder {
                Title = $"{Emojis.MoneyBag} LOTTERY DRAWS {Emojis.MoneyBag}",
                Description = sb.ToString(),
                Color = DiscordColor.DarkGreen
            };

            return msg.ModifyAsync(embed: emb.Build());
        }


        public sealed class Participant
        {
            public DiscordUser User { get; internal set; }
            public int Bid { get; set; }
            public ulong Id => this.User.Id;
            public int[] Numbers { get; set; }
            public int WinAmount { get; set; }
        }
    }
}
