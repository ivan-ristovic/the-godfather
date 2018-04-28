#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Gambling.Common;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Modules.Gambling
{
    [Group("casino"), Module(ModuleType.Gambling)]
    [Description("Betting and gambling games.")]
    [Aliases("vegas")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public partial class CasinoModule : TheGodfatherBaseModule
    {

        public CasinoModule(DBService db) : base(db: db) { }


        #region COMMAND_CASINO_SLOT
        [Command("slot"), Module(ModuleType.Gambling)]
        [Description("Roll a slot machine. You need to specify a bid amount. Default bid amount is 5.")]
        [Aliases("slotmachine")]
        [UsageExample("!casino slot 20")]
        public async Task SlotAsync(CommandContext ctx,
                                   [Description("Bid.")] int bid = 5)
        {
            if (bid <= 0 || bid > 100000)
                throw new InvalidCommandUsageException("Invalid bid amount! Needs to be in range [0, 100000]");

            if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, bid).ConfigureAwait(false))
                throw new CommandFailedException("You do not have enough credits in WM bank!");

            await ctx.RespondAsync(embed: SlotMachine.EmbedSlotRoll(ctx.User, bid, out int won))
                .ConfigureAwait(false);

            if (won > 0)
                await Database.GiveCreditsToUserAsync(ctx.User.Id, won)
                    .ConfigureAwait(false);
        }
        #endregion
    }
}
