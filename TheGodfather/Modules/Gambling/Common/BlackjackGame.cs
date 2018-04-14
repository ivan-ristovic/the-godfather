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
    public class BlackjackGame : Game
    {
        public bool Started { get; private set; }
        public int ParticipantCount => _participants.Count;

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

            _deck.Shuffle();

            foreach (var participant in _participants)
                participant.Hand.AddRange(_deck.Draw(2));
            
            while (_participants.Any(p => !p.Standing)) {
                foreach (var participant in _participants) {
                    if (participant.Standing)
                        continue;

                    await PrintGameAsync(msg, participant)
                        .ConfigureAwait(false);

                    if (await _interactivity.WaitForYesNoAnswerAsync(_channel.Id, participant.Id).ConfigureAwait(false)) 
                        participant.Hand.Add(_deck.Draw());
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
                _hand.Add(_deck.Draw());

            await PrintGameAsync(msg)
                .ConfigureAwait(false);

            GameOver = true;
        }

        public bool AddParticipant(DiscordUser user, int bid)
        {
            if (_participants.Any(p => p.Id == user.Id))
                return false;

            _participants.Enqueue(new BlackjackParticipant {
                User = user,
                Bid = bid
            });

            return true;
        }

        private int HandValue(List<Card> hand)
        {
            int value = 0;
            foreach (var card in hand) {
                if (card.Value >= 10)
                    value += 10;
                else if (card.Value == 1)
                    value += 11;
                else 
                    value += card.Value;
            }
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
                emb.AddField("Deciding whether to hit a card (type yes/no):", tomove == null ? "House" : tomove.User.Mention);

            await msg.ModifyAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }


        private sealed class BlackjackParticipant
        {
            public DiscordUser User { get; internal set; }
            public List<Card> Hand { get; internal set; } = new List<Card>();
            public int Bid { get; set; }
            public bool Standing { get; set; } = false;
            public ulong Id => User.Id;
        }
    }
}
