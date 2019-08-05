#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Services;
using TheGodfather.Modules.Administration.Services;
#endregion

namespace TheGodfather.Modules.Currency
{
    public partial class CasinoModule
    {
        [Group("lottery")]
        [Description("Play a lottery game. The three numbers are drawn from 1 to 15 and they can't be repeated.")]
        [Aliases("lotto")]
        
        public class LotteryModule : TheGodfatherServiceModule<ChannelEventService>
        {

            public LotteryModule(ChannelEventService service, SharedData shared, DatabaseContextBuilder db)
                : base(service, shared, db)
            {
                
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [RemainingText, Description("Three numbers.")] params int[] numbers)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Service.GetEventInChannel(ctx.Channel.Id) is LotteryGame)
                        await this.JoinAsync(ctx, numbers);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var game = new LotteryGame(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, StaticDiscordEmoji.Clock1, $"The Lottery game will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("casino lottery")} to join the pool.");
                    await this.JoinAsync(ctx, numbers);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (game.ParticipantCount > 1) {
                        await game.RunAsync();

                        if (game.Winners.Any()) {
                            await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, $"Winnings:\n\n{string.Join(", ", game.Winners.Select(w => $"{w.User.Mention} : {w.WinAmount}"))}");

                            using (DatabaseContext db = this.Database.CreateContext()) {
                                foreach (LotteryGame.Participant winner in game.Winners)
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
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_LOTTERY_JOIN
            [Command("join")]
            [Description("Join a pending Lottery game.")]
            [Aliases("+", "compete", "enter", "j", "<<", "<")]
            
            public async Task JoinAsync(CommandContext ctx,
                                       [RemainingText, Description("Three numbers.")] params int[] numbers)
            {
                if (numbers is null || numbers.Length != 3)
                    throw new CommandFailedException("You need to specify three numbers!");

                if (numbers.Any(n => n < 1 || n > LotteryGame.MaxNumber))
                    throw new CommandFailedException($"Invalid number given! Numbers must be in range [1, {LotteryGame.MaxNumber}]!");

                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out LotteryGame game))
                    throw new CommandFailedException("There are no Lottery games running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Lottery game has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Lottery slots are full (max 10 participants), kthxbye.");

                if (game.IsParticipating(ctx.User))
                    throw new CommandFailedException("You are already participating in the Lottery game!");

                using (DatabaseContext db = this.Database.CreateContext()) {
                    if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, LotteryGame.TicketPrice))
                        throw new CommandFailedException($"You do not have enough {ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency} to buy a lottery ticket! Use command {Formatter.InlineCode("bank")} to check your account status. The lottery ticket costs {LotteryGame.TicketPrice} {ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency}!");
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
