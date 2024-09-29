using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Currency.Services;

namespace TheGodfather.Modules.Currency;

public partial class CasinoModule
{
    [Group("lottery")]
    [Aliases("lotto", "bingo")]
    public sealed class LotteryModule : TheGodfatherServiceModule<ChannelEventService>
    {
        #region casino lottery
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
            [RemainingText][Description(TranslationKey.desc_gamble_numbers_3)] params int[] numbers)
        {
            if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                if (this.Service.GetEventInChannel(ctx.Channel.Id) is LotteryGame)
                    await this.JoinAsync(ctx, numbers);
                else
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_evt_dup);
                return;
            }

            string currency = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency;
            var game = new LotteryGame(ctx.Client.GetInteractivity(), ctx.Channel);
            this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
            try {
                await ctx.ImpInfoAsync(
                    this.ModuleColor, 
                    Emojis.Clock1, 
                    TranslationKey.str_casino_lottery_start(LotteryGame.MaxParticipants, LotteryGame.TicketPrice, currency)
                );
                await this.JoinAsync(ctx, numbers);
                await Task.Delay(TimeSpan.FromSeconds(30));

                BankAccountService bas = ctx.Services.GetRequiredService<BankAccountService>();
                if (game.ParticipantCount > 1) {
                    await game.RunAsync(this.Localization);

                    if (game.Winners.Any()) {
                        await ctx.ImpInfoAsync(
                            this.ModuleColor, 
                            Emojis.Cards.Suits[0], 
                            TranslationKey.fmt_winnings(game.Winners.Select(w => $"{w.User.Mention}: {w.WinAmount:n0} {currency}").JoinWith())
                        );
                        foreach (LotteryGame.Participant winner in game.Winners)
                            await bas.IncreaseBankAccountAsync(ctx.Guild.Id, winner.Id, winner.WinAmount);
                    } else {
                        await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Cards.Suits[0], TranslationKey.str_casino_lottery_lose);
                    }
                } else {
                    if (game.IsParticipating(ctx.User))
                        await bas.IncreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, LotteryGame.TicketPrice);
                    await ctx.ImpInfoAsync(this.ModuleColor, Emojis.AlarmClock, TranslationKey.str_casino_lottery_none);
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
            [RemainingText][Description(TranslationKey.desc_gamble_numbers_3)] params int[] numbers)
        {
            numbers = numbers.Distinct().ToArray();
            if (numbers is null || numbers.Length != 3 || numbers.Any(n => n is < 1 or > LotteryGame.MaxNumber))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_lottery_num(LotteryGame.MaxNumber));

            if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out LotteryGame? game) || game is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_lottery_none);

            if (game.Started)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_lottery_started);

            if (game.ParticipantCount >= LotteryGame.MaxParticipants)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_lottery_full(LotteryGame.MaxParticipants));

            if (game.IsParticipating(ctx.User))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_casino_lottery_dup);

            if (!await ctx.Services.GetRequiredService<BankAccountService>().TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, LotteryGame.TicketPrice))
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_funds_insuf);

            game.AddParticipant(ctx.User, numbers);

            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.EightBall, TranslationKey.fmt_casino_lottery_join(ctx.User.Mention));
        }
        #endregion

        #region casino lottery rules
        [Command("rules")]
        [Aliases("help", "h", "ruling", "rule")]
        public Task RulesAsync(CommandContext ctx)
        {
            return ctx.ImpInfoAsync(
                this.ModuleColor, 
                Emojis.Information, 
                TranslationKey.str_casino_lottery(LotteryGame.MaxNumber, LotteryGame.TicketPrice, LotteryGame.Prizes.JoinWith(", "))
            );
        }
        #endregion
    }
}