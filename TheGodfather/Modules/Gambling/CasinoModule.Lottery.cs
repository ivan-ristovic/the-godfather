#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Gambling.Common;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Gambling
{
    public partial class CasinoModule
    {
        [Group("lottery"), Module(ModuleType.Gambling)]
        [Description("Play a lottery game.")]
        [Aliases("lotto")]
        [UsageExample("!casino lottery")]
        public class LotteryModule : TheGodfatherBaseModule
        {

            public LotteryModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Number 1.")] int num1,
                                               [Description("Number 2.")] int num2)
            {
                if (Game.RunningInChannel(ctx.Channel.Id)) {
                    if (Game.GetGameInChannel(ctx.Channel.Id) is LotteryGame)
                        await JoinAsync(ctx, num1, num2).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another game is already running in the current channel.");
                    return;
                }

                int? balance = await Database.GetUserCreditAmountAsync(ctx.User.Id)
                    .ConfigureAwait(false);
                if (!balance.HasValue || balance < 250)
                    throw new CommandFailedException("You do not have enough credits to buy a lottery ticket! (250 needed)");

                var game = new LotteryGame(ctx.Client.GetInteractivity(), ctx.Channel);
                Game.RegisterGameInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.RespondWithIconEmbedAsync($"The Lottery game will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("casino lottery")} to join the pool.", ":clock1:")
                        .ConfigureAwait(false);
                    await JoinAsync(ctx, num1, num2)
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                    await game.RunAsync()
                        .ConfigureAwait(false);

                    if (game.Winners.Any()) {
                        await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.MoneyBag, $"Winnings:\n\n{string.Join(", ", game.Winners.Select(w => $"{w.User.Mention} : {w.WinAmount}"))}")
                            .ConfigureAwait(false);
                        foreach (var winner in game.Winners)
                            await Database.GiveCreditsToUserAsync(winner.Id, winner.WinAmount)
                                .ConfigureAwait(false);
                    } else {
                        await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.MoneyBag, "Better luck next time!")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_LOTTERY_JOIN
            [Command("join"), Module(ModuleType.Gambling)]
            [Description("Join a pending Blackjack game.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExample("!casino blackjack join")]
            public async Task JoinAsync(CommandContext ctx,
                                       [Description("Number 1.")] int num1,
                                       [Description("Number 2.")] int num2)
            {
                if (num1 < 1 || num1 > LotteryGame.MaxNumber || num2 < 1 || num2 > LotteryGame.MaxNumber)
                    throw new CommandFailedException("Invalid numbers!");

                var game = Game.GetGameInChannel(ctx.Channel.Id) as LotteryGame;
                if (game == null)
                    throw new CommandFailedException("There are no Lottery games running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Lottery game has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Lottery slots are full (max 10 participants), kthxbye.");

                if (game.IsParticipating(ctx.User))
                    throw new CommandFailedException("You are already participating in the Lottery game!");

                if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, 250))
                    throw new CommandFailedException("You do not have 100 on your account! The lottery ticket costs 250 credits!");

                game.AddParticipant(ctx.User, num1, num2);
                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.MoneyBag, $"{ctx.User.Mention} joined the Lottery game.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_LOTTERY_RULES
            [Command("rules"), Module(ModuleType.Gambling)]
            [Description("Explain the Lottery rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExample("!casino lottery rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.RespondWithIconEmbedAsync(
                    "One correct number gives you 10% of your current bank balance, whereas both numbers give you 50% increase.",
                    ":information_source:"
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
