#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TexasHoldem.Logic.Cards;
using TheGodfather.Common;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Currency.Common
{
    public sealed class BlackjackGame : BaseChannelGame
    {
        public bool Started { get; private set; }
        public int ParticipantCount => this.participants.Count;
        public IReadOnlyList<Participant> Winners {
            get {
                int hvalue = this.HandValue(this.hand);
                if (hvalue > 21)
                    return this.participants.Where(p => this.HandValue(p.Hand) <= 21).ToList().AsReadOnly();
                return this.participants.Where(p => {
                    int value = this.HandValue(p.Hand);
                    if (value > 21)
                        return false;
                    return value > hvalue;
                }).ToList().AsReadOnly();
            }
        }

        private bool GameOver;
        private readonly Deck deck;
        private readonly List<Card> hand;
        private readonly ConcurrentQueue<Participant> participants;


        public BlackjackGame(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            this.Started = false;
            this.GameOver = false;
            this.deck = new Deck();
            this.hand = new List<Card>();
            this.participants = new ConcurrentQueue<Participant>();
        }


        public override async Task RunAsync()
        {
            this.Started = true;

            DiscordMessage msg = await this.Channel.EmbedAsync("Starting blackjack game...");

            foreach (Participant participant in this.participants) {
                participant.Hand.Add(this.deck.GetNextCard());
                participant.Hand.Add(this.deck.GetNextCard());
                if (this.HandValue(participant.Hand) == 21) {
                    this.GameOver = true;
                    this.Winner = participant.User;
                    break;
                }
            }

            if (this.GameOver) {
                await this.PrintGameAsync(msg);
                return;
            }

            while (this.participants.Any(p => !p.Standing)) {
                foreach (Participant participant in this.participants) {
                    if (participant.Standing)
                        continue;

                    await this.PrintGameAsync(msg, participant);

                    if (await this.Interactivity.WaitForBoolReplyAsync(this.Channel.Id, participant.Id))
                        participant.Hand.Add(this.deck.GetNextCard());
                    else
                        participant.Standing = true;

                    if (this.HandValue(participant.Hand) > 21)
                        participant.Standing = true;
                }
            }

            await this.PrintGameAsync(msg);
            await Task.Delay(TimeSpan.FromSeconds(5));

            while (this.HandValue(this.hand) <= 17)
                this.hand.Add(this.deck.GetNextCard());

            if (this.hand.Count == 2 && this.HandValue(this.hand) == 21)
                await this.Channel.EmbedAsync("BLACKJACK!");

            this.GameOver = true;
            await this.PrintGameAsync(msg);
        }

        public void AddParticipant(DiscordUser user, int bid)
        {
            if (this.IsParticipating(user))
                return;

            this.participants.Enqueue(new Participant {
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

        private Task PrintGameAsync(DiscordMessage msg, Participant tomove = null)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Formatter.Bold($"House hand: (value: {this.HandValue(this.hand)})")).AppendLine();
            if (this.hand.Any())
                sb.AppendJoin(" | ", this.hand).AppendLine();
            else
                sb.AppendLine(Emojis.Question).AppendLine();

            foreach (Participant participant in this.participants) {
                sb.Append(participant.User.Mention);
                sb.Append(" (value: ");
                sb.Append(Formatter.Bold(this.HandValue(participant.Hand).ToString()));
                sb.AppendLine(")").AppendLine();
                sb.AppendJoin(" | ", participant.Hand);
                sb.AppendLine();
            }

            var emb = new DiscordEmbedBuilder {
                Title = $"{Emojis.Cards.Suits[0]} BLACKJACK GAME STATE {Emojis.Cards.Suits[0]}",
                Description = sb.ToString(),
                Color = DiscordColor.DarkGreen
            };

            if (!this.GameOver)
                emb.AddField("Deciding whether to hit a card (type yes/no):", tomove?.User.Mention ?? "House");

            return msg.ModifyAsync(embed: emb.Build());
        }


        public sealed class Participant
        {
            public int Bid { get; set; }
            public List<Card> Hand { get; internal set; } = new List<Card>();
            public DiscordUser User { get; internal set; }
            public bool Standing { get; set; } = false;
            public ulong Id => this.User.Id;
        }
    }
}
