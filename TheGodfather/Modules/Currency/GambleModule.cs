using Discord.Commands;
using TheGodfather.Attributes;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Currency;

[Group("gamble")]
[Alias("bet")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class GambleModule : TheGodfatherModule
{
    private const long MaxBid = 5_000_000_000;

    private readonly BankAccountService bank;
    private readonly GuildConfigService guildConfig;
    private readonly RandomService random;


    public GambleModule(
        LocalizationService localization,
        BankAccountService bank,
        GuildConfigService guildConfig,
        RandomService random)
        : base(localization, new Color(0x2ECC71))
    {
        this.bank = bank;
        this.guildConfig = guildConfig;
        this.random = random;
    }


    [Command("coinflip")]
    [Priority(1)]
    [Alias("coin", "flip")]
    public async Task CoinflipAsync(long bid, string bet)
    {
        if (bid is < 1 or > MaxBid)
            throw new LocalizedCommandException(TranslationKey.cmd_err_gamble_bid(MaxBid));

        bool guess = bet.ToLowerInvariant() switch {
            "heads" or "head" or "h" => true,
            "tails" or "tail" or "t" => false,
            _ => throw new LocalizedCommandException(TranslationKey.cmd_err_gamble_coin)
        };

        if (!await this.bank.TryDecreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, bid))
            throw new LocalizedCommandException(TranslationKey.cmd_err_funds_insuf);

        bool actual = this.random.Coinflip();
        string flipped = this.Localization.GetString(this.Context.Guild.Id, actual ? TranslationKey.str_coinflip_heads : TranslationKey.str_coinflip_tails);
        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;

        if (guess == actual) {
            await this.bank.IncreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, 2 * bid);
            await this.ImpInfoAsync(this.ModuleColor, Emojis.Dice, TranslationKey.fmt_gamble_coin_w(this.Context.User.Mention, flipped, bid, currency));
        } else {
            await this.ImpInfoAsync(this.ModuleColor, Emojis.Dice, TranslationKey.fmt_gamble_coin_l(this.Context.User.Mention, flipped, bid, currency));
        }
    }

    [Command("coinflip")]
    [Priority(0)]
    [Alias("coin", "flip")]
    public Task CoinflipAsync(string bet, long bid = 5)
        => this.CoinflipAsync(bid, bet);

    [Command("dice")]
    [Alias("roll", "die")]
    public async Task RollDiceAsync(int bet, long bid = 5)
    {
        if (bid is < 1 or > MaxBid)
            throw new LocalizedCommandException(TranslationKey.cmd_err_gamble_bid(MaxBid));

        if (bet is < 1 or > 6)
            throw new LocalizedCommandException(TranslationKey.cmd_err_gamble_dice);

        if (!await this.bank.TryDecreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, bid))
            throw new LocalizedCommandException(TranslationKey.cmd_err_funds_insuf);

        int actual = this.random.Dice();
        string currency = this.guildConfig.GetCachedConfig(this.Context.Guild.Id).Currency;
        if (bet == actual) {
            long reward = 6 * bid;
            await this.bank.IncreaseBankAccountAsync(this.Context.Guild.Id, this.Context.User.Id, reward);
            await this.ImpInfoAsync(this.ModuleColor, Emojis.Dice, TranslationKey.fmt_gamble_dice_w(this.Context.User.Mention, actual, reward, currency));
        } else {
            await this.ImpInfoAsync(this.ModuleColor, Emojis.Dice, TranslationKey.fmt_gamble_dice_l(this.Context.User.Mention, actual, bid, currency));
        }
    }
}
