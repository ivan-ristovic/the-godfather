#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TexasHoldem.Logic.Cards;
using TheGodfather.Common;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Currency.Common
{
    public sealed class BlackjackParticipant
    {
        public int Bid { get; set; }
        public List<Card> Hand { get; internal set; } = new List<Card>();
        public DiscordUser User { get; internal set; }
        public bool Standing { get; set; } = false;
        public ulong Id => this.User.Id;
    }

    public sealed class BlackjackGame : ChannelEvent
    {
        public bool Started { get; private set; }
        public int ParticipantCount => this.participants.Count;
        public IReadOnlyList<BlackjackParticipant> Winners
        {
            get {
                int hvalue = HandValue(this.hand);
                if (hvalue > 21)
                    return this.participants.Where(p => HandValue(p.Hand) <= 21).ToList().AsReadOnly();
                return this.participants.Where(p => {
                    int value = HandValue(p.Hand);
                    if (value > 21)
                        return false;
                    return value > hvalue;
                }).ToList().AsReadOnly();
            }
        }

        private bool GameOver;
        private readonly Deck deck;
        private readonly List<Card> hand;
        private readonly ConcurrentQueue<BlackjackParticipant> participants;


        public BlackjackGame(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            this.Started = false;
            this.GameOver = false;
            this.deck = new Deck();
            this.hand = new List<Card>();
            this.participants = new ConcurrentQueue<BlackjackParticipant>();
        }


        public override async Task RunAsync()
        {
            this.Started = true;

            DiscordMessage msg = await this.Channel.EmbedAsync("Starting blackjack game...");

            foreach (BlackjackParticipant participant in this.participants) {
                participant.Hand.Add(this.deck.GetNextCard());
                participant.Hand.Add(this.deck.GetNextCard());
                if (HandValue(participant.Hand) == 21) {
                    this.GameOver = true;
                    this.Winner = participant.User;
                    break;
                }
            }

            if (this.GameOver) {
                await PrintGameAsync(msg);
                return;
            }
            
            while (this.participants.Any(p => !p.Standing)) {
                foreach (BlackjackParticipant participant in this.participants) {
                    if (participant.Standing)
                        continue;

                    await PrintGameAsync(msg, participant);

                    if (await this.Interactivity.WaitForBoolReplyAsync(this.Channel.Id, participant.Id)) 
                        participant.Hand.Add(this.deck.GetNextCard());
                    else 
                        participant.Standing = true;

                    if (HandValue(participant.Hand) > 21)
                        participant.Standing = true;
                }
            }

            await PrintGameAsync(msg);
            await Task.Delay(TimeSpan.FromSeconds(5));

            while (HandValue(this.hand) <= 17)
                this.hand.Add(this.deck.GetNextCard());

            if (this.hand.Count == 2 && HandValue(this.hand) == 21)
                await this.Channel.EmbedAsync("BLACKJACK!");

            this.GameOver = true;
            await PrintGameAsync(msg);
        }

        public void AddParticipant(DiscordUser user, int bid)
        {
            if (IsParticipating(user))
                return;

            this.participants.Enqueue(new BlackjackParticipant {
                User = user,
                Bid = bid
            });
        }

        public bool IsParticipating(DiscordUser user) 
            => this.participants.Any(p => p.Id == user.Id);

        private int HandValue(List<Card> hand)
        {
            int value = 0;
            bool one = false;
            foreach (Card card in hand) {
                if (card.Type >= CardType.Ten) {
                    value += 10;
                } else if (card.Type == CardType.Ace) {
                    value += 1;
                    one = true;
                } else {
                    value += (int)card.Type;
                }
            }

            if (one && value <= 11)
                value += 10;

            return value;
        }

        private Task PrintGameAsync(DiscordMessage msg, BlackjackParticipant tomove = null)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Formatter.Bold($"House hand: (value: {HandValue(this.hand)})")).AppendLine();
            if (this.hand.Any())
                sb.AppendLine(string.Join(" | ", this.hand)).AppendLine();
            else
                sb.AppendLine(StaticDiscordEmoji.Question).AppendLine();

            foreach (BlackjackParticipant participant in this.participants) {
                sb.Append(participant.User.Mention);
                sb.Append(" (value: ");
                sb.Append(Formatter.Bold(HandValue(participant.Hand).ToString()));
                sb.AppendLine(")").AppendLine();
                sb.AppendLine(string.Join(" | ", participant.Hand));
                sb.AppendLine();
            }

            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.CardSuits[0]} BLACKJACK GAME STATE {StaticDiscordEmoji.CardSuits[0]}",
                Description = sb.ToString(),
                Color = DiscordColor.Red
            };

            if (!this.GameOver)
                emb.AddField("Deciding whether to hit a card (type yes/no):", tomove?.User.Mention ?? "House");

            return msg.ModifyAsync(embed: emb.Build());
        }
    }
}
