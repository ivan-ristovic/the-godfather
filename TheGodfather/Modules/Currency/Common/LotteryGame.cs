#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Currency.Common
{
    public class LotteryGame : ChannelEvent
    {
        public static readonly int MaxNumber = 15;
        public static readonly int DrawCount = 3;
        public static readonly int TicketPrice = 250;
        public static readonly ImmutableArray<int> Prizes = new int[] {
            0, 2500, 50000, 1000000
        }.ToImmutableArray();

        public bool Started { get; private set; }
        public int ParticipantCount => _participants.Count;
        public IReadOnlyList<LotteryParticipant> Winners => _participants.Where(p => p.WinAmount > 0).ToList().AsReadOnly();

        private ConcurrentQueue<LotteryParticipant> _participants = new ConcurrentQueue<LotteryParticipant>();
       


        public LotteryGame(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            Started = false;
        }


        public override async Task RunAsync()
        {
            Started = true;

            var msg = await Channel.SendIconEmbedAsync("Drawing lottery numbers in 5s...", StaticDiscordEmoji.MoneyBag)
                .ConfigureAwait(false);

            var drawn = Enumerable.Range(1, MaxNumber + 1).Shuffle().Take(3);

            for (int i = 0; i < DrawCount; i++) {
                await Task.Delay(TimeSpan.FromSeconds(5))
                    .ConfigureAwait(false);
                await PrintGameAsync(msg, drawn, i + 1)
                    .ConfigureAwait(false);
            }

            foreach (var participant in _participants) {
                int guessed = participant.Numbers.Intersect(drawn).Count();
                participant.WinAmount = Prizes[guessed];
            }
        }

        public void AddParticipant(DiscordUser user, int[] numbers)
        {
            if (IsParticipating(user))
                return;

            _participants.Enqueue(new LotteryParticipant {
                User = user,
                Numbers = numbers
            });
        }

        public bool IsParticipating(DiscordUser user) => _participants.Any(p => p.Id == user.Id);

        private async Task PrintGameAsync(DiscordMessage msg, IEnumerable<int> numbers, int step)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Formatter.Bold($"Drawn numbers:"));
            sb.AppendLine(Formatter.Bold(string.Join(" ", numbers.Take(step)))).AppendLine();

            foreach (var participant in _participants) {
                sb.Append(participant.User.Mention).Append(" | ");
                sb.AppendLine(Formatter.Bold(string.Join(" ", participant.Numbers)));
                sb.AppendLine();
            }

            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.MoneyBag} LOTTERY DRAWS {StaticDiscordEmoji.MoneyBag}",
                Description = sb.ToString(),
                Color = DiscordColor.Gold
            };

            await msg.ModifyAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }


        public sealed class LotteryParticipant
        {
            public DiscordUser User { get; internal set; }
            public int Bid { get; set; }
            public ulong Id => User.Id;
            public int[] Numbers { get; set; }
            public int WinAmount { get; set; } = 0;
        }
    }
}
