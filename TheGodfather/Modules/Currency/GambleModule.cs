using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Currency;

[Group("gamble")][Module(ModuleType.Currency)][NotBlocked]
[Description("Betting and gambling commands.")]
[Aliases("bet")]
[RequireGuild][Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class GambleModule : TheGodfatherServiceModule<BankAccountService>
{
    private const long MaxBid = 5_000_000_000;


    #region gamble coinflip
    [Command("coinflip")][Priority(1)]
    [Aliases("coin", "flip")]
    public async Task CoinflipAsync(CommandContext ctx,
        [Description(TranslationKey.desc_gamble_bid)] long bid,
        [Description(TranslationKey.desc_gamble_ht)] string bet)
    {
        if (bid is < 1 or > MaxBid)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_gamble_bid(MaxBid));

        if (string.IsNullOrWhiteSpace(bet))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_gamble_coin);

        bool guess = bet.ToLowerInvariant() switch {
            "heads" or "head" or "h" => true,
            "tails" or "tail" or "t" => false,
            _ => throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_gamble_coin)
        };

        if (!await this.Service.TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, bid))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_funds_insuf);

        bool actual = ctx.Services.GetRequiredService<RandomService>().Coinflip();

        string flipped = this.Localization.GetString(ctx.Guild.Id, actual ? TranslationKey.str_coinflip_heads : TranslationKey.str_coinflip_tails);
        string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
        if (guess == actual) {
            await this.Service.IncreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, 2 * bid);
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dice, TranslationKey.fmt_gamble_coin_w(ctx.User.Mention, flipped, bid, currency));
        } else {
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dice, TranslationKey.fmt_gamble_coin_l(ctx.User.Mention, flipped, bid, currency));
        }
    }

    [Command("coinflip")][Priority(0)]
    public Task CoinflipAsync(CommandContext ctx,
        [Description(TranslationKey.desc_gamble_ht)] string bet,
        [Description(TranslationKey.desc_gamble_bid)] long bid = 5)
        => this.CoinflipAsync(ctx, bid, bet);
    #endregion

    #region gamble dice
    [Command("dice")]
    [Aliases("roll", "die")]
    public async Task RollDiceAsync(CommandContext ctx,
        [Description(TranslationKey.desc_gamble_dice)] int bet,
        [Description(TranslationKey.desc_gamble_bid)] long bid = 5)
    {
        if (bid is < 1 or > MaxBid)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_gamble_bid(MaxBid));

        if (bet is < 1 or > 6)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_gamble_dice);

        if (!await this.Service.TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, bid))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_funds_insuf);

        int actual = ctx.Services.GetRequiredService<RandomService>().Dice();

        string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
        if (bet == actual) {
            long reward = 6 * bid;
            await this.Service.IncreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, reward);
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dice, TranslationKey.fmt_gamble_dice_w(ctx.User.Mention, actual, reward, currency));
        } else {
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dice, TranslationKey.fmt_gamble_dice_l(ctx.User.Mention, actual, bid, currency));
        }
    }
    #endregion
}