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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Gambling.Common
{
    public class LotteryGame : Game
    {
        public static readonly int MaxNumber = 10;
        public static readonly int Jackpot = 1000000;

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

            var msg = await _channel.SendIconEmbedAsync("Drawing numbers in 5s...", StaticDiscordEmoji.MoneyBag)
                .ConfigureAwait(false);

            var drawn = new List<int>();

            await Task.Delay(TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);

            drawn.Add(GFRandom.Generator.Next(10));
            await PrintGameAsync(msg, drawn)
                .ConfigureAwait(false);

            await Task.Delay(TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);

            drawn.Add(GFRandom.Generator.Next(10));
            await PrintGameAsync(msg, drawn)
                .ConfigureAwait(false);

            foreach (var participant in _participants) {
                int won = 1;
                if (drawn.Contains(participant.Number1))
                    won *= 1000;
                if (drawn.Contains(participant.Number2))
                    won *= 1000;
                participant.WinAmount = won != 1 ? won : 0;
            }
        }

        public void AddParticipant(DiscordUser user, int num1, int num2)
        {
            if (IsParticipating(user))
                return;

            _participants.Enqueue(new LotteryParticipant {
                User = user,
                Number1 = num1 - 1,
                Number2 = num2 - 1
            });
        }

        public bool IsParticipating(DiscordUser user) => _participants.Any(p => p.Id == user.Id);

        private async Task PrintGameAsync(DiscordMessage msg, List<int> numbers)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Formatter.Bold($"Drawn numbers:"));
            sb.AppendLine(string.Join(" ", numbers.Select(n => StaticDiscordEmoji.Numbers[n]))).AppendLine();

            foreach (var participant in _participants) {
                sb.AppendLine(participant.User.Mention);
                sb.Append(StaticDiscordEmoji.Numbers[participant.Number1]);
                sb.Append(" ");
                sb.Append(StaticDiscordEmoji.Numbers[participant.Number2]);
                sb.AppendLine().AppendLine();
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
            public int Number1 { get; set; }
            public int Number2 { get; set; }
            public int WinAmount { get; set; } = 0;
        }
    }
}
