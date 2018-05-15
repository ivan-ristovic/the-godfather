#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Currency
{
    [Group("casino"), Module(ModuleType.Gambling)]
    [Description("Betting and gambling games.")]
    [Aliases("vegas", "cs", "cas")]
    [Cooldown(3, 7, CooldownBucketType.Channel)]
    [ListeningCheck]
    public partial class CasinoModule : TheGodfatherBaseModule
    {

        public CasinoModule(DBService db) : base(db: db) { }

        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
        {
            return ctx.RespondWithIconEmbedAsync(
                Formatter.Bold("Casino games:\n\n") +
                "holdem, lottery, slot, wheeloffortune"
            );
        }


        #region COMMAND_CASINO_SLOT
        [Command("slot"), Module(ModuleType.Gambling)]
        [Description("Roll a slot machine. You need to specify a bid amount. Default bid amount is 5.")]
        [Aliases("slotmachine")]
        [UsageExample("!casino slot 20")]
        public async Task SlotAsync(CommandContext ctx,
                                   [Description("Bid.")] long bid = 5)
        {
            if (bid <= 0 || bid > 1000000000)
                throw new InvalidCommandUsageException("Invalid bid amount! Needs to be in range [1, 1000000000]");

            if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            await ctx.RespondAsync(embed: SlotMachine.EmbedSlotRoll(ctx.User, bid, out long won))
                .ConfigureAwait(false);

            if (won > 0)
                await Database.GiveCreditsToUserAsync(ctx.User.Id, won)
                    .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CASINO_WHEELOFFORTUNE
        [Command("wheeloffortune"), Module(ModuleType.Gambling)]
        [Description("Roll a Wheel Of Fortune. You need to specify a bid amount. Default bid amount is 5.")]
        [Aliases("wof")]
        [UsageExample("!casino wof 20")]
        public async Task WheelOfFortuneAsync(CommandContext ctx,
                                             [Description("Bid.")] long bid = 5)
        {
            if (bid <= 0 || bid > 1000000000)
                throw new InvalidCommandUsageException("Invalid bid amount! Needs to be in range [1, 1000000000]");

            if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            var wof = new WheelOfFortune(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, bid);
            await wof.RunAsync()
                .ConfigureAwait(false);
            
            if (wof.WonAmount > 0)
                await Database.GiveCreditsToUserAsync(ctx.User.Id, wof.WonAmount)
                    .ConfigureAwait(false);
        }
        #endregion
    }
}
