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
using TheGodfather.Exceptions;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Currency
{
    public partial class CasinoModule
    {
        [Group("blackjack")]
        [Description("Play a blackjack game.")]
        [Aliases("bj")]
        [UsageExamples("!casino blackjack")]
        public class BlackjackModule : TheGodfatherModule
        {

            public BlackjackModule(SharedData shared, DBService db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.SapGreen;
            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Bid amount.")] int bid = 5)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Shared.GetEventInChannel(ctx.Channel.Id) is BlackjackGame)
                        await JoinAsync(ctx);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                long? balance = await this.Database.GetBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id);
                if (!balance.HasValue || balance < bid)
                    throw new CommandFailedException($"You do not have enough {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}! Use command {Formatter.InlineCode("bank")} to check your account status.");

                var game = new BlackjackGame(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Shared.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await InformAsync(ctx, StaticDiscordEmoji.Clock1, $"The Blackjack game will start in 30s or when there are 5 participants. Use command {Formatter.InlineCode("casino blackjack <bid>")} to join the pool. Default bid is 5 {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}.");
                    await JoinAsync(ctx, bid);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    await game.RunAsync();

                    if (game.Winners.Any()) {
                        if (game.Winner != null) {
                            await InformAsync(ctx, StaticDiscordEmoji.CardSuits[0], $"{game.Winner.Mention} got the BlackJack!");
                            await this.Database.IncreaseBankAccountBalanceAsync(game.Winner.Id, ctx.Guild.Id, game.Winners.First(p => p.Id == game.Winner.Id).Bid);
                        } else {
                            await InformAsync(ctx, StaticDiscordEmoji.CardSuits[0], $"Winners:\n\n{string.Join(", ", game.Winners.Select(w => w.User.Mention))}");

                            foreach (var winner in game.Winners)
                                await this.Database.IncreaseBankAccountBalanceAsync(winner.Id, ctx.Guild.Id, winner.Bid * 2);
                        }
                    } else {
                        await InformAsync(ctx, StaticDiscordEmoji.CardSuits[0], "The House always wins!");
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_BLACKJACK_JOIN
            [Command("join")]
            [Description("Join a pending Blackjack game.")]
            [Aliases("+", "compete", "enter", "j", "<<", "<")]
            [UsageExamples("!casino blackjack join")]
            public async Task JoinAsync(CommandContext ctx,
                                       [Description("Bid amount.")] int bid = 5)
            {
                if (!(this.Shared.GetEventInChannel(ctx.Channel.Id) is BlackjackGame game))
                    throw new CommandFailedException("There are no Blackjack games running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Blackjack game has already started, you can't join it.");

                if (game.ParticipantCount >= 5)
                    throw new CommandFailedException("Blackjack slots are full (max 5 participants), kthxbye.");
                
                if (game.IsParticipating(ctx.User))
                    throw new CommandFailedException("You are already participating in the Blackjack game!");

                if (bid <= 0 || !await this.Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, bid))
                    throw new CommandFailedException($"You do not have enough {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}! Use command {Formatter.InlineCode("bank")} to check your account status.");

                game.AddParticipant(ctx.User, bid);
                await InformAsync(ctx, StaticDiscordEmoji.CardSuits[0], $"{ctx.User.Mention} joined the Blackjack game.");
            }
            #endregion

            #region COMMAND_BLACKJACK_RULES
            [Command("rules")]
            [Description("Explain the Blackjack rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExamples("!casino blackjack rules")]
            public Task RulesAsync(CommandContext ctx)
            {
                return InformAsync(ctx,
                    StaticDiscordEmoji.Information,
                    "Each participant attempts to beat the dealer by getting a card value sum as close to 21 as possible, without going over 21. " +
                    "It is up to each individual player if an ace is worth 1 or 11. Face cards are valued as 10 and any other card is its pip value. " +
                    "Each player is dealt two cards in the begining and in turns they decide whether to hit (get one more card dealt) or stand. " +
                    "After all players with sums smaller or equal to 21 decide to stand, the House does the same. Whoever beats the house gets the reward."
                );
            }
            #endregion
        }
    }
}
