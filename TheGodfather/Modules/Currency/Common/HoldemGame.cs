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
using TexasHoldem.Logic;
using TexasHoldem.Logic.Cards;
using TexasHoldem.Logic.Helpers;
using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Currency.Common
{
    public sealed class HoldemGame : BaseChannelGame
    {
        private static readonly HandEvaluator _evaluator = new HandEvaluator();

        public int MoneyNeeded { get; private set; }
        public bool Started { get; private set; }
        public long Pot { get; private set; }
        public ConcurrentQueue<Participant> Participants { get; }

        private bool GameOver;
        private readonly Deck deck;
        private readonly List<Card> drawn;
        private IEnumerable<Participant> ActiveParticipants => this.Participants.Where(p => p.Folded == false);


        public HoldemGame(InteractivityExtension interactivity, DiscordChannel channel, int balance)
            : base(interactivity, channel)
        {
            this.MoneyNeeded = balance;
            this.Started = false;
            this.GameOver = false;
            this.deck = new Deck();
            this.drawn = new List<Card>();
            this.Participants = new ConcurrentQueue<Participant>();
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            this.Started = true;

            DiscordMessage msg = await this.Channel.EmbedAsync("Starting Hold'Em game... Keep an eye on DM!");

            foreach (Participant participant in this.Participants) {
                participant.Card1 = this.deck.GetNextCard();
                participant.Card2 = this.deck.GetNextCard();
                try {
                    participant.DmHandle = await participant.DmHandle.ModifyAsync($"Your hand: {participant.Card1.ToUserFriendlyString()} {participant.Card2.ToUserFriendlyString()}");
                } catch {

                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10));

            this.drawn.AddRange(this.deck.DrawCards(3));

            int bet = 5;
            for (int step = 0; step < 2; step++) {
                foreach (Participant participant in this.ActiveParticipants)
                    participant.Bet = 0;

                while (this.ActiveParticipants.Any(p => p.Bet != bet)) {
                    foreach (Participant participant in this.ActiveParticipants) {
                        await this.PrintGameAsync(msg, bet, participant);

                        if (await this.Interactivity.WaitForBoolReplyAsync(this.Channel, participant.User)) {
                            await this.Channel.SendMessageAsync($"Do you wish to raise the current bet? If yes, reply yes and then reply raise amount in new message, otherwise say no. Max: {participant.Balance - bet}");
                            if (await this.Interactivity.WaitForBoolReplyAsync(this.Channel, participant.User)) {
                                int raise = 0;
                                InteractivityResult<DiscordMessage> mctx = await this.Interactivity.WaitForMessageAsync(
                                    m => m.Channel.Id == this.Channel.Id && m.Author.Id == participant.Id && int.TryParse(m.Content, out raise) && bet + raise <= participant.Balance
                                );
                                if (!mctx.TimedOut) {
                                    bet += raise;
                                }
                            }
                            participant.Balance -= bet - participant.Bet;
                            participant.Bet = bet;
                            this.Pot += bet;
                        } else {
                            participant.Folded = true;
                        }
                    }
                }

                this.drawn.Add(this.deck.GetNextCard());
            }

            this.GameOver = true;
            await this.PrintGameAsync(msg, bet, showhands: true);

            foreach (Participant p in this.ActiveParticipants)
                p.HandRank = _evaluator.GetBestHand(new List<Card>(this.drawn) { p.Card1, p.Card2 }).RankType;

            Participant winner = this.Participants.OrderByDescending(p => p.HandRank).FirstOrDefault();
            if (!(winner is null))
                winner.Balance += this.Pot;
            this.Winner = winner?.User;
        }

        public void AddParticipant(DiscordUser user, DiscordMessage dm)
        {
            if (this.IsParticipating(user))
                return;

            this.Participants.Enqueue(new Participant {
                User = user,
                Balance = MoneyNeeded,
                DmHandle = dm
            });
        }

        public bool IsParticipating(DiscordUser user)
            => this.Participants.Any(p => p.Id == user.Id);


        private Task PrintGameAsync(DiscordMessage msg, int bet, Participant tomove = null, bool showhands = false)
        {
            var sb = new StringBuilder();

            sb.AppendJoin(' ', this.drawn).AppendLine();
            sb.Append("Current pot: ").AppendLine(Formatter.Bold(this.Pot.ToString())).AppendLine();
            sb.Append("Call value: ").AppendLine(Formatter.Bold(bet.ToString())).AppendLine();

            foreach (Participant participant in this.Participants) {
                sb.Append(participant.User.Mention)
                  .Append(" | Chips: ")
                  .Append(Formatter.Bold(participant.Balance.ToString()))
                  .Append(" | Bet: ")
                  .Append(Formatter.Bold(participant.Bet.ToString()));
                if (participant.Folded) {
                    sb.AppendLine(Formatter.Bold("Folded")).AppendLine();
                }
                sb.AppendLine().AppendLine();
                if (showhands)
                    sb.AppendLine($"{participant.Card1.ToUserFriendlyString()} {participant.Card2.ToUserFriendlyString()} | {participant.HandRank}").AppendLine();
            }

            var emb = new DiscordEmbedBuilder {
                Title = $"{Emojis.Cards.Suits[0]} HOLD'EM GAME STATE {Emojis.Cards.Suits[0]}",
                Description = sb.ToString(),
                Color = DiscordColor.DarkGreen
            };

            if (!this.GameOver && !(tomove is null))
                emb.AddField("Deciding whether to call (type yes/no):", tomove.User.Mention);

            return msg.ModifyAsync(embed: emb.Build());
        }


        public sealed class Participant
        {
            public DiscordUser User { get; internal set; }
            public Card Card1 { get; set; }
            public Card Card2 { get; set; }
            public long Balance { get; set; }
            public int Bet { get; set; }
            public bool Folded { get; set; } = false;
            public ulong Id => this.User.Id;
            public DiscordMessage DmHandle { get; internal set; }
            public HandRankType HandRank { get; set; }
        }
    }
}
