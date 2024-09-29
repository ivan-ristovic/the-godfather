using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Currency.Common;

public class LotteryGame : BaseChannelGame
{
    public const int MaxParticipants = 10;
    public const int MaxNumber = 15;
    public const int DrawCount = 3;
    public const int TicketPrice = 250;
    public static readonly ImmutableArray<int> Prizes = new[] {
        0, 2500, 50000, 1000000,
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

        DiscordMessage msg = await this.Channel.EmbedAsync(lcs.GetString(this.Channel.GuildId, TranslationKey.str_casino_lottery_starting));

        var drawn = Enumerable.Range(1, MaxNumber + 1).Shuffle().Take(3).ToList();

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
        if (!this.IsParticipating(user))
            this.participants.Enqueue(new Participant(user, numbers));
    }

    public bool IsParticipating(DiscordUser user)
        => this.participants.Any(p => p.Id == user.Id);

    private Task PrintGameAsync(LocalizationService lcs, DiscordMessage msg, IEnumerable<int> numbers, int step)
    {
        var sb = new StringBuilder();

        sb.Append(Formatter.Bold(lcs.GetString(this.Channel.GuildId, TranslationKey.str_casino_lottery_drawn))).Append(' ');
        sb.AppendLine(Formatter.Bold(numbers.Take(step).JoinWith(" "))).AppendLine();

        foreach (Participant participant in this.participants) {
            sb.Append(participant.User.Mention).Append(" | ");
            sb.AppendLine(Formatter.Bold(participant.Numbers.JoinWith(" ")));
            sb.AppendLine();
        }

        var emb = new LocalizedEmbedBuilder(lcs, this.Channel.GuildId);
        emb.WithLocalizedTitle(TranslationKey.fmt_casino_lottery(Emojis.MoneyBag, Emojis.MoneyBag));
        emb.WithColor(DiscordColor.DarkGreen);
        emb.WithDescription(sb);

        return msg.ModifyAsync(emb.Build());
    }


    public sealed class Participant(DiscordUser user, int[] numbers)
    {
        public DiscordUser User { get; } = user;
        public IEnumerable<int> Numbers { get; } = numbers.ToImmutableSortedSet();
        public ulong Id => this.User.Id;
        public int WinAmount { get; set; }
    }
}