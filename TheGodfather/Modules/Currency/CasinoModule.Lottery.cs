#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services.Database.Bank;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Currency
{
    public partial class CasinoModule
    {
        [Group("lottery"), Module(ModuleType.Currency)]
        [Description("Play a lottery game. The three numbers are drawn from 1 to 15 and they can't repeat.")]
        [Aliases("lotto")]
        [UsageExamples("!casino lottery 2 10 8")]
        public class LotteryModule : TheGodfatherModule
        {

            public LotteryModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [RemainingText, Description("Three numbers.")] params int[] numbers)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Shared.GetEventInChannel(ctx.Channel.Id) is LotteryGame)
                        await JoinAsync(ctx, numbers).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                long? balance = await Database.GetBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (!balance.HasValue || balance < LotteryGame.TicketPrice)
                    throw new CommandFailedException($"You do not have enough credits on your account to buy a lottery ticket! The lottery ticket costs {LotteryGame.TicketPrice} credits!");

                var game = new LotteryGame(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Shared.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.InformSuccessAsync($"The Lottery game will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("casino lottery")} to join the pool.", ":clock1:")
                        .ConfigureAwait(false);
                    await JoinAsync(ctx, numbers)
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                    await game.RunAsync()
                        .ConfigureAwait(false);

                    if (game.Winners.Any()) {
                        await ctx.InformSuccessAsync(StaticDiscordEmoji.MoneyBag, $"Winnings:\n\n{string.Join(", ", game.Winners.Select(w => $"{w.User.Mention} : {w.WinAmount}"))}")
                            .ConfigureAwait(false);
                        foreach (var winner in game.Winners)
                            await Database.IncreaseBankAccountBalanceAsync(winner.Id, ctx.Guild.Id, winner.WinAmount)
                                .ConfigureAwait(false);
                    } else {
                        await ctx.InformSuccessAsync(StaticDiscordEmoji.MoneyBag, "Better luck next time!")
                            .ConfigureAwait(false);
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_LOTTERY_JOIN
            [Command("join"), Module(ModuleType.Currency)]
            [Description("Join a pending Lottery game.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExamples("!casino lottery join 2 10 8")]
            public async Task JoinAsync(CommandContext ctx,
                                       [RemainingText, Description("Three numbers.")] params int[] numbers)
            {
                if (numbers.Length != 3)
                    throw new CommandFailedException("You need to specify three numbers!");

                if (numbers.Any(n => n < 1 || n > LotteryGame.MaxNumber))
                    throw new CommandFailedException($"Invalid number given! Numbers must be in range [1, {LotteryGame.MaxNumber}]!");

                if (!(this.Shared.GetEventInChannel(ctx.Channel.Id) is LotteryGame game))
                    throw new CommandFailedException("There are no Lottery games running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Lottery game has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Lottery slots are full (max 10 participants), kthxbye.");

                if (game.IsParticipating(ctx.User))
                    throw new CommandFailedException("You are already participating in the Lottery game!");

                if (!await Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, LotteryGame.TicketPrice))
                    throw new CommandFailedException($"You do not have enough credits on your account to buy a lottery ticket! The lottery ticket costs {LotteryGame.TicketPrice} credits!");

                game.AddParticipant(ctx.User, numbers);
                await ctx.InformSuccessAsync(StaticDiscordEmoji.MoneyBag, $"{ctx.User.Mention} joined the Lottery game.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_LOTTERY_RULES
            [Command("rules"), Module(ModuleType.Currency)]
            [Description("Explain the Lottery rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!casino lottery rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.InformSuccessAsync(
                    "TODO",
                    ":information_source:"
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
