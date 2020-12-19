#region USING_DIRECTIVES
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Currency
{
    [Group("casino"), Module(ModuleType.Currency), NotBlocked]
    [Description("Betting and gambling games.")]
    [Aliases("vegas", "cs", "cas")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class CasinoModule : TheGodfatherModule
    {
        private static readonly long _maxBet = 5_000_000_000;

        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
        {
            var sb = new StringBuilder();

            sb.AppendLine().AppendLine();
            sb.Append(Emojis.SmallBlueDiamond).AppendLine("holdem");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine("lottery");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine("slot");
            sb.Append(Emojis.SmallBlueDiamond).AppendLine("wheeloffortune");

            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = "Available casino games:",
                Description = sb.ToString(),
                Color = this.ModuleColor,
            }.WithFooter("Start a game by typing: casino <game name>").Build());
        }


        #region COMMAND_CASINO_SLOT
        [Command("slot"), Priority(1)]
        [Description("Roll a slot machine. You need to specify a bid amount. Default bid amount is 5.")]
        [Aliases("slotmachine")]

        public async Task SlotAsync(CommandContext ctx,
                                   [Description("Bid.")] long bid = 5)
        {
            if (bid <= 0 || bid > _maxBet)
                throw new InvalidCommandUsageException($"Invalid bid amount! Needs to be in range [1, {_maxBet:n0}]");

            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, bid))
                    throw new CommandFailedException($"You do not have enough {ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency}! Use command {Formatter.InlineCode("bank")} to check your account status.");
                await db.SaveChangesAsync();
            }

            await ctx.RespondAsync(embed: SlotMachine.RollToDiscordEmbed(ctx.User, bid, ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency, out long won));

            if (won > 0) {
                using TheGodfatherDbContext db = this.Database.CreateContext();
                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + won);
                await db.SaveChangesAsync();
            }
        }

        [Command("slot"), Priority(0)]
        public Task SlotAsync(CommandContext ctx,
                             [RemainingText, Description("Bid as a metric number.")] string bidstr)
        {
            if (string.IsNullOrWhiteSpace(bidstr))
                throw new InvalidCommandUsageException("Bid missing.");

            try {
                long bid = (long)bidstr.FromMetric();
                return this.SlotAsync(ctx, bid);
            } catch {
                throw new InvalidCommandUsageException("Given string does not correspond to a valid metric number.");
            }
        }
        #endregion

        #region COMMAND_CASINO_WHEELOFFORTUNE
        [Command("wheeloffortune"), Priority(1)]
        [Description("Roll a Wheel Of Fortune. You need to specify a bid amount. Default bid amount is 5.")]
        [Aliases("wof")]

        public async Task WheelOfFortuneAsync(CommandContext ctx,
                                             [Description("Bid.")] long bid = 5)
        {
            if (bid <= 0 || bid > _maxBet)
                throw new InvalidCommandUsageException($"Invalid bid amount! Needs to be in range [1, {_maxBet:n0}]");

            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, bid))
                    throw new CommandFailedException($"You do not have enough {ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency}! Use command {Formatter.InlineCode("bank")} to check your account status.");
                await db.SaveChangesAsync();
            }

            var wof = new WheelOfFortuneGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, bid, ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency);
            await wof.RunAsync(ctx.Services.GetRequiredService<LocalizationService>());

            if (wof.WonAmount > 0) {
                using TheGodfatherDbContext db = this.Database.CreateContext();
                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + wof.WonAmount);
                await db.SaveChangesAsync();
            }
        }

        [Command("wheeloffortune"), Priority(0)]
        public Task WheelOfFortuneAsync(CommandContext ctx,
                                       [RemainingText, Description("Bid as a metric number.")] string bidstr)
        {
            if (string.IsNullOrWhiteSpace(bidstr))
                throw new InvalidCommandUsageException("Bid missing.");

            try {
                long bid = (long)bidstr.FromMetric();
                return this.WheelOfFortuneAsync(ctx, bid);
            } catch {
                throw new InvalidCommandUsageException("Given string does not correspond to a valid metric number.");
            }
        }
        #endregion
    }
}
