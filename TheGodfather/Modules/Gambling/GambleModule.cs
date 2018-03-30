#region USING_DIRECTIVES
using System;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Gambling
{
    [Group("gamble")]
    [Description("Betting and gambling commands.")]
    [Aliases("bet")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public partial class GambleModule : TheGodfatherBaseModule
    {
        public GambleModule(DBService db) : base(db: db) { }


        #region COMMAND_GAMBLE_COINFLIP
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

            if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, bid).ConfigureAwait(false))
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

            await ctx.RespondWithIconEmbedAsync(sb.ToString(), ":game_die:")
                .ConfigureAwait(false);

            if (rnd == guess)
                await Database.GiveCreditsToUserAsync(ctx.User.Id, bid * 2)
                    .ConfigureAwait(false);
        }

        [Command("coinflip"), Priority(0)]
        public async Task CoinflipAsync(CommandContext ctx,
                                       [Description("Heads/Tails (h/t).")] string bet,
                                       [Description("Bid.")] int bid)
            => await CoinflipAsync(ctx, bid, bet).ConfigureAwait(false);
        #endregion

        #region COMMAND_GAMBLE_DICE
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
                case "one": guess_int = 1; break;
                case "two": guess_int = 2; break;
                case "three": guess_int = 3; break;
                case "four": guess_int = 4; break;
                case "five": guess_int = 5; break;
                case "six": guess_int = 6; break;
                default:
                    throw new CommandFailedException($"Invalid guess. Has to be a number from {Formatter.Bold("one")} to {Formatter.Bold("six")})");
            }

            if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            int rnd = new Random().Next(1, 7);

            StringBuilder sb = new StringBuilder();
            sb.Append(ctx.User.Mention)
              .Append(" rolled a ")
              .Append(Formatter.Bold(rnd.ToString()))
              .Append(" and ")
              .Append(guess_int == rnd ? $"won {Formatter.Bold((bid * 5).ToString())}" : $"lost {Formatter.Bold((bid).ToString())}")
              .Append(" credits!");

            await ctx.RespondWithIconEmbedAsync(sb.ToString(), ":game_die:")
                .ConfigureAwait(false);

            if (rnd == guess_int)
                await Database.GiveCreditsToUserAsync(ctx.User.Id, bid * 6)
                    .ConfigureAwait(false);
        }

        [Command("dice"), Priority(0)]
        public async Task RollDiceAsync(CommandContext ctx,
                                       [Description("Number guess (has to be a word one-six).")] string guess,
                                       [Description("Bid.")] int bid)
            => await RollDiceAsync(ctx, bid, guess).ConfigureAwait(false);
        #endregion

        #region COMMAND_SLOT
        [Command("slot")]
        [Description("Roll a slot machine.")]
        [Aliases("slotmachine")]
        [UsageExample("!gamble slot 20")]
        public async Task SlotMachine(CommandContext ctx,
                                     [Description("Bid.")] int bid = 5)
        {
            if (bid <= 0)
                throw new InvalidCommandUsageException("Invalid bid amount!");

            if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            DiscordEmoji[,] res = RollSlot(ctx);
            int won = EvaluateSlotResult(res, bid);

            var em = new DiscordEmbedBuilder() {
                Title = "TOTALLY NOT RIGGED SERBIAN SLOT MACHINE",
                Description = MakeStringFromResult(res),
                Color = DiscordColor.Yellow
            };
            em.AddField("Result", $"You won {Formatter.Bold(won.ToString())} credits!");

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);

            if (won > 0)
                await Database.GiveCreditsToUserAsync(ctx.User.Id, won)
                    .ConfigureAwait(false);
        }
        #endregion


        #region HELPER_FUNCTIONS
        private DiscordEmoji[,] RollSlot(CommandContext ctx)
        {
            DiscordEmoji[] emoji = {
                DiscordEmoji.FromName(ctx.Client, ":peach:"),
                DiscordEmoji.FromName(ctx.Client, ":moneybag:"),
                DiscordEmoji.FromName(ctx.Client, ":gift:"),
                DiscordEmoji.FromName(ctx.Client, ":large_blue_diamond:"),
                DiscordEmoji.FromName(ctx.Client, ":seven:"),
                DiscordEmoji.FromName(ctx.Client, ":cherries:")
            };

            var rnd = new Random();
            DiscordEmoji[,] result = new DiscordEmoji[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    result[i, j] = emoji[rnd.Next(emoji.Length)];

            return result;
        }

        private string MakeStringFromResult(DiscordEmoji[,] res)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++)
                    sb.Append(res[i, j]);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private int EvaluateSlotResult(DiscordEmoji[,] res, int bid)
        {
            int pts = bid;

            for (int i = 0; i < 3; i++) {
                if (res[i, 0] == res[i, 1] && res[i, 1] == res[i, 2]) {
                    if (res[i, 0].GetDiscordName() == ":large_blue_diamond:")
                        pts *= 50;
                    else if (res[i, 0].GetDiscordName() == ":moneybag:")
                        pts *= 25;
                    else if (res[i, 0].GetDiscordName() == ":seven:")
                        pts *= 10;
                    else
                        pts *= 5;
                }
            }

            for (int i = 0; i < 3; i++) {
                if (res[0, i] == res[1, i] && res[1, i] == res[2, i]) {
                    if (res[0, i].GetDiscordName() == ":large_blue_diamond:")
                        pts *= 50;
                    else if (res[0, i].GetDiscordName() == ":moneybag:")
                        pts *= 25;
                    else if (res[0, i].GetDiscordName() == ":seven:")
                        pts *= 10;
                    else
                        pts *= 5;
                }
            }

            return pts == bid ? 0 : pts;
        }
        #endregion
    }
}
