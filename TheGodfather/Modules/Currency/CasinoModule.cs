#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Services.Database.Bank;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;

using Humanizer;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Currency
{
    [Group("casino"), Module(ModuleType.Currency)]
    [Description("Betting and gambling games.")]
    [Aliases("vegas", "cs", "cas")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public partial class CasinoModule : TheGodfatherModule
    {

        public CasinoModule(DBService db) : base(db: db) { }

        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
        {
            return ctx.InformSuccessAsync(
                Formatter.Bold("Casino games:\n\n") +
                "holdem, lottery, slot, wheeloffortune"
            );
        }


        #region COMMAND_CASINO_SLOT
        [Command("slot"), Priority(1)]
        [Module(ModuleType.Currency)]
        [Description("Roll a slot machine. You need to specify a bid amount. Default bid amount is 5.")]
        [Aliases("slotmachine")]
        [UsageExamples("!casino slot 20")]
        public async Task SlotAsync(CommandContext ctx,
                                   [Description("Bid.")] long bid = 5)
        {
            if (bid <= 0 || bid > 1000000000)
                throw new InvalidCommandUsageException($"Invalid bid amount! Needs to be in range [1 - {1000000000:n0}]");

            if (!await Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            await ctx.RespondAsync(embed: SlotMachine.EmbedSlotRoll(ctx.User, bid, out long won))
                .ConfigureAwait(false);

            if (won > 0)
                await Database.IncreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, won)
                    .ConfigureAwait(false);
        }


        [Command("slot"), Priority(0)]
        public Task SlotAsync(CommandContext ctx,
                             [RemainingText, Description("Bid as a metric number.")] string bidstr)
        {
            if (string.IsNullOrWhiteSpace(bidstr))
                throw new InvalidCommandUsageException($"Bid missing.");
            
            try {
                int bid = (int)bidstr.FromMetric();
                return SlotAsync(ctx, bid);
            } catch {
                throw new InvalidCommandUsageException("Given string does not correspond to a valid metric number.");
            }
        }
        #endregion

        #region COMMAND_CASINO_WHEELOFFORTUNE
        [Command("wheeloffortune"), Priority(1)]
        [Module(ModuleType.Currency)]
        [Description("Roll a Wheel Of Fortune. You need to specify a bid amount. Default bid amount is 5.")]
        [Aliases("wof")]
        [UsageExamples("!casino wof 20")]
        public async Task WheelOfFortuneAsync(CommandContext ctx,
                                             [Description("Bid.")] long bid = 5)
        {
            if (bid <= 0 || bid > 1000000000)
                throw new InvalidCommandUsageException($"Invalid bid amount! Needs to be in range [1 - {1000000000:n0}]");

            if (!await Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            var wof = new WheelOfFortune(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, bid);
            await wof.RunAsync()
                .ConfigureAwait(false);
            
            if (wof.WonAmount > 0)
                await Database.IncreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, wof.WonAmount)
                    .ConfigureAwait(false);
        }

        [Command("wheeloffortune"), Priority(0)]
        public Task WheelOfFortuneAsync(CommandContext ctx,
                                       [RemainingText, Description("Bid as a metric number.")] string bidstr)
        {
            if (string.IsNullOrWhiteSpace(bidstr))
                throw new InvalidCommandUsageException($"Bid missing.");

            try {
                int bid = (int)bidstr.FromMetric();
                return WheelOfFortuneAsync(ctx, bid);
            } catch {
                throw new InvalidCommandUsageException("Given string does not correspond to a valid metric number.");
            }
        }
        #endregion
    }
}
