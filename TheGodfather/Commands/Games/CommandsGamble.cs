#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Services;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Games
{
    [Group("gamble", CanInvokeWithoutSubcommand = false)]
    [Description("Random betting and gambling commands.")]
    [Aliases("bet")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public partial class CommandsGamble
    {
        #region COMMAND_COINFLIP
        [Command("coinflip")]
        [Description("Flips a coin.")]
        [Aliases("coin", "flip")]
        public async Task CoinflipAsync(CommandContext ctx,
                                       [Description("Bid.")] int bid = 0,
                                       [Description("Heads/Tails (h/t).")] string bet = null)
        {
            if (bid != 0) {

                if (bid < 0)
                    throw new InvalidCommandUsageException("Invalid bid ammount!");

                if (string.IsNullOrWhiteSpace(bet))
                    throw new InvalidCommandUsageException("Missing heads or tails call.");
                bet = bet.ToLower();

                int guess;
                if (bet == "heads" || bet == "head" || bet == "h")
                    guess = 0;
                else if (bet == "tails" || bet == "tail" || bet == "t")
                    guess = 1;
                else
                    throw new CommandFailedException($"Invalid coin outcome call (has to be {Formatter.Bold("h")} or {Formatter.Bold("t")})");

                if (!await ctx.Dependencies.GetDependency<DatabaseService>().RetrieveCreditsAsync(ctx.User.Id, bid).ConfigureAwait(false))
                    throw new CommandFailedException("You do not have enough credits in WM bank!");

                int rnd = new Random().Next(2);
                if (rnd == guess)
                    await ctx.Dependencies.GetDependency<DatabaseService>().IncreaseBalanceForUserAsync(ctx.User.Id, bid * 2)
                        .ConfigureAwait(false);
                await ctx.RespondAsync($"{ctx.User.Mention} flipped " +
                    $"{(Formatter.Bold(rnd == 0 ? "Heads" : "Tails"))} " +
                    $"{(guess == rnd ? $"and won {bid} credits" : $"and lost {bid} credits")} !"
                ).ConfigureAwait(false);
            } else {
                await ctx.RespondAsync($"{ctx.User.Mention} flipped " + $"{(Formatter.Bold(new Random().Next(2) == 0 ? "Heads" : "Tails"))} !")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_ROLL
        [Command("roll")]
        [Description("Rolls a dice.")]
        [Aliases("dice", "die")]
        public async Task RollDiceAsync(CommandContext ctx,
                                       [Description("Bid.")] int bid = 0,
                                       [Description("Number guess.")] int guess = 0)
        {
            if (bid != 0) {

                if (bid < 0)
                    throw new InvalidCommandUsageException("Invalid bid ammount!");

                if (guess == 0)
                    throw new InvalidCommandUsageException("Missing number as a guess.");

                if (guess < 1 || guess > 6)
                    throw new CommandFailedException($"Invalid guess. Has to be a number from {Formatter.Bold("1")} to {Formatter.Bold("6")})");

                if (!await ctx.Dependencies.GetDependency<DatabaseService>().RetrieveCreditsAsync(ctx.User.Id, bid).ConfigureAwait(false))
                    throw new CommandFailedException("You do not have enough credits in WM bank!");

                int rnd = new Random().Next(1, 7);
                if (rnd == guess)
                    await ctx.Dependencies.GetDependency<DatabaseService>().IncreaseBalanceForUserAsync(ctx.User.Id, bid * 6)
                        .ConfigureAwait(false);

                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":game_die:")} {ctx.User.Mention} rolled a " +
                    $"{rnd} {(guess == rnd ? $"and won {bid * 5} credits" : $"and lost {bid} credits")} !"
                ).ConfigureAwait(false);
            } else {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":game_die:")} {ctx.User.Mention} rolled a {Formatter.Bold(new Random().Next(1, 7).ToString())}!")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_SLOT
        [Command("slot")]
        [Description("Roll a slot machine.")]
        [Aliases("slotmachine")]
        public async Task SlotMachine(CommandContext ctx,
                                     [Description("Bid.")] int bid = 5)
        {
            if (bid < 5)
                throw new CommandFailedException("5 is the minimum bid!", new ArgumentOutOfRangeException());

            if (!await ctx.Dependencies.GetDependency<DatabaseService>().RetrieveCreditsAsync(ctx.User.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            var slot_res = RollSlot(ctx);
            int won = EvaluateSlotResult(slot_res, bid);

            var em = new DiscordEmbedBuilder() {
                Title = "TOTALLY NOT RIGGED SLOT MACHINE",
                Description = MakeStringFromResult(slot_res),
                Color = DiscordColor.Yellow
            };
            em.AddField("Result", $"You won {Formatter.Bold(won.ToString())} credits!");

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);

            if (won > 0)
                await ctx.Dependencies.GetDependency<DatabaseService>().IncreaseBalanceForUserAsync(ctx.User.Id, won)
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
                    result[i, j] = emoji[rnd.Next(0, emoji.Length)];

            return result;
        }

        private string MakeStringFromResult(DiscordEmoji[,] res)
        {
            string s = "";
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++)
                    s += res[i, j].ToString();
                s += '\n';
            }
            return s;
        }

        private int EvaluateSlotResult(DiscordEmoji[,] res, int bid)
        {
            int pts = bid;

            // Rows
            for (int i = 0; i < 3; i++) {
                if (res[i, 0] == res[i, 1] && res[i, 1] == res[i, 2]) {
                    if (res[i, 0].ToString() == ":large_blue_diamond:")
                        pts *= 50;
                    else if (res[i, 0].ToString() == ":moneybag:")
                        pts *= 25;
                    else if (res[i, 0].ToString() == ":seven:")
                        pts *= 10;
                    else
                        pts *= 5;
                }
            }

            // Columns
            for (int i = 0; i < 3; i++) {
                if (res[0, i] == res[1, i] && res[1, i] == res[2, i]) {
                    if (res[0, i].ToString() == ":large_blue_diamond:")
                        pts *= 50;
                    else if (res[0, i].ToString() == ":moneybag:")
                        pts *= 25;
                    else if (res[0, i].ToString() == ":seven:")
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
