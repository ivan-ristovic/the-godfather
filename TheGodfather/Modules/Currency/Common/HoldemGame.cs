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

using TexasHoldem.Logic;
using TexasHoldem.Logic.Cards;
using TexasHoldem.Logic.Helpers;
#endregion

namespace TheGodfather.Modules.Currency.Common
{
    public class HoldemGame : ChannelEvent
    {
        public bool Started { get; private set; }
        public ConcurrentQueue<HoldemParticipant> Participants { get; } = new ConcurrentQueue<HoldemParticipant>();
        public int ParticipantCount => Participants.Count;
        public int MoneyNeeded { get; set; }
        public long Pot { get; set; }

        private IEnumerable<HoldemParticipant> ActiveParticipants => Participants.Where(p => p.Folded == false);
        private Deck _deck = new Deck();
        private List<Card> _drawn = new List<Card>();
        private bool GameOver = false;
        private HandEvaluator _evaluator = new HandEvaluator();


        public HoldemGame(InteractivityExtension interactivity, DiscordChannel channel, int balance)
            : base(interactivity, channel)
        {
            MoneyNeeded = balance;
            Started = false;
        }


        public override async Task RunAsync()
        {
            Started = true;

            var msg = await _channel.SendIconEmbedAsync("Starting Hold'Em game... Keep an eye on DM!")
                .ConfigureAwait(false);

            foreach (var participant in Participants) {
                participant.Card1 = _deck.GetNextCard();
                participant.Card2 = _deck.GetNextCard();
                try {
                    participant.DmHandle = await participant.DmHandle.ModifyAsync($"Your hand: {participant.Card1.ToUserFriendlyString()} {participant.Card2.ToUserFriendlyString()}")
                        .ConfigureAwait(false);
                } catch {

                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10))
                .ConfigureAwait(false);
            
            _drawn.Add(_deck.GetNextCard());
            _drawn.Add(_deck.GetNextCard());
            _drawn.Add(_deck.GetNextCard());

            int bet = 5;
            int step = 0;
            do {
                foreach (var participant in ActiveParticipants)
                    participant.Bet = 0;

                while (ActiveParticipants.Any(p => p.Bet != bet)) {
                    foreach (var participant in ActiveParticipants) {

                        await PrintGameAsync(msg, bet, participant)
                            .ConfigureAwait(false);

                        if (await _interactivity.WaitForYesNoAnswerAsync(_channel.Id, participant.Id).ConfigureAwait(false)) {
                            await _channel.SendMessageAsync($"Do you wish to raise the current bet? If yes, reply yes and then reply raise amount in new message, otherwise say no. Max: {participant.Balance - bet}")
                                .ConfigureAwait(false);
                            if (await _interactivity.WaitForYesNoAnswerAsync(_channel.Id, participant.Id).ConfigureAwait(false)) {
                                int raise = 0;
                                var mctx = await _interactivity.WaitForMessageAsync(
                                    m => m.Channel.Id == _channel.Id && m.Author.Id == participant.Id && int.TryParse(m.Content, out raise) && bet + raise <= participant.Balance
                                ).ConfigureAwait(false);
                                if (mctx != null) {
                                    bet += raise;
                                }
                            }
                            participant.Balance -= bet - participant.Bet;
                            participant.Bet = bet;
                            Pot += bet;
                        } else {
                            participant.Folded = true;
                        }
                    }
                }

                _drawn.Add(_deck.GetNextCard());

                step++;
            } while (step < 2);

            GameOver = true;
            await PrintGameAsync(msg, bet, showhands: true)
                .ConfigureAwait(false);

            foreach (var p in ActiveParticipants)
                p.HandRank = _evaluator.GetBestHand(new List<Card>(_drawn) { p.Card1, p.Card2 }).RankType;


            var winner = Participants.OrderByDescending(p => p.HandRank).FirstOrDefault();
            if (winner != null)
                winner.Balance += Pot;
            Winner = winner?.User;
        }

        public void AddParticipant(DiscordUser user, DiscordMessage dm)
        {
            if (IsParticipating(user))
                return;

            Participants.Enqueue(new HoldemParticipant {
                User = user,
                Balance = MoneyNeeded,
                DmHandle = dm
            });
        }

        public bool IsParticipating(DiscordUser user)
            => Participants.Any(p => p.Id == user.Id);

        private async Task PrintGameAsync(DiscordMessage msg, int bet, HoldemParticipant tomove = null, bool showhands = false)
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Join(" ", _drawn)).AppendLine();
            sb.Append("Current pot: ").AppendLine(Formatter.Bold(Pot.ToString())).AppendLine();
            sb.Append("Call value: ").AppendLine(Formatter.Bold(bet.ToString())).AppendLine();

            foreach (var participant in Participants) {
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

            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.CardSuits[0]} HOLD'EM GAME STATE {StaticDiscordEmoji.CardSuits[0]}",
                Description = sb.ToString(),
                Color = DiscordColor.Red
            };

            if (!GameOver && tomove != null)
                emb.AddField("Deciding whether to call (type yes/no):", tomove.User.Mention);

            await msg.ModifyAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }


        public sealed class HoldemParticipant
        {
            public DiscordUser User { get; internal set; }
            public Card Card1 { get; set; }
            public Card Card2 { get; set; }
            public long Balance { get; set; }
            public int Bet { get; set; }
            public bool Folded { get; set; } = false;
            public ulong Id => User.Id;
            public DiscordMessage DmHandle { get; set; }
            public HandRankType HandRank { get; set; }
        }
    }
}
