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
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Currency
{
    public partial class CasinoModule
    {
        [Group("blackjack"), Module(ModuleType.Currency)]
        [Description("Play a blackjack game.")]
        [Aliases("bj")]
        [UsageExample("!casino blackjack")]
        public class BlackjackModule : TheGodfatherBaseModule
        {

            public BlackjackModule(DBService db) : base(db: db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Bid amount.")] int bid = 5)
            {
                if (ChannelEvent.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is BlackjackGame)
                        await JoinAsync(ctx).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                long? balance = await Database.GetUserCreditAmountAsync(ctx.User.Id)
                    .ConfigureAwait(false);
                if (!balance.HasValue || balance < bid)
                    throw new CommandFailedException("You do not have that many credits on your account! Specify a smaller bid amount.");

                var game = new BlackjackGame(ctx.Client.GetInteractivity(), ctx.Channel);
                ChannelEvent.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.RespondWithIconEmbedAsync($"The Blackjack game will start in 30s or when there are 5 participants. Use command {Formatter.InlineCode("casino blackjack <bid>")} to join the pool. Default bid is 5 credits.", ":clock1:")
                        .ConfigureAwait(false);
                    await JoinAsync(ctx, bid)
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                    await game.RunAsync()
                        .ConfigureAwait(false);

                    if (game.Winners.Any()) {
                        if (game.Winner != null) {
                            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.CardSuits[0], $"{game.Winner.Mention} got the BlackJack!")
                                .ConfigureAwait(false);
                            await Database.GiveCreditsToUserAsync(game.Winner.Id, game.Winners.First(p => p.Id == game.Winner.Id).Bid)
                                    .ConfigureAwait(false);
                        } else {
                            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.CardSuits[0], $"Winners:\n\n{string.Join(", ", game.Winners.Select(w => w.User.Mention))}")
                                .ConfigureAwait(false);

                            foreach (var winner in game.Winners)
                                await Database.GiveCreditsToUserAsync(winner.Id, winner.Bid * 2)
                                    .ConfigureAwait(false);
                        }
                    } else {
                        await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.CardSuits[0], "The House always wins!")
                            .ConfigureAwait(false);
                    }
                } finally {
                    ChannelEvent.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_BLACKJACK_JOIN
            [Command("join"), Module(ModuleType.Currency)]
            [Description("Join a pending Blackjack game.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExample("!casino blackjack join")]
            public async Task JoinAsync(CommandContext ctx,
                                       [Description("Bid amount.")] int bid = 5)
            {
                if (!(ChannelEvent.GetEventInChannel(ctx.Channel.Id) is BlackjackGame game))
                    throw new CommandFailedException("There are no Blackjack games running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Blackjack game has already started, you can't join it.");

                if (game.ParticipantCount >= 5)
                    throw new CommandFailedException("Blackjack slots are full (max 5 participants), kthxbye.");
                
                if (game.IsParticipating(ctx.User))
                    throw new CommandFailedException("You are already participating in the Blackjack game!");

                if (bid <= 0 || !await Database.TakeCreditsFromUserAsync(ctx.User.Id, bid))
                    throw new CommandFailedException("You do not have that many credits on your account! Specify a smaller bid amount.");

                game.AddParticipant(ctx.User, bid);
                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.CardSuits[0], $"{ctx.User.Mention} joined the Blackjack game.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_BLACKJACK_RULES
            [Command("rules"), Module(ModuleType.Currency)]
            [Description("Explain the Blackjack rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            [UsageExample("!casino blackjack rules")]
            public async Task RulesAsync(CommandContext ctx)
            {
                await ctx.RespondWithIconEmbedAsync(
                    "Each participant attempts to beat the dealer by getting a count as close to 21 as possible, without going over 21. " +
                    "It is up to each individual player if an ace is worth 1 or 11. Face cards are 10 and any other card is its pip value. " +
                    "Each player is dealt two cards in the begining and in turns they decide whether to hit (get one more card dealt) or stand. " +
                    "After all players with sums smaller or equal to 21 decide to stand, the House does the same. Whoever beats the house gets the reward.",
                    ":information_source:"
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
