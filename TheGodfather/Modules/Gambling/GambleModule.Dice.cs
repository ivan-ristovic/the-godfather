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
        [Command("dice"), Priority(1)]
        [Description("Roll a dice and bet on the outcome.")]
        [Aliases("roll", "die")]
        [UsageExample("!dice 50 six")]
        [UsageExample("!dice three 10")]
        public async Task RollDiceAsync(CommandContext ctx,
                                       [Description("Bid.")] int bid,
                                       [Description("Number guess (has to be a word one-six).")] string guess)
        {
            if (bid < 0)
                throw new InvalidCommandUsageException("Invalid bid amount!");

            if (string.IsNullOrWhiteSpace(guess))
                throw new InvalidCommandUsageException("Missing guess number.");
            guess = guess.ToLowerInvariant();

            int guess_int = 0;
            switch (guess) {
                case "one":   guess_int = 1; break;
                case "two":   guess_int = 2; break;
                case "three": guess_int = 3; break;
                case "four":  guess_int = 4; break;
                case "five":  guess_int = 5; break;
                case "six":   guess_int = 6; break;
                default:
                    throw new CommandFailedException($"Invalid guess. Has to be a number from {Formatter.Bold("one")} to {Formatter.Bold("six")})");
            }

            if (!await DatabaseService.RetrieveCreditsAsync(ctx.User.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            int rnd = new Random().Next(1, 7);

            StringBuilder sb = new StringBuilder();
            sb.Append(ctx.User.Mention)
              .Append(" rolled a ")
              .Append(Formatter.Bold(rnd.ToString()))
              .Append(" and ")
              .Append(guess_int == rnd ? $"won {Formatter.Bold((bid * 5).ToString())}" : $"lost {Formatter.Bold((bid).ToString())}")
              .Append(" credits!");

            await ReplyWithEmbedAsync(ctx, sb.ToString(), ":game_die:")
                .ConfigureAwait(false);

            if (rnd == guess_int)
                await DatabaseService.IncreaseBalanceForUserAsync(ctx.User.Id, bid * 6)
                    .ConfigureAwait(false);
        }

        [Command("dice"), Priority(0)]
        public async Task RollDiceAsync(CommandContext ctx,
                                       [Description("Number guess (has to be a word one-six).")] string guess,
                                       [Description("Bid.")] int bid)
            => await RollDiceAsync(ctx, bid, guess).ConfigureAwait(false);
    }
}
