using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Discord.Commands;
using TexasHoldem.Logic;
using TexasHoldem.Logic.Cards;
using TexasHoldem.Logic.Helpers;
using TheGodfather.Attributes;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Currency.Services;

namespace TheGodfather.Modules.Currency;

[Group("casino")]
[Alias("vegas", "cs", "cas")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class CasinoModule : TheGodfatherModule
{
    private const long MaxBid = 1_000_000_000;
    private const int BlackjackInitialBid = 5;
    private const int LotteryMaxNumber = 15;
    private const int LotteryTicketPrice = 250;

    private static readonly ImmutableArray<string> _slotEmoji = new[] {
        Emojis.LargeBlueDiamond,
        Emojis.Seven,
        Emojis.MoneyBag,
        Emojis.Trophy,
        Emojis.Gift,
        Emojis.Cherries
    }.ToImmutableArray();
    private static readonly ImmutableArray<float> _wofMultipliers = new[] {
        2.4f, 0.3f, 1.7f, 0.5f, 1.2f, 0.1f, 0.2f, 1.5f
    }.ToImmutableArray();
    private static readonly ImmutableArray<int> _lotteryPrizes = new[] {
        0, 2500, 50000, 1000000
    }.ToImmutableArray();
    private static readonly HandEvaluator _holdemEvaluator = new();

    private readonly BankAccountService bank;
    private readonly GuildConfigService guildConfig;
    private readonly SecureRandom rng = new();


    public CasinoModule(LocalizationService localization, BankAccountService bank, GuildConfigService guildConfig)
        : base(localization, new Color(0x2ECC71))
    {
        this.bank = bank;
        this.guildConfig = guildConfig;
    }


    [Command("")]
    public Task ExecuteGroupAsync()
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.Append(Emojis.SmallBlueDiamond).AppendLine("slot");
        sb.Append(Emojis.SmallBlueDiamond).AppendLine("wheeloffortune");
        sb.Append(Emojis.SmallBlueDiamond).AppendLine("blackjack");
        sb.Append(Emojis.SmallBlueDiamond).AppendLine("holdem");
        sb.Append(Emojis.SmallBlueDiamond).AppendLine("lottery");

        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_casino);
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(sb);
            emb.WithLocalizedFooter(TranslationKey.str_casino_footer, this.Context.Client.CurrentUser.GetAvatarUrl());
        });
    }

    [Command("slot")]
    [Priority(1)]
    [Alias("slotmachine")]
    public async Task SlotAsync(long bid = 5)
    {
        if (bid is < 1 or > MaxBid)
            throw new LocalizedCommandException(TranslationKey.cmd_err_gamble_bid(MaxBid));

        if (!await this.bank.TryDecreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, bid))
            throw new LocalizedCommandException(TranslationKey.cmd_err_funds_insuf);

        var roll = new SlotMachineRoll(bid);
        await this.SendSlotRollAsync(roll);

        if (roll.WonAmount > 0)
            await this.bank.IncreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, roll.WonAmount);
    }

    [Command("slot")]
    [Priority(0)]
    public Task SlotMetricAsync([Remainder] string bidMetric)
    {
        if (string.IsNullOrWhiteSpace(bidMetric))
            throw new LocalizedCommandException(TranslationKey.cmd_err_casino_bid_none);

        try {
            long bid = (long)bidMetric.FromMetric();
            return this.SlotAsync(bid);
        } catch {
            throw new LocalizedCommandException(TranslationKey.cmd_err_casino_bid_met);
        }
    }

    [Command("wheeloffortune")]
    [Priority(1)]
    [Alias("wof")]
    public async Task WheelOfFortuneAsync(long bid = 5)
    {
        if (bid is < 1 or > MaxBid)
            throw new LocalizedCommandException(TranslationKey.cmd_err_gamble_bid(MaxBid));

        if (!await this.bank.TryDecreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, bid))
            throw new LocalizedCommandException(TranslationKey.cmd_err_funds_insuf);

        int index = this.rng.Next(_wofMultipliers.Length);
        long won = (long)(bid * _wofMultipliers[index]);
        if (won > 0)
            await this.bank.IncreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, won);

        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        CultureInfo culture = this.Localization.GetGuildCulture(this.Context.Guild.Id);
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_casino);
            emb.WithColor(this.ModuleColor);
            emb.WithDescription($"{Emojis.Joystick} x{_wofMultipliers[index]:0.0#}\n\n" +
                                this.Localization.GetString(this.Context.Guild.Id, TranslationKey.fmt_casino_win(this.Context.User.Mention, won.ToWords(culture), won, currency)));
        });
    }

    [Command("wheeloffortune")]
    [Priority(0)]
    public Task WheelOfFortuneMetricAsync([Remainder] string bidMetric)
    {
        if (string.IsNullOrWhiteSpace(bidMetric))
            throw new LocalizedCommandException(TranslationKey.cmd_err_casino_bid_none);

        try {
            long bid = (long)bidMetric.FromMetric();
            return this.WheelOfFortuneAsync(bid);
        } catch {
            throw new LocalizedCommandException(TranslationKey.cmd_err_casino_bid_met);
        }
    }

    [Command("blackjack")]
    [Alias("bj")]
    public async Task BlackjackAsync(long bid = BlackjackInitialBid)
    {
        if (bid is < 1 or > MaxBid)
            throw new LocalizedCommandException(TranslationKey.cmd_err_gamble_bid(MaxBid));

        if (!await this.bank.TryDecreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, bid))
            throw new LocalizedCommandException(TranslationKey.cmd_err_funds_insuf);

        var deck = new Deck();
        var player = new List<Card> { deck.GetNextCard(), deck.GetNextCard() };
        var dealer = new List<Card> { deck.GetNextCard(), deck.GetNextCard() };

        while (HandValue(player) < 21) {
            await this.PrintBlackjackAsync(player, dealer, revealDealer: false, $"{this.Context.User.Mention}: hit? (y/n)");
            SocketUserMessage? reply = await this.WaitForMessageAsync(msg =>
                msg.Author.Id == this.Context.User.Id &&
                msg.Channel.Id == this.Context.Channel.Id &&
                IsHitOrStand(msg.Content),
                TimeSpan.FromSeconds(30)
            );

            string answer = reply?.Content.Trim().ToLowerInvariant() ?? "n";
            if (answer is "y" or "yes" or "h" or "hit")
                player.Add(deck.GetNextCard());
            else
                break;
        }

        while (HandValue(player) <= 21 && HandValue(dealer) <= 17)
            dealer.Add(deck.GetNextCard());

        int playerValue = HandValue(player);
        int dealerValue = HandValue(dealer);
        long payout = playerValue > 21 ? 0
            : dealerValue > 21 || playerValue > dealerValue ? bid * 2
            : playerValue == dealerValue ? bid
            : 0;

        if (payout > 0)
            await this.bank.IncreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, payout);

        string result = payout == bid * 2 ? "Blackjack result: you win."
            : payout == bid ? "Blackjack result: push."
            : "Blackjack result: house wins.";
        await this.PrintBlackjackAsync(player, dealer, revealDealer: true, result);
    }

    [Command("holdem")]
    [Alias("poker", "texasholdem", "texas")]
    public async Task HoldemAsync(long bid = 5)
    {
        if (bid is < 1 or > MaxBid)
            throw new LocalizedCommandException(TranslationKey.cmd_err_gamble_bid(MaxBid));

        if (!await this.bank.TryDecreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, bid))
            throw new LocalizedCommandException(TranslationKey.cmd_err_funds_insuf);

        var deck = new Deck();
        var player = new List<Card> { deck.GetNextCard(), deck.GetNextCard() };
        var dealer = new List<Card> { deck.GetNextCard(), deck.GetNextCard() };
        var table = deck.DrawCards(5).ToList();

        HandRankType playerRank = _holdemEvaluator.GetBestHand(table.Concat(player).ToList()).RankType;
        HandRankType dealerRank = _holdemEvaluator.GetBestHand(table.Concat(dealer).ToList()).RankType;
        long payout = playerRank > dealerRank ? bid * 2 : playerRank == dealerRank ? bid : 0;
        if (payout > 0)
            await this.bank.IncreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, payout);

        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.fmt_casino_holdem(Emojis.Cards.Suits[0], Emojis.Cards.Suits[0]));
            emb.WithColor(this.ModuleColor);
            emb.AddField("Table", FormatCards(table));
            emb.AddField(this.Context.User.Username, $"{FormatCards(player)} | {playerRank.Humanize(LetterCasing.Title)}", true);
            emb.AddField(this.Localization.GetString(this.Context.Guild.Id, TranslationKey.str_house), $"{FormatCards(dealer)} | {dealerRank.Humanize(LetterCasing.Title)}", true);
            emb.AddLocalizedField(TranslationKey.str_result, $"{(payout > bid ? "You win" : payout == bid ? "Push" : "House wins")} ({payout:n0} {currency})");
        });
    }

    [Command("lottery")]
    [Alias("lotto", "bingo")]
    public async Task LotteryAsync(params int[] numbers)
    {
        numbers = numbers?.Distinct().OrderBy(n => n).ToArray() ?? Array.Empty<int>();
        if (numbers.Length != 3 || numbers.Any(n => n is < 1 or > LotteryMaxNumber))
            throw new LocalizedCommandException(TranslationKey.cmd_err_casino_lottery_num(LotteryMaxNumber));

        if (!await this.bank.TryDecreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, LotteryTicketPrice))
            throw new LocalizedCommandException(TranslationKey.cmd_err_funds_insuf);

        int[] drawn = Enumerable.Range(1, LotteryMaxNumber)
            .OrderBy(_ => this.rng.Next())
            .Take(3)
            .OrderBy(n => n)
            .ToArray();
        int matches = numbers.Intersect(drawn).Count();
        int won = _lotteryPrizes[matches];
        if (won > 0)
            await this.bank.IncreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, won);

        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.fmt_casino_lottery(Emojis.MoneyBag, Emojis.MoneyBag));
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_casino_lottery_drawn, drawn.JoinWith(" "), true);
            emb.AddField(this.Context.User.Username, numbers.JoinWith(" "), true);
            emb.AddLocalizedField(TranslationKey.str_result, $"{matches}/3 - {won:n0} {currency}");
        });
    }

    private Task SendSlotRollAsync(SlotMachineRoll roll)
    {
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.fmt_casino_slot(Emojis.LargeOrangeDiamond, Emojis.LargeOrangeDiamond));
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(ToEmojiString(roll));
            emb.WithThumbnail(this.Context.User.GetAvatarUrl() ?? this.Context.User.GetDefaultAvatarUrl());

            var multipliers = new StringBuilder();
            foreach ((string emoji, int multiplier) in _slotEmoji.Zip(SlotMachineRoll.Multipliers))
                multipliers.Append(emoji).Append(InlineCode($" x{multiplier} "));

            emb.AddLocalizedField(TranslationKey.str_multipliers, multipliers);

            string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
            CultureInfo culture = this.Localization.GetGuildCulture(this.Context.Guild.Id);
            emb.AddLocalizedField(
                TranslationKey.str_result,
                TranslationKey.fmt_casino_win(this.Context.User.Mention, roll.WonAmount.ToWords(culture), roll.WonAmount, currency)
            );
        });
    }

    private static string ToEmojiString(SlotMachineRoll roll)
    {
        var sb = new StringBuilder();

        sb.Append(Emojis.BlackSquare);
        for (int i = 0; i < 5; i++)
            sb.Append(Emojis.SmallOrangeDiamond);
        sb.AppendLine();

        for (int i = 0; i < 3; i++) {
            sb.Append(i % 2 != 0 ? Emojis.Joystick : Emojis.BlackSquare);
            sb.Append(Emojis.SmallOrangeDiamond);
            for (int j = 0; j < 3; j++)
                sb.Append(_slotEmoji[roll.Result[i, j]]);
            sb.AppendLine(Emojis.SmallOrangeDiamond);
        }

        sb.Append(Emojis.BlackSquare);
        for (int i = 0; i < 5; i++)
            sb.Append(Emojis.SmallOrangeDiamond);

        return sb.ToString();
    }

    private Task PrintBlackjackAsync(IReadOnlyList<Card> player, IReadOnlyList<Card> dealer, bool revealDealer, string footer)
    {
        string dealerCards = revealDealer ? FormatCards(dealer) : $"{dealer[0]} | {Emojis.Question}";
        string dealerValue = revealDealer ? HandValue(dealer).ToString(CultureInfo.InvariantCulture) : "?";
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.fmt_casino_blackjack(Emojis.Cards.Suits[0], Emojis.Cards.Suits[0]));
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_house, $"{Bold(dealerValue)}\n{dealerCards}");
            emb.AddField(this.Context.User.Username, $"{Bold(HandValue(player).ToString(CultureInfo.InvariantCulture))}\n{FormatCards(player)}");
            emb.WithFooter(footer);
        });
    }

    private static int HandValue(IEnumerable<Card> hand)
    {
        int value = 0;
        bool ace = false;
        foreach (Card card in hand) {
            if (card.Type >= CardType.Ten) {
                value += 10;
            } else if (card.Type == CardType.Ace) {
                value += 1;
                ace = true;
            } else {
                value += (int)card.Type;
            }
        }

        if (ace && value <= 11)
            value += 10;
        return value;
    }

    private static string FormatCards(IEnumerable<Card> cards)
        => cards.Select(card => card.ToString()).JoinWith(" | ");

    private static bool IsHitOrStand(string content)
    {
        string value = content.Trim().ToLowerInvariant();
        return value is "y" or "yes" or "h" or "hit" or "n" or "no" or "s" or "stand";
    }
}
