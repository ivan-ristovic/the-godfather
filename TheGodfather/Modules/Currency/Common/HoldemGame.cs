using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Humanizer;
using TexasHoldem.Logic;
using TexasHoldem.Logic.Cards;
using TexasHoldem.Logic.Helpers;
using TheGodfather.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Currency.Common
{
    public sealed class HoldemGame : BaseChannelGame
    {
        public const int DefaultBalance = 1000;
        public const int MaxParticipants = 7;

        private static readonly HandEvaluator _evaluator = new HandEvaluator();

        public int MaxBalance { get; }
        public bool Started { get; private set; }
        public long Pot { get; private set; }
        public ConcurrentQueue<Participant> Participants { get; }

        private bool gameOver;
        private readonly Deck deck;
        private readonly List<Card> drawn;
        private IEnumerable<Participant> ActiveParticipants => this.Participants.Where(p => !p.HasFolded);


        public HoldemGame(InteractivityExtension interactivity, DiscordChannel channel, int balance)
            : base(interactivity, channel)
        {
            this.MaxBalance = balance;
            this.Started = false;
            this.gameOver = false;
            this.deck = new Deck();
            this.drawn = new List<Card>();
            this.Participants = new ConcurrentQueue<Participant>();
        }


        public override async Task RunAsync(LocalizationService lcs)
        {
            this.Started = true;

            DiscordMessage msg = await this.Channel.EmbedAsync(lcs.GetString(this.Channel.GuildId, "str-casino-holdem-starting"));

            foreach (Participant participant in this.Participants) {
                participant.Card1 = this.deck.GetNextCard();
                participant.Card2 = this.deck.GetNextCard();
                try {
                    participant.DmHandle = await participant.DmHandle.ModifyAsync(embed: new DiscordEmbedBuilder {
                        Description = $"{participant.Card1.ToUserFriendlyString()} {participant.Card2.ToUserFriendlyString()}",
                        Color = DiscordColor.DarkGreen,
                    }.Build());
                } catch {
                    await this.Channel.InformFailureAsync(lcs.GetString(this.Channel.GuildId, "str-casino-holdem-dm-fail", participant.User.Mention));
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10));

            this.drawn.AddRange(this.deck.DrawCards(3));

            int bet = Math.Max(this.MaxBalance / 10, 1);
            for (int step = 0; step < 2; step++) {
                foreach (Participant participant in this.ActiveParticipants)
                    participant.Bet = 0;

                while (this.ActiveParticipants.Any(p => p.Bet != bet)) {
                    foreach (Participant participant in this.ActiveParticipants) {
                        await this.PrintGameAsync(lcs, msg, bet, participant);

                        if (await this.Interactivity.WaitForBoolReplyAsync(this.Channel, participant.User)) {
                            await this.Channel.LocalizedEmbedAsync(lcs, "q-casino-holdem", participant.User.Mention, participant.Balance - bet);
                            if (await this.Interactivity.WaitForBoolReplyAsync(this.Channel, participant.User)) {
                                int raise = 0;
                                InteractivityResult<DiscordMessage> mctx = await this.Interactivity.WaitForMessageAsync(
                                    m => m.Channel.Id == this.Channel.Id && m.Author.Id == participant.Id 
                                      && int.TryParse(m.Content, out raise) && bet + raise <= participant.Balance
                                );
                                if (!mctx.TimedOut)
                                    bet += raise;
                            }
                            participant.Balance -= bet - participant.Bet;
                            participant.Bet = bet;
                            this.Pot += bet;
                        } else {
                            participant.HasFolded = true;
                        }
                    }
                }

                this.drawn.Add(this.deck.GetNextCard());
            }

            this.gameOver = true;
            await this.PrintGameAsync(lcs, msg, bet, showhands: true);

            foreach (Participant p in this.ActiveParticipants)
                p.HandRank = _evaluator.GetBestHand(new List<Card>(this.drawn) { p.Card1!, p.Card2! }).RankType;

            Participant winner = this.Participants.OrderByDescending(p => p.HandRank).First();
            if (!(winner is null))
                winner.Balance += this.Pot;
            this.Winner = winner?.User;
        }

        public void AddParticipant(DiscordUser user, DiscordMessage dm)
        {
            if (this.IsParticipating(user))
                return;

            this.Participants.Enqueue(new Participant(user, dm, this.MaxBalance));
        }

        public bool IsParticipating(DiscordUser user)
            => this.Participants.Any(p => p.Id == user.Id);


        private Task PrintGameAsync(LocalizationService lcs, DiscordMessage msg, int bet, Participant? toMove = null, bool showhands = false)
        {
            var sb = new StringBuilder();

            sb.AppendJoin(' ', this.drawn).AppendLine();
            sb.Append(lcs.GetString(this.Channel.GuildId, "str-casino-holdem-pot")).Append(' ')
              .AppendLine(Formatter.Bold(this.Pot.ToString())).AppendLine();
            sb.Append(lcs.GetString(this.Channel.GuildId, "str-casino-holdem-cv")).Append(' ')
              .AppendLine(Formatter.Bold(bet.ToString())).AppendLine();

            foreach (Participant participant in this.Participants) {
                sb.Append(participant.User.Mention)
                  .Append(" | ").Append(lcs.GetString(this.Channel.GuildId, "str-casino-holdem-chips")).Append(' ')
                  .Append(Formatter.Bold(participant.Balance.ToString()))
                  .Append(" | ").Append(lcs.GetString(this.Channel.GuildId, "str-casino-holdem-bet")).Append(' ')
                  .Append(Formatter.Bold(participant.Bet.ToString()));
                if (participant.HasFolded) {
                    sb.AppendLine(Formatter.Bold(lcs.GetString(this.Channel.GuildId, "str-casino-holdem-fold"))).AppendLine();
                }
                sb.AppendLine().AppendLine();
                if (showhands) {
                    if (participant.Card1 is { })
                        sb.Append(participant.Card1.ToUserFriendlyString());
                    sb.Append(' ');
                    if (participant.Card2 is { })
                        sb.Append(participant.Card2.ToUserFriendlyString());
                    sb.Append(" | ");
                    sb.AppendLine(participant.HandRank.Humanize(LetterCasing.Title)).AppendLine();
                }
            }

            var emb = new LocalizedEmbedBuilder(lcs, this.Channel.GuildId);
            emb.WithLocalizedTitle("fmt-casino-holdem", Emojis.Cards.Suits[0], Emojis.Cards.Suits[0]);
            emb.WithColor(DiscordColor.DarkGreen);
            emb.WithDescription(sb);

            if (!this.gameOver && toMove is { })
                emb.AddLocalizedField("str-casino-holdem-call", toMove.User.Mention);

            return msg.ModifyAsync(embed: emb.Build());
        }


        public sealed class Participant
        {
            public DiscordUser User { get; }
            public Card? Card1 { get; set; }
            public Card? Card2 { get; set; }
            public long Balance { get; set; }
            public int Bet { get; set; }
            public bool HasFolded { get; set; }
            public ulong Id => this.User.Id;
            public DiscordMessage DmHandle { get; internal set; }
            public HandRankType HandRank { get; set; }


            public Participant(DiscordUser user, DiscordMessage dmHandle, int balance)
            {
                this.User = user;
                this.Balance = balance;
                this.DmHandle = dmHandle;
            }
        }
    }
}
