#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;
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
    public partial class CasinoModule
    {
        [Group("blackjack")]
        [Description("Play a blackjack game.")]
        [Aliases("bj")]

        public class BlackjackModule : TheGodfatherServiceModule<ChannelEventService>
        {

            public BlackjackModule(ChannelEventService service, DbContextBuilder db)
                : base(service, db)
            {

            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Bid amount.")] int bid = 5)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Service.GetEventInChannel(ctx.Channel.Id) is BlackjackGame)
                        await this.JoinAsync(ctx);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var game = new BlackjackGame(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, Emojis.Clock1, $"The Blackjack game will start in 30s or when there are 5 participants. Use command {Formatter.InlineCode("casino blackjack <bid>")} to join the pool. Default bid is 5 {ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency}.");
                    await this.JoinAsync(ctx, bid);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (game.ParticipantCount > 0) {
                        await game.RunAsync();

                        if (game.Winners.Any()) {
                            if (game.Winner is null) {
                                await this.InformAsync(ctx, Emojis.Cards.Suits[0], $"Winners:\n\n{string.Join(", ", game.Winners.Select(w => w.User.Mention))}");

                                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                                    foreach (BlackjackGame.Participant winner in game.Winners)
                                        await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + winner.Bid * 2);
                                    await db.SaveChangesAsync();
                                }
                            } else {
                                await this.InformAsync(ctx, Emojis.Cards.Suits[0], $"{game.Winner.Mention} got the BlackJack!");
                                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                                    await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + game.Winners.First(p => p.Id == game.Winner.Id).Bid * 2);
                                    await db.SaveChangesAsync();
                                }
                            }
                        } else {
                            await this.InformAsync(ctx, Emojis.Cards.Suits[0], "The House always wins!");
                        }
                    } else {
                        if (game.IsParticipating(ctx.User)) {
                            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + bid);
                                await db.SaveChangesAsync();
                            }
                        }
                        await this.InformAsync(ctx, Emojis.AlarmClock, "Not enough users joined the Blackjack game.");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_BLACKJACK_JOIN
            [Command("join")]
            [Description("Join a pending Blackjack game.")]
            [Aliases("+", "compete", "enter", "j", "<<", "<")]

            public async Task JoinAsync(CommandContext ctx,
                                       [Description("Bid amount.")] int bid = 5)
            {
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out BlackjackGame game))
                    throw new CommandFailedException("There are no Blackjack games running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Blackjack game has already started, you can't join it.");

                if (game.ParticipantCount >= 5)
                    throw new CommandFailedException("Blackjack slots are full (max 5 participants), kthxbye.");

                if (game.IsParticipating(ctx.User))
                    throw new CommandFailedException("You are already participating in the Blackjack game!");

                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    if (bid <= 0 || !await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, bid))
                        throw new CommandFailedException($"You do not have enough {ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency}! Use command {Formatter.InlineCode("bank")} to check your account status.");
                    await db.SaveChangesAsync();
                }

                game.AddParticipant(ctx.User, bid);
                await this.InformAsync(ctx, Emojis.Cards.Suits[0], $"{ctx.User.Mention} joined the Blackjack game.");
            }
            #endregion

            #region COMMAND_BLACKJACK_RULES
            [Command("rules")]
            [Description("Explain the Blackjack rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
            {
                return this.InformAsync(ctx,
                    Emojis.Information,
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
