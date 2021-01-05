using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Currency
{
    [Group("gamble"), Module(ModuleType.Currency), NotBlocked]
    [Description("Betting and gambling commands.")]
    [Aliases("bet")]
    [RequireGuild, Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class GambleModule : TheGodfatherServiceModule<BankAccountService>
    {
        private const long MaxBid = 5_000_000_000;


        #region gamble coinflip
        [Command("coinflip"), Priority(1)]
        [Aliases("coin", "flip")]
        public async Task CoinflipAsync(CommandContext ctx,
                                       [Description("desc-gamble-bid")] long bid,
                                       [Description("desc-gamble-ht")] string bet)
        {
            if (bid is < 1 or > MaxBid)
                throw new InvalidCommandUsageException(ctx, "cmd-err-gamble-bid", MaxBid);

            if (string.IsNullOrWhiteSpace(bet))
                throw new InvalidCommandUsageException(ctx, "cmd-err-gamble-coin");

            bool guess = bet.ToLowerInvariant() switch {
                "heads" or "head" or "h" => true,
                "tails" or "tail" or "t" => false,
                _ => throw new InvalidCommandUsageException(ctx, "cmd-err-gamble-coin"),
            };

            if (!await this.Service.TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, bid))
                throw new CommandFailedException(ctx, "cmd-err-funds-insuf");

            bool actual = ctx.Services.GetRequiredService<RandomService>().Coinflip();

            string flipped = this.Localization.GetString(ctx.Guild.Id, actual ? "str-coinflip-heads" : "str-coinflip-tails");
            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            if (guess == actual) {
                await this.Service.IncreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, 2 * bid);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dice, "fmt-gamble-coin-w", ctx.User.Mention, flipped, bid, currency);
            } else {
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dice, "fmt-gamble-coin-l", ctx.User.Mention, flipped, bid, currency);
            }
        }

        [Command("coinflip"), Priority(0)]
        public Task CoinflipAsync(CommandContext ctx,
                                 [Description("desc-gamble-ht")] string bet,
                                 [Description("desc-gamble-bid")] long bid = 5)
            => this.CoinflipAsync(ctx, bid, bet);
        #endregion

        #region gamble dice
        [Command("dice")]
        [Aliases("roll", "die")]
        public async Task RollDiceAsync(CommandContext ctx,
                                       [Description("desc-gamble-bid")] long bid,
                                       [Description("desc-gamble-dice")] int bet = 5)
        {
            if (bid is < 1 or > MaxBid)
                throw new InvalidCommandUsageException(ctx, "cmd-err-gamble-bid", MaxBid);

            if (bet is < 1 or > 6)
                throw new InvalidCommandUsageException(ctx, "cmd-err-gamble-dice");

            if (!await this.Service.TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, bid))
                throw new CommandFailedException(ctx, "cmd-err-funds-insuf");

            int actual = ctx.Services.GetRequiredService<RandomService>().Dice();

            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            if (bet == actual) {
                long reward = 6 * bid;
                await this.Service.IncreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, reward);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dice, "fmt-gamble-dice-w", actual, reward, currency);
            } else {
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dice, "fmt-gamble-dice-l", actual, bid, currency);
            }
        }
        #endregion
    }
}
