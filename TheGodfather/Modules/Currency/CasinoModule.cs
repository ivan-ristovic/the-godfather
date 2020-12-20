using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Currency
{
    [Group("casino"), Module(ModuleType.Currency), NotBlocked]
    [Aliases("vegas", "cs", "cas")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class CasinoModule : TheGodfatherServiceModule<BankAccountService>
    {
        private const long MaxBid = 5_000_000_000;


        #region casino
        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
        {
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                var sb = new StringBuilder();
                sb.AppendLine().AppendLine();
                sb.Append(Emojis.SmallBlueDiamond).AppendLine("holdem");
                sb.Append(Emojis.SmallBlueDiamond).AppendLine("lottery");
                sb.Append(Emojis.SmallBlueDiamond).AppendLine("slot");
                sb.Append(Emojis.SmallBlueDiamond).AppendLine("wheeloffortune");
                emb.WithLocalizedTitle("str-casino");
                emb.WithColor(this.ModuleColor);
                emb.WithDescription(sb);
                emb.WithLocalizedFooter("str-casino-footer", ctx.Client.CurrentUser.AvatarUrl);
            });
        }
        #endregion

        #region casino slot
        [Command("slot"), Priority(1)]
        [Aliases("slotmachine")]
        public async Task SlotAsync(CommandContext ctx,
                                   [Description("desc-bid")] long bid = 5)
        {
            if (bid < 1 || bid > MaxBid)
                throw new InvalidCommandUsageException(ctx, "cmd-err-gamble-bid", MaxBid);

            if (!await this.Service.TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, bid))
                throw new CommandFailedException(ctx, "cmd-err-funds-insuf");

            var roll = new SlotMachineRoll(bid);
            await roll.SendAsync(ctx, this.ModuleColor);

            if (roll.WonAmount > 0)
                await this.Service.IncreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, roll.WonAmount);
        }

        [Command("slot"), Priority(0)]
        public Task SlotAsync(CommandContext ctx,
                             [RemainingText, Description("desc-bid")] string bidMetric)
        {
            if (string.IsNullOrWhiteSpace(bidMetric))
                throw new InvalidCommandUsageException(ctx, "cmd-err-casino-bid-none");

            try {
                long bid = (long)bidMetric.FromMetric();
                return this.SlotAsync(ctx, bid);
            } catch {
                throw new InvalidCommandUsageException(ctx, "cmd-err-casino-bid-met");
            }
        }
        #endregion

        #region casino wheeloffortune
        [Command("wheeloffortune"), Priority(1)]
        [Aliases("wof")]
        public async Task WheelOfFortuneAsync(CommandContext ctx,
                                             [Description("desc-bid")] long bid = 5)
        {
            if (bid < 1 || bid > MaxBid)
                throw new InvalidCommandUsageException(ctx, "cmd-err-gamble-bid", MaxBid);

            if (!await this.Service.TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, bid))
                throw new CommandFailedException(ctx, "cmd-err-funds-insuf");

            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            var wof = new WofGame(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, bid, currency);
            await wof.RunAsync(ctx.Services.GetRequiredService<LocalizationService>());

            if (wof.WonAmount > 0) 
                await this.Service.IncreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, wof.WonAmount);
        }

        [Command("wheeloffortune"), Priority(0)]
        public Task WheelOfFortuneAsync(CommandContext ctx,
                                       [RemainingText, Description("desc-bid")] string bidMetric)
        {
            if (string.IsNullOrWhiteSpace(bidMetric))
                throw new InvalidCommandUsageException(ctx, "cmd-err-casino-bid-none");

            try {
                long bid = (long)bidMetric.FromMetric();
                return this.WheelOfFortuneAsync(ctx, bid);
            } catch {
                throw new InvalidCommandUsageException(ctx, "cmd-err-casino-bid-met");
            }
        }
        #endregion
    }
}
