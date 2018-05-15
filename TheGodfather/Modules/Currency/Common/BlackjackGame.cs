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

using TexasHoldem.Logic.Cards;
#endregion

namespace TheGodfather.Modules.Currency.Common
{
    public class BlackjackGame : ChannelEvent
    {
        public bool Started { get; private set; }
        public int ParticipantCount => _participants.Count;
        public IReadOnlyList<BlackjackParticipant> Winners
        {
            get {
                int hvalue = HandValue(_hand);
                if (hvalue > 21)
                    return _participants.Where(p => HandValue(p.Hand) <= 21).ToList().AsReadOnly();
                return _participants.Where(p => {
                    var value = HandValue(p.Hand);
                    if (value > 21)
                        return false;
                    return value > hvalue;
                }).ToList().AsReadOnly();
            }
        }

        private ConcurrentQueue<BlackjackParticipant> _participants = new ConcurrentQueue<BlackjackParticipant>();
        private Deck _deck = new Deck();
        private List<Card> _hand = new List<Card>();
        private bool GameOver = false;


        public BlackjackGame(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            Started = false;
        }


        public override async Task RunAsync()
        {
            Started = true;

            var msg = await _channel.SendIconEmbedAsync("Starting blackjack game...")
                .ConfigureAwait(false);

            foreach (var participant in _participants) {
                participant.Hand.Add(_deck.GetNextCard());
                participant.Hand.Add(_deck.GetNextCard());
                if (HandValue(participant.Hand) == 21) {
                    GameOver = true;
                    Winner = participant.User;
                    break;
                }
            }

            if (GameOver) {
                await PrintGameAsync(msg)
                    .ConfigureAwait(false);
                return;
            }
            
            while (_participants.Any(p => !p.Standing)) {
                foreach (var participant in _participants) {
                    if (participant.Standing)
                        continue;

                    await PrintGameAsync(msg, participant)
                        .ConfigureAwait(false);

                    if (await _interactivity.WaitForYesNoAnswerAsync(_channel.Id, participant.Id).ConfigureAwait(false)) 
                        participant.Hand.Add(_deck.GetNextCard());
                    else 
                        participant.Standing = true;

                    if (HandValue(participant.Hand) > 21)
                        participant.Standing = true;
                }
            }

            await PrintGameAsync(msg)
                .ConfigureAwait(false);

            await Task.Delay(TimeSpan.FromSeconds(5))
                .ConfigureAwait(false);

            while (HandValue(_hand) <= 17)
                _hand.Add(_deck.GetNextCard());

            if (_hand.Count == 2 && HandValue(_hand) == 21) {
                await _channel.SendIconEmbedAsync("BLACKJACK!")
                    .ConfigureAwait(false);
            }

            GameOver = true;
            await PrintGameAsync(msg)
                .ConfigureAwait(false);
        }

        public void AddParticipant(DiscordUser user, int bid)
        {
            if (IsParticipating(user))
                return;

            _participants.Enqueue(new BlackjackParticipant {
                User = user,
                Bid = bid
            });
        }

        public bool IsParticipating(DiscordUser user) => _participants.Any(p => p.Id == user.Id);

        private int HandValue(List<Card> hand)
        {
            int value = 0;
            bool one = false;
            foreach (var card in hand) {
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

        private async Task PrintGameAsync(DiscordMessage msg, BlackjackParticipant tomove = null)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Formatter.Bold($"House hand: (value: {HandValue(_hand)})")).AppendLine();
            if (_hand.Any())
                sb.AppendLine(string.Join(" | ", _hand)).AppendLine();
            else
                sb.AppendLine(StaticDiscordEmoji.Question).AppendLine();

            foreach (var participant in _participants) {
                sb.Append(participant.User.Mention)
                  .Append(" (value: ")
                  .Append(Formatter.Bold(HandValue(participant.Hand).ToString()))
                  .AppendLine(")").AppendLine()
                  .AppendLine(string.Join(" | ", participant.Hand))
                  .AppendLine();
            }
            var emb = new DiscordEmbedBuilder() {
                Title = $"{StaticDiscordEmoji.CardSuits[0]} BLACKJACK GAME STATE {StaticDiscordEmoji.CardSuits[0]}",
                Description = sb.ToString(),
                Color = DiscordColor.Red
            };

            if (!GameOver)
                emb.AddField("Deciding whether to hit a card (type yes/no):", tomove?.User.Mention ?? "House");

            await msg.ModifyAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }


        public sealed class BlackjackParticipant
        {
            public DiscordUser User { get; internal set; }
            public List<Card> Hand { get; internal set; } = new List<Card>();
            public int Bid { get; set; }
            public bool Standing { get; set; } = false;
            public ulong Id => User.Id;
        }
    }
}
