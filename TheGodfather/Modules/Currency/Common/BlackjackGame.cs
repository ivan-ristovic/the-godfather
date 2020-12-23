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
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Currency.Common
{
    public sealed class BlackjackGame : BaseChannelGame
    {
        public const int InitialBid = 5;
        public const int MaxParticipants = 5;

        public bool Started { get; private set; }
        public int ParticipantCount => this.participants.Count;
        public IReadOnlyList<Participant> Winners {
            get {
                int hvalue = this.HandValue(this.hand);
                if (hvalue > 21)
                    return this.participants.Where(p => this.HandValue(p.Hand) <= 21).ToList().AsReadOnly();
                return this.participants.Where(p => {
                    int value = this.HandValue(p.Hand);
                    return value <= 21 && value > hvalue;
                }).ToList().AsReadOnly();
            }
        }

        private bool gameOver;
        private readonly Deck deck;
        private readonly List<Card> hand;
        private readonly ConcurrentQueue<Participant> participants;


        public BlackjackGame(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            this.Started = false;
            this.gameOver = false;
            this.deck = new Deck();
            this.hand = new List<Card>();
            this.participants = new ConcurrentQueue<Participant>();
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            this.Started = true;

            DiscordMessage msg = await this.Channel.EmbedAsync(lcs.GetString(this.Channel.GuildId, "str-casino-blackjack-starting"));

            foreach (Participant participant in this.participants) {
                participant.Hand.Add(this.deck.GetNextCard());
                participant.Hand.Add(this.deck.GetNextCard());
                if (this.HandValue(participant.Hand) == 21) {
                    this.gameOver = true;
                    this.Winner = participant.User;
                    break;
                }
            }

            if (this.gameOver) {
                await this.PrintGameAsync(lcs, msg);
                return;
            }

            while (this.participants.Any(p => !p.IsStanding)) {
                foreach (Participant participant in this.participants.Where(p => !p.IsStanding)) {
                    await this.PrintGameAsync(lcs, msg, participant);

                    if (await this.Interactivity.WaitForBoolReplyAsync(this.Channel, participant.User))
                        participant.Hand.Add(this.deck.GetNextCard());
                    else
                        participant.IsStanding = true;

                    if (this.HandValue(participant.Hand) > 21)
                        participant.IsStanding = true;
                }
            }

            await this.PrintGameAsync(lcs, msg);
            await Task.Delay(TimeSpan.FromSeconds(5));

            while (this.HandValue(this.hand) <= 17)
                this.hand.Add(this.deck.GetNextCard());

            if (this.hand.Count == 2 && this.HandValue(this.hand) == 21)
                await this.Channel.EmbedAsync("BLACKJACK!");

            this.gameOver = true;
            await this.PrintGameAsync(lcs, msg);
        }

        public void AddParticipant(DiscordUser user, int bid)
        {
            if (this.IsParticipating(user))
                return;

            this.participants.Enqueue(new Participant(user, bid));
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


        private Task PrintGameAsync(LocalizationService lcs, DiscordMessage msg, Participant? toMove = null)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Formatter.Bold($"{lcs.GetString(this.Channel.GuildId, "str-house")}: {this.HandValue(this.hand)}"));
            if (this.hand.Any())
                sb.AppendJoin(" | ", this.hand).AppendLine();
            else
                sb.AppendLine(Emojis.Question).AppendLine();

            foreach (Participant participant in this.participants) {
                sb.Append(participant.User.Mention).Append(": ");
                sb.AppendLine(Formatter.Bold(this.HandValue(participant.Hand).ToString()));
                sb.AppendJoin(" | ", participant.Hand);
                sb.AppendLine().AppendLine();
            }

            var emb = new LocalizedEmbedBuilder(lcs, this.Channel.GuildId);
            emb.WithLocalizedTitle("fmt-casino-blackjack", Emojis.Cards.Suits[0], Emojis.Cards.Suits[0]);
            emb.WithColor(DiscordColor.DarkGreen);
            emb.WithDescription(sb);

            if (!this.gameOver)
                emb.AddLocalizedTitleField("str-casino-blackjack-hit", toMove?.User.Mention ?? lcs.GetString(this.Channel.GuildId, "str-house"));

            return msg.ModifyAsync(embed: emb.Build());
        }


        public sealed class Participant
        {
            public DiscordUser User { get; }
            public int Bid { get; }
            public List<Card> Hand { get; }
            public bool IsStanding { get; set; }
            public ulong Id => this.User.Id;

            public Participant(DiscordUser user, int bid)
            {
                this.User = user;
                this.Bid = bid;
                this.Hand = new List<Card>();
            }
        }
    }
}
