using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Currency
{
    public partial class CasinoModule
    {
        [Group("lottery")]
        [Aliases("lotto", "bingo")]
        public sealed class LotteryModule : TheGodfatherServiceModule<ChannelEventService>
        {
            #region casino lottery
            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [RemainingText, Description("desc-gamble-numbers-3")] params int[] numbers)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Service.GetEventInChannel(ctx.Channel.Id) is LotteryGame)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException(ctx, "cmd-err-evt-dup");
                    return;
                }

                string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
                var game = new LotteryGame(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, "str-casino-lottery-start",
                        LotteryGame.MaxParticipants, LotteryGame.TicketPrice, currency
                    );
                    await this.JoinAsync(ctx, numbers);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    BankAccountService bas = ctx.Services.GetRequiredService<BankAccountService>();
                    if (game.ParticipantCount > 1) {
                        await game.RunAsync(ctx.Services.GetRequiredService<LocalizationService>());

                        if (game.Winners.Any()) {
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Cards.Suits[0], "fmt-winnings",
                                game.Winners.Select(w => $"{w.User.Mention}: {w.WinAmount:n0} {currency}").JoinWith()
                            );
                            foreach (LotteryGame.Participant winner in game.Winners)
                                await bas.IncreaseBankAccountAsync(ctx.Guild.Id, winner.Id, winner.WinAmount);
                        } else {
                            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Cards.Suits[0], "str-casino-lottery-lose");
                        }
                    } else {
                        if (game.IsParticipating(ctx.User))
                            await bas.IncreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, LotteryGame.TicketPrice);
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.AlarmClock, "str-casino-lottery-none");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region casino lottery join
            [Command("join")]
            [Aliases("+", "compete", "enter", "j", "<<", "<")]
            public async Task JoinAsync(CommandContext ctx,
                                       [RemainingText, Description("desc-gamble-numbers-3")] params int[] numbers)
            {
                if (numbers is null || numbers.Length != 3 || numbers.Any(n => n is < 1 or > LotteryGame.MaxNumber))
                    throw new CommandFailedException(ctx, "cmd-err-casino-lottery-num", LotteryGame.MaxNumber);

                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out LotteryGame? game) || game is null)
                    throw new CommandFailedException(ctx, "cmd-err-casino-lottery-none");

                if (game.Started)
                    throw new CommandFailedException(ctx, "cmd-err-casino-lottery-started");

                if (game.ParticipantCount >= LotteryGame.MaxParticipants)
                    throw new CommandFailedException(ctx, "cmd-err-casino-lottery-full", LotteryGame.MaxParticipants);

                if (game.IsParticipating(ctx.User))
                    throw new CommandFailedException(ctx, "cmd-err-casino-lottery-dup");

                if (!await ctx.Services.GetRequiredService<BankAccountService>().TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, LotteryGame.TicketPrice))
                    throw new CommandFailedException(ctx, "cmd-err-funds-insuf");

                game.AddParticipant(ctx.User, numbers);

                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.EightBall, "fmt-casino-lottery-join", ctx.User.Mention);
            }
            #endregion

            #region casino lottery rules
            [Command("rules")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
            {
                return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, "str-casino-lottery", 
                    LotteryGame.MaxNumber, LotteryGame.TicketPrice, LotteryGame.Prizes.JoinWith(", ")
                );
            }
            #endregion
        }
    }
}
