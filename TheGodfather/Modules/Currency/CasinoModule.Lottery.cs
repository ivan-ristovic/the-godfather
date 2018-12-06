#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Currency.Extensions;
#endregion

namespace TheGodfather.Modules.Currency
{
    public partial class CasinoModule
    {
        [Group("lottery")]
        [Description("Play a lottery game. The three numbers are drawn from 1 to 15 and they can't be repeated.")]
        [Aliases("lotto")]
        [UsageExamples("!casino lottery 2 10 8")]
        public class LotteryModule : TheGodfatherModule
        {

            public LotteryModule(SharedData shared, DatabaseContextBuilder db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.SapGreen;
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [RemainingText, Description("Three numbers.")] params int[] numbers)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Shared.GetEventInChannel(ctx.Channel.Id) is LotteryGame)
                        await this.JoinAsync(ctx, numbers);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var game = new LotteryGame(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Shared.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, StaticDiscordEmoji.Clock1, $"The Lottery game will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("casino lottery")} to join the pool.");
                    await this.JoinAsync(ctx, numbers);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync();

                        if (game.Winners.Any()) {
                            await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, $"Winnings:\n\n{string.Join(", ", game.Winners.Select(w => $"{w.User.Mention} : {w.WinAmount}"))}");

                            using (DatabaseContext db = this.Database.CreateContext()) {
                                foreach (var winner in game.Winners)
                                    await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + winner.WinAmount);
                                await db.SaveChangesAsync();
                            }
                        } else {
                            await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, "Better luck next time!");
                        }
                    } else {
                        if (game.IsParticipating(ctx.User)) {
                            using (DatabaseContext db = this.Database.CreateContext()) {
                                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + LotteryGame.TicketPrice);
                                await db.SaveChangesAsync();
                            }
                        }
                        await this.InformAsync(ctx, StaticDiscordEmoji.AlarmClock, "Not enough users joined the Blackjack game.");
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_LOTTERY_JOIN
            [Command("join")]
            [Description("Join a pending Lottery game.")]
            [Aliases("+", "compete", "enter", "j", "<<", "<")]
            [UsageExamples("!casino lottery join 2 10 8")]
            public async Task JoinAsync(CommandContext ctx,
                                       [RemainingText, Description("Three numbers.")] params int[] numbers)
            {
                if (numbers is null || numbers.Length != 3)
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

                using (DatabaseContext db = this.Database.CreateContext()) {
                    if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, LotteryGame.TicketPrice))
                        throw new CommandFailedException($"You do not have enough {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"} to buy a lottery ticket! Use command {Formatter.InlineCode("bank")} to check your account status. The lottery ticket costs {LotteryGame.TicketPrice} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}!");
                    await db.SaveChangesAsync();
                }

                game.AddParticipant(ctx.User, numbers);
                await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, $"{ctx.User.Mention} joined the Lottery game.");
            }
            #endregion

            #region COMMAND_LOTTERY_RULES
            [Command("rules")]
            [Description("Explain the Lottery rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!casino lottery rules")]
            public Task RulesAsync(CommandContext ctx)
            {
                return this.InformAsync(ctx,
                    StaticDiscordEmoji.Information,
                    "Three numbers will be drawn, and rewards will be given to participants depending on " +
                    "the number of correct guesses."
                );
            }
            #endregion
        }
    }
}
