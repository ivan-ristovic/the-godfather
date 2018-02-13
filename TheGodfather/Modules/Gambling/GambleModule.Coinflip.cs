#region USING_DIRECTIVES
using System;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Gambling
{
    public partial class GambleModule : GodfatherBaseModule
    {
        [Command("coinflip"), Priority(1)]
        [Description("Flip a coin and bet on the outcome.")]
        [Aliases("coin", "flip")]
        [UsageExample("!bet coinflip 10 heads")]
        [UsageExample("!bet coinflip tails 20")]
        public async Task CoinflipAsync(CommandContext ctx,
                                       [Description("Bid.")] int bid,
                                       [Description("Heads/Tails (h/t).")] string bet)
        {
            if (bid <= 0)
                throw new InvalidCommandUsageException("Invalid bid amount!");

            if (string.IsNullOrWhiteSpace(bet))
                throw new InvalidCommandUsageException("Missing heads or tails call.");
            bet = bet.ToLowerInvariant();

            int guess;
            if (bet == "heads" || bet == "head" || bet == "h")
                guess = 0;
            else if (bet == "tails" || bet == "tail" || bet == "t")
                guess = 1;
            else
                throw new CommandFailedException($"Invalid coin outcome call (has to be {Formatter.Bold("heads")} or {Formatter.Bold("tails")})");

            if (!await DatabaseService.RetrieveCreditsAsync(ctx.User.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            int rnd = new Random().Next(2);

            StringBuilder sb = new StringBuilder();
            sb.Append(ctx.User.Mention)
              .Append(" flipped ")
              .Append(Formatter.Bold(rnd == 0 ? "Heads" : "Tails"))
              .Append(" and ")
              .Append(guess == rnd ? "won " : "lost ")
              .Append(Formatter.Bold(bid.ToString()))
              .Append(" credits!");

            await ReplyWithEmbedAsync(ctx, sb.ToString(), ":game_die:")
                .ConfigureAwait(false);

            if (rnd == guess)
                await DatabaseService.IncreaseBalanceForUserAsync(ctx.User.Id, bid * 2)
                    .ConfigureAwait(false);
        }

        [Command("coinflip"), Priority(0)]
        public async Task CoinflipAsync(CommandContext ctx,
                                       [Description("Heads/Tails (h/t).")] string bet,
                                       [Description("Bid.")] int bid)
            => await CoinflipAsync(ctx, bid, bet).ConfigureAwait(false);
    }
}
